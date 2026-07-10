using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Notification.Models;
using Notification.Services;

namespace NotificationTests.Unit;

public sealed class TelegramServiceTests
{
    private const string ValidBotToken = "test-bot-token-12345";
    private const string ValidChatId = "987654321";

    private static (TelegramService Service, Mock<HttpMessageHandler> Handler) Create(
        string botToken = ValidBotToken,
        string chatId = ValidChatId,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode) { Content = new StringContent("{}") });

        var http = new HttpClient(handler.Object);
        var options = Options.Create(new TelegramOptions { BotToken = botToken, ChatId = chatId });
        var service = new TelegramService(options, http, NullLogger<TelegramService>.Instance);

        return (service, handler);
    }

    // ── Skip when not configured ─────────────────────────────────────────────

    [Fact]
    public async Task SendMessageAsync_WhenBotTokenIsEmpty_DoesNotCallHttp()
    {
        var (service, handler) = Create(botToken: "");
        await service.SendMessageAsync("hello");

        handler.Protected().Verify(
            "SendAsync", Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendMessageAsync_WhenChatIdIsEmpty_DoesNotCallHttp()
    {
        var (service, handler) = Create(chatId: "");
        await service.SendMessageAsync("hello");

        handler.Protected().Verify(
            "SendAsync", Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendMessageAsync_WhenBotTokenIsWhitespace_DoesNotCallHttp()
    {
        var (service, handler) = Create(botToken: "   ");
        await service.SendMessageAsync("hello");

        handler.Protected().Verify(
            "SendAsync", Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendMessageAsync_WhenChatIdIsWhitespace_DoesNotCallHttp()
    {
        var (service, handler) = Create(chatId: "   ");
        await service.SendMessageAsync("hello");

        handler.Protected().Verify(
            "SendAsync", Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    // ── HTTP call shape ──────────────────────────────────────────────────────

    [Fact]
    public async Task SendMessageAsync_WhenConfigured_CallsHttpExactlyOnce()
    {
        var (service, handler) = Create();
        await service.SendMessageAsync("test message");

        handler.Protected().Verify(
            "SendAsync", Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendMessageAsync_WhenConfigured_UsesPostMethod()
    {
        var (service, handler) = Create();
        await service.SendMessageAsync("test");

        handler.Protected().Verify(
            "SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendMessageAsync_WhenConfigured_PostsToCorrectTelegramBotUrl()
    {
        var (service, handler) = Create(botToken: ValidBotToken);
        await service.SendMessageAsync("test");

        handler.Protected().Verify(
            "SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri!.ToString().Contains($"bot{ValidBotToken}/sendMessage")),
            ItExpr.IsAny<CancellationToken>());
    }

    // ── Resilience — must never throw ────────────────────────────────────────

    [Fact]
    public async Task SendMessageAsync_WhenHttpResponseIsFailure_DoesNotThrow()
    {
        var (service, _) = Create(statusCode: HttpStatusCode.InternalServerError);

        var act = () => service.SendMessageAsync("test");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendMessageAsync_WhenHttpResponseIsUnauthorized_DoesNotThrow()
    {
        var (service, _) = Create(statusCode: HttpStatusCode.Unauthorized);

        var act = () => service.SendMessageAsync("test");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendMessageAsync_WhenHttpThrowsNetworkException_DoesNotThrow()
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("network unreachable"));

        var options = Options.Create(new TelegramOptions { BotToken = ValidBotToken, ChatId = ValidChatId });
        var service = new TelegramService(options, new HttpClient(handler.Object), NullLogger<TelegramService>.Instance);

        var act = () => service.SendMessageAsync("test");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendMessageAsync_OnSuccessfulResponse_CompletesWithoutException()
    {
        var (service, _) = Create(statusCode: HttpStatusCode.OK);

        var act = () => service.SendMessageAsync("alert text");

        await act.Should().NotThrowAsync();
    }
}
