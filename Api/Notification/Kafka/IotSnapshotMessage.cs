using System.Text.Json.Serialization;

namespace Notification.Kafka;

public sealed record IotSnapshotMessage(
    [property: JsonPropertyName("capturedAt")] DateTime CapturedAt,
    [property: JsonPropertyName("devices")] List<IotDevice> Devices);

public sealed record IotDevice(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("payload")] DevicePayload? Payload);

public sealed record DevicePayload(
    [property: JsonPropertyName("co2")]      double? Co2,
    [property: JsonPropertyName("pm25")]     double? Pm25,
    [property: JsonPropertyName("humidity")] double? Humidity,
    [property: JsonPropertyName("energy")]   double? Energy);
