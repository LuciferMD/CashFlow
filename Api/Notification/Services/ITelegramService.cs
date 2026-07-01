namespace Notification.Services;

public interface ITelegramService
{
    Task SendMessageAsync(string text, CancellationToken ct = default);
}
