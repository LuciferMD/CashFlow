using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;

namespace NotificationTests.Service;

/// <summary>
/// Service-level tests for the Notification API.
/// Uses WebApplicationFactory (in-process host) + Testcontainers Kafka.
/// Only the Notification service is exercised — no Auth, Gateway, or HistoryStore.
/// </summary>
[Collection(nameof(NotificationServiceCollection))]
[Trait("Category", "Service")]
public sealed class NotificationServiceTests
{
    private readonly NotificationServiceFixture _fixture;

    public NotificationServiceTests(NotificationServiceFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var client = _fixture.Factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task TestSnapshot_WithValidBody_ReturnsProcessedTrue()
    {
        var client = _fixture.Factory.CreateClient();
        var body = ServiceTestHelpers.BuildSnapshot(humidity: 45);

        var response = await client.PostAsync(
            "/test/snapshot",
            ServiceTestHelpers.ToJsonContent(body));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("processed").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("deviceCount").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task TestSnapshot_BroadcastsIotSnapshotToSignalRClient()
    {
        await using var connection = await ServiceTestHelpers.ConnectHubAsync(_fixture.Factory);
        var received = ServiceTestHelpers.WaitForHubEventAsync<JsonElement>(
            connection,
            "IotSnapshot",
            TimeSpan.FromSeconds(10));

        var client = _fixture.Factory.CreateClient();
        var body = ServiceTestHelpers.BuildSnapshot(humidity: 45, deviceName: "Living Room");

        await client.PostAsync("/test/snapshot", ServiceTestHelpers.ToJsonContent(body));

        var payload = await received;
        payload.GetProperty("devices")[0].GetProperty("name").GetString()
            .Should().Be("Living Room");
    }

    [Fact]
    public async Task TestSnapshot_HighHumidity_BroadcastsHumidityAlertToSignalRClient()
    {
        await using var connection = await ServiceTestHelpers.ConnectHubAsync(_fixture.Factory);
        var received = ServiceTestHelpers.WaitForHubEventAsync<JsonElement>(
            connection,
            "HumidityAlert",
            TimeSpan.FromSeconds(10));

        var client = _fixture.Factory.CreateClient();
        var body = ServiceTestHelpers.BuildSnapshot(humidity: 85, deviceName: "Bathroom");

        await client.PostAsync("/test/snapshot", ServiceTestHelpers.ToJsonContent(body));

        var alert = await received;
        alert.GetProperty("deviceName").GetString().Should().Be("Bathroom");
        alert.GetProperty("humidity").GetDouble().Should().Be(85);
        alert.GetProperty("threshold").GetDouble().Should().Be(60);
    }

    [Fact]
    public async Task KafkaConsumer_WhenSnapshotProduced_BroadcastsIotSnapshotToSignalRClient()
    {
        await using var connection = await ServiceTestHelpers.ConnectHubAsync(_fixture.Factory);
        var received = ServiceTestHelpers.WaitForHubEventAsync<JsonElement>(
            connection,
            "IotSnapshot",
            TimeSpan.FromSeconds(20));

        var snapshot = ServiceTestHelpers.BuildSnapshot(humidity: 50, deviceName: "Office");
        await ServiceTestHelpers.ProduceKafkaMessageAsync(
            _fixture.BootstrapServers,
            snapshot);

        var payload = await received;
        payload.GetProperty("devices")[0].GetProperty("name").GetString()
            .Should().Be("Office");
    }
}
