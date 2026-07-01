using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Notification.Hubs;
using Notification.Kafka;
using Notification.Models;

namespace Notification.Services;

public sealed class SnapshotProcessor : ISnapshotProcessor
{
    private readonly NotificationOptions _options;
    private readonly IHubContext<NotificationHub> _hub;
    private readonly ITelegramService _telegram;
    private readonly ILogger<SnapshotProcessor> _logger;

    public SnapshotProcessor(
        IOptions<NotificationOptions> options,
        IHubContext<NotificationHub> hub,
        ITelegramService telegram,
        ILogger<SnapshotProcessor> logger)
    {
        _options = options.Value;
        _hub = hub;
        _telegram = telegram;
        _logger = logger;
    }

    public async Task ProcessAsync(IotSnapshotMessage snapshot, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Processing snapshot captured at {CapturedAt} with {Count} device(s).",
            snapshot.CapturedAt, snapshot.Devices.Count);

        // Push full snapshot to every connected SignalR client.
        await _hub.Clients.All.SendAsync("IotSnapshot", snapshot, ct);

        // Check humidity threshold per device.
        foreach (var device in snapshot.Devices)
        {
            var humidity = device.Payload?.Humidity;
            if (humidity is null) continue;

            if (humidity > _options.HumidityThreshold)
            {
                _logger.LogWarning(
                    "[alert] High humidity on \"{Device}\": {Humidity}% (threshold: {Threshold}%)",
                    device.Name, humidity, _options.HumidityThreshold);

                var alert = new
                {
                    DeviceName = device.Name,
                    DeviceType = device.Type,
                    Humidity   = humidity,
                    Threshold  = _options.HumidityThreshold,
                    CapturedAt = snapshot.CapturedAt,
                };

                await _hub.Clients.All.SendAsync("HumidityAlert", alert, ct);

                var text =
                    $"⚠️ <b>Humidity Alert</b>\n" +
                    $"Device: <code>{device.Name}</code> ({device.Type})\n" +
                    $"Humidity: <b>{humidity}%</b> (threshold: {_options.HumidityThreshold}%)\n" +
                    $"Captured at: {snapshot.CapturedAt:yyyy-MM-dd HH:mm:ss} UTC";

                await _telegram.SendMessageAsync(text, ct);
            }
        }
    }
}
