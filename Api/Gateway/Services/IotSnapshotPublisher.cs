using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;
using Gateway.Models;
using Gateway.Types;
using Microsoft.Extensions.Options;

namespace Gateway.Services;

public sealed class IotSnapshotPublisher : IIotSnapshotPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private readonly ILogger<IotSnapshotPublisher> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public IotSnapshotPublisher(
        IProducer<string, string> producer,
        IOptions<KafkaOptions> options,
        ILogger<IotSnapshotPublisher> logger)
    {
        _producer = producer;
        _topic = options.Value.Topic.IotSnapshots;
        _logger = logger;
    }

    public async Task PublishAsync(Iot iot, CancellationToken cancellationToken = default)
    {
        if (iot.Devices.Count == 0)
            return;

        var message = new IotSnapshotMessage(DateTime.UtcNow, iot.Devices);
        var json = JsonSerializer.Serialize(message, JsonOptions);

        try
        {
            var result = await _producer.ProduceAsync(
                _topic,
                new Message<string, string>
                {
                    Key = message.CapturedAt.ToString("yyyy-MM-dd"),
                    Value = json
                },
                cancellationToken);

            _logger.LogInformation(
                "Published IoT snapshot to {Topic} partition {Partition} offset {Offset}",
                result.Topic,
                result.Partition.Value,
                result.Offset.Value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish IoT snapshot to Kafka topic {Topic}", _topic);
        }
    }

    public void Dispose() => _producer.Dispose();

    private sealed record IotSnapshotMessage(DateTime CapturedAt, List<IotDevice> Devices);
}
