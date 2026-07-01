namespace Notification.Models;

public sealed class NotificationOptions
{
    /// <summary>
    /// Humidity percentage above which an alert is triggered (default: 60).
    /// </summary>
    public double HumidityThreshold { get; set; } = 60;
}
