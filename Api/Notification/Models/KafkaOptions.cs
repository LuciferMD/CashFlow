namespace Notification.Models;

public sealed class KafkaOptions
{
    public string Brokers { get; set; } = "localhost:9092";
    public string GroupId { get; set; } = "notification-service";
    public KafkaTopicOptions Topic { get; set; } = new();
}

public sealed class KafkaTopicOptions
{
    public string IotSnapshots { get; set; } = "iot.snapshots";
}
