using Microsoft.AspNetCore.SignalR;

namespace Notification.Hubs;

/// <summary>
/// SignalR hub that pushes real-time IoT alerts to connected browser clients.
///
/// Events pushed from server:
///   "IotSnapshot"    – fired on every incoming Kafka snapshot (all device data)
///   "HumidityAlert"  – fired when any device humidity exceeds the configured threshold
/// </summary>
public sealed class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
