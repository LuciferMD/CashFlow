using System.Text.Json;
using FluentAssertions;

namespace GatewayTests.Service;

/// <summary>
/// Service-level tests for the Gateway GraphQL API.
/// Uses WebApplicationFactory + Testcontainers Kafka + WireMock (IoT upstream).
/// Only the Gateway service is exercised — no Auth, Notification, or HistoryStore.
/// </summary>
[Collection(nameof(GatewayServiceCollection))]
[Trait("Category", "Service")]
public sealed class GatewayServiceTests
{
    private readonly GatewayServiceFixture _fixture;

    public GatewayServiceTests(GatewayServiceFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetIot_WithoutAuth_ReturnsGraphQlUnauthorizedError()
    {
        var response = await ServiceTestHelpers.PostGraphQlAsync(_fixture.Factory);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        using var doc = await ServiceTestHelpers.ReadJsonAsync(response);
        ServiceTestHelpers.AssertGraphQlUnauthorized(doc);
    }

    [Fact]
    public async Task GetIot_WithValidJwt_ReturnsDevicesFromStub()
    {
        await _fixture.ConfigureIotStubAsync(ServiceTestHelpers.SingleDeviceJson);
        var jwt = _fixture.CreateJwt();

        var response = await ServiceTestHelpers.PostGraphQlAsync(_fixture.Factory, jwt);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        using var doc = await ServiceTestHelpers.ReadJsonAsync(response);
        var deviceName = doc.RootElement
            .GetProperty("data")
            .GetProperty("iot")
            .GetProperty("devices")[0]
            .GetProperty("name")
            .GetString();

        deviceName.Should().Be("Kitchen");
    }

    [Fact]
    public async Task GetIot_WhenUpstreamReturnsData_PublishesToKafka()
    {
        await _fixture.ConfigureIotStubAsync(ServiceTestHelpers.SingleDeviceJson);
        var jwt = _fixture.CreateJwt();

        var kafkaMessage = await ServiceTestHelpers.ConsumeKafkaMessageAfterAsync(
            _fixture.BootstrapServers,
            "iot.snapshots",
            async () =>
            {
                var response = await ServiceTestHelpers.PostGraphQlAsync(_fixture.Factory, jwt);
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            },
            TimeSpan.FromSeconds(20));

        kafkaMessage.Should().NotBeNullOrEmpty();
        kafkaMessage.Should().Contain("Kitchen");
    }

    [Fact]
    public async Task GetIot_WithInvalidJwt_ReturnsGraphQlUnauthorizedError()
    {
        var response = await ServiceTestHelpers.PostGraphQlAsync(_fixture.Factory, "not-a-valid-jwt");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        using var doc = await ServiceTestHelpers.ReadJsonAsync(response);
        ServiceTestHelpers.AssertGraphQlUnauthorized(doc);
    }

    [Fact(Timeout = 120_000)]
    public async Task GetIot_WhenUpstreamReturns500_ReturnsEmptyDevices()
    {
        await _fixture.ConfigureIotStubAsync("upstream failure", statusCode: 500);
        var jwt = _fixture.CreateJwt();

        var response = await ServiceTestHelpers.PostGraphQlAsync(_fixture.Factory, jwt);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        using var doc = await ServiceTestHelpers.ReadJsonAsync(response);
        doc.RootElement
            .GetProperty("data")
            .GetProperty("iot")
            .GetProperty("devices")
            .GetArrayLength()
            .Should()
            .Be(0);
    }
}
