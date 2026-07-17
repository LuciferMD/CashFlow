using System.Text.Json;
using System.Text.Json.Serialization;
using Gateway.Types;

namespace GatewayTests.Contract;

internal static class ContractTestHelpers
{
    internal static string PactsDirectory =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "pacts"));

    internal static string NotificationGatewayPactPath =>
        Path.Combine(PactsDirectory, "Notification-Gateway.json");

    internal static string HistoryStoreGatewayPactPath =>
        Path.Combine(PactsDirectory, "HistoryStore-Gateway.json");

    internal static string WebGatewayPactPath =>
        Path.Combine(PactsDirectory, "Web-Gateway.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Builds the Kafka message body Gateway publishes via <see cref="Gateway.Services.IotSnapshotPublisher"/>.
    /// </summary>
    internal static object CreateIotSnapshotMessage() =>
        new
        {
            capturedAt = new DateTime(2024, 6, 15, 10, 0, 0, DateTimeKind.Utc),
            devices = new List<IotDevice>
            {
                new()
                {
                    Type = "sensor",
                    Name = "Kitchen",
                    Payload = new IotPayload
                    {
                        Co2 = 400,
                        Pm25 = 10,
                        Humidity = 60,
                        Energy = 1.5,
                    },
                },
            },
        };

    internal static string SerializeIotSnapshotMessage(object message) =>
        JsonSerializer.Serialize(message, JsonOptions);
}
