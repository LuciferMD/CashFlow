using System.Text.Json;
using Confluent.Kafka;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Gateway.Models;
using Gateway.Services;
using Gateway.Types;

namespace GatewayTests.Unit;

public sealed class IotSnapshotPublisherTests
{
    private const string DefaultTopic = "iot.snapshots";

    private readonly Mock<IProducer<string, string>> _mockProducer;
    private readonly IotSnapshotPublisher _sut;

    public IotSnapshotPublisherTests()
    {
        _mockProducer = new Mock<IProducer<string, string>>();
        _mockProducer
            .Setup(p => p.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<Message<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeliveryResult<string, string>
            {
                Topic = DefaultTopic,
                Partition = new Partition(0),
                Offset = new Offset(0)
            });

        var options = Options.Create(new KafkaOptions
        {
            Topic = new KafkaTopicOptions { IotSnapshots = DefaultTopic }
        });

        _sut = new IotSnapshotPublisher(_mockProducer.Object, options, NullLogger<IotSnapshotPublisher>.Instance);
    }

    private static Iot IotWith(params string[] deviceNames) => new()
    {
        Devices = deviceNames
            .Select(name => new IotDevice { Type = "sensor", Name = name, Payload = new IotPayload { Humidity = 55 } })
            .ToList()
    };

    // ── Skip when empty ──────────────────────────────────────────────────────

    [Fact]
    public async Task PublishAsync_WhenNoDevices_DoesNotCallProducer()
    {
        await _sut.PublishAsync(new Iot { Devices = [] });

        _mockProducer.Verify(
            p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── Produce behavior ─────────────────────────────────────────────────────

    [Fact]
    public async Task PublishAsync_WhenHasDevices_CallsProduceAsyncOnce()
    {
        await _sut.PublishAsync(IotWith("device-1"));

        _mockProducer.Verify(
            p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WhenHasDevices_PublishesToConfiguredTopic()
    {
        await _sut.PublishAsync(IotWith("device-1"));

        _mockProducer.Verify(
            p => p.ProduceAsync(
                DefaultTopic,
                It.IsAny<Message<string, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WhenHasDevices_KeyIsDateInYyyyMmDdFormat()
    {
        Message<string, string>? captured = null;
        _mockProducer
            .Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .Callback<string, Message<string, string>, CancellationToken>((_, msg, _) => captured = msg)
            .ReturnsAsync(new DeliveryResult<string, string> { Topic = DefaultTopic, Partition = new Partition(0), Offset = new Offset(0) });

        await _sut.PublishAsync(IotWith("device-1"));

        captured!.Key.Should().MatchRegex(@"^\d{4}-\d{2}-\d{2}$",
            "key must be formatted as yyyy-MM-dd");
    }

    [Fact]
    public async Task PublishAsync_WhenHasDevices_ValueContainsDeviceName()
    {
        const string deviceName = "living-room-sensor";
        Message<string, string>? captured = null;
        _mockProducer
            .Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .Callback<string, Message<string, string>, CancellationToken>((_, msg, _) => captured = msg)
            .ReturnsAsync(new DeliveryResult<string, string> { Topic = DefaultTopic, Partition = new Partition(0), Offset = new Offset(0) });

        await _sut.PublishAsync(IotWith(deviceName));

        captured!.Value.Should().Contain(deviceName);
    }

    [Fact]
    public async Task PublishAsync_WhenHasDevices_ValueIsValidJson()
    {
        Message<string, string>? captured = null;
        _mockProducer
            .Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .Callback<string, Message<string, string>, CancellationToken>((_, msg, _) => captured = msg)
            .ReturnsAsync(new DeliveryResult<string, string> { Topic = DefaultTopic, Partition = new Partition(0), Offset = new Offset(0) });

        await _sut.PublishAsync(IotWith("device-1"));

        var act = () => JsonDocument.Parse(captured!.Value);
        act.Should().NotThrow("published value must be valid JSON");
    }

    [Fact]
    public async Task PublishAsync_WhenHasDevices_ValueUsesCamelCasePropertyNames()
    {
        Message<string, string>? captured = null;
        _mockProducer
            .Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .Callback<string, Message<string, string>, CancellationToken>((_, msg, _) => captured = msg)
            .ReturnsAsync(new DeliveryResult<string, string> { Topic = DefaultTopic, Partition = new Partition(0), Offset = new Offset(0) });

        await _sut.PublishAsync(IotWith("d1"));

        // camelCase: capturedAt, devices — not CapturedAt, Devices
        captured!.Value.Should().Contain("capturedAt").And.Contain("devices");
    }

    [Fact]
    public async Task PublishAsync_WhenProducerThrows_DoesNotRethrow()
    {
        _mockProducer
            .Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KafkaException(new Error(ErrorCode.BrokerNotAvailable)));

        var act = () => _sut.PublishAsync(IotWith("device-1"));

        await act.Should().NotThrowAsync("Kafka publish errors must be swallowed");
    }

    [Fact]
    public async Task PublishAsync_WithMultipleDevices_StillPublishesOnce()
    {
        await _sut.PublishAsync(IotWith("d1", "d2", "d3"));

        _mockProducer.Verify(
            p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "all devices must be bundled into one Kafka message");
    }
}
