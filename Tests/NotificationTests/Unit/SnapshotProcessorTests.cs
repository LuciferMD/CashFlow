using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Notification.Hubs;
using Notification.Kafka;
using Notification.Models;
using Notification.Services;

namespace NotificationTests.Unit;

public sealed class SnapshotProcessorTests
{
    private readonly Mock<IHubContext<NotificationHub>> _mockHub;
    private readonly Mock<IHubClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<ITelegramService> _mockTelegram;

    public SnapshotProcessorTests()
    {
        _mockHub = new Mock<IHubContext<NotificationHub>>();
        _mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockTelegram = new Mock<ITelegramService>();

        _mockHub.Setup(x => x.Clients).Returns(_mockClients.Object);
        _mockClients.Setup(x => x.All).Returns(_mockClientProxy.Object);
        _mockClientProxy
            .Setup(x => x.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockTelegram
            .Setup(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private SnapshotProcessor CreateProcessor(double humidityThreshold = 70) =>
        new(
            Options.Create(new NotificationOptions { HumidityThreshold = humidityThreshold }),
            _mockHub.Object,
            _mockTelegram.Object,
            NullLogger<SnapshotProcessor>.Instance);

    private static IotSnapshotMessage EmptySnapshot() =>
        new(DateTime.UtcNow, []);

    private static IotSnapshotMessage SnapshotWith(string name, double? humidity, string type = "sensor") =>
        new(DateTime.UtcNow, [
            new IotDevice(type, name, new DevicePayload(null, null, humidity, null))
        ]);

    // ── IotSnapshot event ────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_AlwaysSendsIotSnapshotEvent()
    {
        await CreateProcessor().ProcessAsync(EmptySnapshot());

        _mockClientProxy.Verify(
            x => x.SendCoreAsync("IotSnapshot", It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_SendsExactSnapshotObjectAsSignalRArgument()
    {
        var snapshot = SnapshotWith("s1", 50);
        await CreateProcessor().ProcessAsync(snapshot);

        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "IotSnapshot",
                It.Is<object[]>(args => args.Length == 1 && ReferenceEquals(args[0], snapshot)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_EmptyDeviceList_SendsIotSnapshotExactlyOnce()
    {
        await CreateProcessor().ProcessAsync(EmptySnapshot());

        _mockClientProxy.Verify(
            x => x.SendCoreAsync("IotSnapshot", It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── No alerts when empty / null ──────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_EmptyDeviceList_NeverSendsHumidityAlert()
    {
        await CreateProcessor().ProcessAsync(EmptySnapshot());

        _mockClientProxy.Verify(
            x => x.SendCoreAsync("HumidityAlert", It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_EmptyDeviceList_NeverCallsTelegram()
    {
        await CreateProcessor().ProcessAsync(EmptySnapshot());

        _mockTelegram.Verify(
            x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_WhenPayloadIsNull_DoesNotSendHumidityAlert()
    {
        var snapshot = new IotSnapshotMessage(DateTime.UtcNow, [
            new IotDevice("sensor", "s1", Payload: null)
        ]);

        await CreateProcessor().ProcessAsync(snapshot);

        _mockClientProxy.Verify(
            x => x.SendCoreAsync("HumidityAlert", It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_WhenHumidityIsNull_DoesNotSendHumidityAlert()
    {
        await CreateProcessor().ProcessAsync(SnapshotWith("s1", humidity: null));

        _mockClientProxy.Verify(
            x => x.SendCoreAsync("HumidityAlert", It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── Threshold boundary ───────────────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_WhenHumidityAboveThreshold_SendsHumidityAlertEvent()
    {
        await CreateProcessor(humidityThreshold: 70).ProcessAsync(SnapshotWith("s1", 71));

        _mockClientProxy.Verify(
            x => x.SendCoreAsync("HumidityAlert", It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WhenHumidityAboveThreshold_SendsTelegramMessage()
    {
        await CreateProcessor(humidityThreshold: 70).ProcessAsync(SnapshotWith("s1", 85));

        _mockTelegram.Verify(
            x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WhenHumidityExactlyAtThreshold_DoesNotSendAlert()
    {
        // Condition is strictly greater-than, so equality must not trigger
        await CreateProcessor(humidityThreshold: 70).ProcessAsync(SnapshotWith("s1", 70));

        _mockClientProxy.Verify(
            x => x.SendCoreAsync("HumidityAlert", It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_WhenHumidityBelowThreshold_DoesNotSendHumidityAlert()
    {
        await CreateProcessor(humidityThreshold: 70).ProcessAsync(SnapshotWith("s1", 65));

        _mockClientProxy.Verify(
            x => x.SendCoreAsync("HumidityAlert", It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_WhenHumidityBelowThreshold_DoesNotCallTelegram()
    {
        await CreateProcessor(humidityThreshold: 70).ProcessAsync(SnapshotWith("s1", 55));

        _mockTelegram.Verify(
            x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── Multiple devices ─────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_MultipleDevicesAboveThreshold_SendsAlertForEachDevice()
    {
        var snapshot = new IotSnapshotMessage(DateTime.UtcNow, [
            new IotDevice("sensor", "s1", new DevicePayload(null, null, 80, null)),
            new IotDevice("sensor", "s2", new DevicePayload(null, null, 90, null)),
            new IotDevice("sensor", "s3", new DevicePayload(null, null, 75, null)),
        ]);

        await CreateProcessor(humidityThreshold: 70).ProcessAsync(snapshot);

        _mockClientProxy.Verify(
            x => x.SendCoreAsync("HumidityAlert", It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
        _mockTelegram.Verify(
            x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task ProcessAsync_MixedHumidityDevices_SendsAlertOnlyForHighOnes()
    {
        var snapshot = new IotSnapshotMessage(DateTime.UtcNow, [
            new IotDevice("sensor", "high",  new DevicePayload(null, null, 85,   null)),
            new IotDevice("sensor", "low",   new DevicePayload(null, null, 50,   null)),
            new IotDevice("sensor", "noHum", new DevicePayload(null, null, null, null)),
        ]);

        await CreateProcessor(humidityThreshold: 70).ProcessAsync(snapshot);

        _mockClientProxy.Verify(
            x => x.SendCoreAsync("HumidityAlert", It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockTelegram.Verify(
            x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── Telegram message content ─────────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_TelegramTextContainsDeviceName()
    {
        const string deviceName = "bathroom-sensor";

        await CreateProcessor(humidityThreshold: 70).ProcessAsync(SnapshotWith(deviceName, 90));

        _mockTelegram.Verify(
            x => x.SendMessageAsync(
                It.Is<string>(msg => msg.Contains(deviceName)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_TelegramTextContainsHumidityValue()
    {
        await CreateProcessor(humidityThreshold: 70).ProcessAsync(SnapshotWith("s1", 88.5));

        _mockTelegram.Verify(
            x => x.SendMessageAsync(
                It.Is<string>(msg => msg.Contains("88.5")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
