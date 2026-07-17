using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using Gateway.Services;

namespace GatewayTests.Unit;

/// <summary>
/// Unit tests for HttpIotClient covering the happy path only.
/// Failure / retry paths (429, 500, empty array, object payload) are covered
/// at the integration level because the Polly pipeline's exponential back-off
/// makes them unsuitable as unit tests.
/// </summary>
public sealed class HttpIotClientTests
{
    private static HttpIotClient CreateClient(
        string responseJson,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Iot:BaseUrl"] = "http://iot-service.test/",
                ["Iot:ApiKey"]  = "test-api-key"
            })
            .Build();

        return new HttpIotClient(new HttpClient(handler.Object), config, NullLogger<HttpIotClient>.Instance);
    }

    private const string SingleDeviceJson = """
        [{"type":"sensor","name":"room-1","payload":{"co2":400,"pm25":10,"humidity":60,"energy":1.5}}]
        """;

    private const string ThreeDevicesJson = """
        [
          {"type":"sensor","name":"room-1","payload":{"co2":400,"pm25":10,"humidity":60,"energy":1.5}},
          {"type":"actuator","name":"room-2","payload":{"co2":350,"pm25":5,"humidity":45,"energy":2.0}},
          {"type":"sensor","name":"room-3","payload":{"co2":500,"pm25":20,"humidity":70,"energy":0.8}}
        ]
        """;

    // ── Happy path ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMetersAsync_WithValidArray_ReturnsNonNullIot()
    {
        var client = CreateClient(SingleDeviceJson);

        var result = await client.GetMetersAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMetersAsync_WithSingleDevice_ReturnsOneDevice()
    {
        var client = CreateClient(SingleDeviceJson);

        var result = await client.GetMetersAsync();

        result.Devices.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetMetersAsync_WithValidArray_ParsesDeviceName()
    {
        var client = CreateClient(SingleDeviceJson);

        var result = await client.GetMetersAsync();

        result.Devices[0].Name.Should().Be("room-1");
    }

    [Fact]
    public async Task GetMetersAsync_WithValidArray_ParsesDeviceType()
    {
        var client = CreateClient(SingleDeviceJson);

        var result = await client.GetMetersAsync();

        result.Devices[0].Type.Should().Be("sensor");
    }

    [Fact]
    public async Task GetMetersAsync_WithValidArray_ParsesPayloadFields()
    {
        var client = CreateClient(SingleDeviceJson);

        var result = await client.GetMetersAsync();

        var payload = result.Devices[0].Payload;
        payload.Co2.Should().Be(400);
        payload.Pm25.Should().Be(10);
        payload.Humidity.Should().Be(60);
        payload.Energy.Should().Be(1.5);
    }

    [Fact]
    public async Task GetMetersAsync_WithThreeDevices_ReturnsAllThree()
    {
        var client = CreateClient(ThreeDevicesJson);

        var result = await client.GetMetersAsync();

        result.Devices.Should().HaveCount(3);
    }
}
