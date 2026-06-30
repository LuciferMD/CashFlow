namespace Gateway.Models;

public class KafkaOptions
{
    public string Brokers { get; set; } = "localhost:9092";

    public KafkaTopicOptions Topic { get; set; } = new();
}

public class KafkaTopicOptions
{
    public string IotSnapshots { get; set; } = "iot.snapshots";
}
