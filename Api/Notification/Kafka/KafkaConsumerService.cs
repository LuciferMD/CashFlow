using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Notification.Models;
using Notification.Services;

namespace Notification.Kafka;

public sealed class KafkaConsumerService : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly KafkaOptions _kafka;
    private readonly ISnapshotProcessor _processor;
    private readonly ILogger<KafkaConsumerService> _logger;

    public KafkaConsumerService(
        IOptions<KafkaOptions> kafka,
        ISnapshotProcessor processor,
        ILogger<KafkaConsumerService> logger)
    {
        _kafka = kafka.Value;
        _processor = processor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Yield immediately so the host can finish starting Kestrel before we
        // enter the blocking Confluent.Kafka consume loop.
        await Task.Yield();

        _logger.LogInformation(
            "[kafka] Starting consumer. Brokers={Brokers} Topic={Topic} Group={Group}",
            _kafka.Brokers, _kafka.Topic.IotSnapshots, _kafka.GroupId);

        var config = new ConsumerConfig
        {
            BootstrapServers = _kafka.Brokers,
            GroupId = _kafka.GroupId,
            AutoOffsetReset = AutoOffsetReset.Latest,
            EnableAutoCommit = true,
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_kafka.Topic.IotSnapshots);

        _logger.LogInformation("[kafka] Subscribed to topic \"{Topic}\"", _kafka.Topic.IotSnapshots);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? result;

                try
                {
                    result = consumer.Consume(TimeSpan.FromSeconds(1));
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "[kafka] Consume error: {Reason}", ex.Error.Reason);
                    continue;
                }

                if (result is null) continue;

                try
                {
                    var snapshot = JsonSerializer.Deserialize<IotSnapshotMessage>(
                        result.Message.Value, JsonOptions)
                        ?? throw new InvalidOperationException("Null deserialization result.");

                    await _processor.ProcessAsync(snapshot, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[kafka] Failed to process message.");
                }
            }
        }
        finally
        {
            consumer.Close();
            _logger.LogInformation("[kafka] Consumer stopped.");
        }
    }
}
