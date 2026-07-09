using System.Text.Json.Serialization;

namespace Notification.Kafka;

public sealed record IotSnapshotMessage(
    DateTime CapturedAt,
    List<IotDevice> Devices);

public sealed record IotDevice(
    string Type,
    string Name,
    DevicePayload? Payload);

public sealed record DevicePayload(
    [property: JsonPropertyName("co2")]      double? Co2,
    [property: JsonPropertyName("pm25")]     double? Pm25,
    [property: JsonPropertyName("humidity")] double? Humidity,
    [property: JsonPropertyName("energy")]   double? Energy);
