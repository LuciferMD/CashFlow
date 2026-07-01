using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Notification.Models;

namespace Notification.Services;

public sealed class TelegramService : ITelegramService
{
    private readonly TelegramOptions _options;
    private readonly HttpClient _http;
    private readonly ILogger<TelegramService> _logger;

    public TelegramService(
        IOptions<TelegramOptions> options,
        HttpClient http,
        ILogger<TelegramService> logger)
    {
        _options = options.Value;
        _http = http;
        _logger = logger;
    }

    public async Task SendMessageAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.BotToken) ||
            string.IsNullOrWhiteSpace(_options.ChatId))
        {
            _logger.LogDebug("[telegram] Skipped – BotToken or ChatId not configured.");
            return;
        }

        var url = $"https://api.telegram.org/bot{_options.BotToken}/sendMessage";

        var payload = new
        {
            chat_id    = _options.ChatId,
            text       = text,
            parse_mode = "HTML",
        };

        try
        {
            var response = await _http.PostAsJsonAsync(url, payload, ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning(
                    "[telegram] Request failed {StatusCode}: {Body}",
                    (int)response.StatusCode, body);
                return;
            }

            _logger.LogInformation("[telegram] Message sent to chat {ChatId}.", _options.ChatId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[telegram] Failed to send message to Telegram.");
        }
    }
}
