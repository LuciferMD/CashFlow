using CashFlow.Shared.Middleware;
using DotNetEnv;
using Notification.Hubs;
using Notification.Infrastructure;
using Notification.Kafka;
using Notification.Models;
using Notification.Services;

var repoRoot = RepoRoot.Find();
var envPath = Path.Combine(repoRoot, ".env");
if (File.Exists(envPath))
    Env.Load(envPath);

var builder = WebApplication.CreateBuilder(args);

// ── Options ───────────────────────────────────────────────────────────────────

builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<TelegramOptions>(builder.Configuration.GetSection("Telegram"));
builder.Services.Configure<NotificationOptions>(builder.Configuration.GetSection("Notification"));

// ── CORS – allow React frontend to connect over WebSocket ─────────────────────

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy
            .WithOrigins(
                "https://localhost:5173",
                "https://localhost:3000",
                "http://localhost:3000",
                "http://localhost:5173")
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// ── SignalR ───────────────────────────────────────────────────────────────────

builder.Services.AddSignalR();

// ── Telegram ─────────────────────────────────────────────────────────────────

builder.Services.AddHttpClient<ITelegramService, TelegramService>();

// ── Core snapshot processing (shared by Kafka consumer and test endpoint) ─────

builder.Services.AddSingleton<ISnapshotProcessor, SnapshotProcessor>();

// ── Kafka consumer ────────────────────────────────────────────────────────────

builder.Services.AddHostedService<KafkaConsumerService>();

// ── Health ────────────────────────────────────────────────────────────────────

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandling();

app.UseCors("Frontend");
app.UseWebSockets();
app.UseRouting();

// ── Endpoints ─────────────────────────────────────────────────────────────────

app.MapHealthChecks("/health");

app.MapHub<NotificationHub>("/hubs/notifications");

// Test endpoint – available in all environments so you can verify from Postman
// without needing Kafka to be running.
app.MapPost("/test/snapshot", async (
    IotSnapshotMessage snapshot,
    ISnapshotProcessor processor,
    CancellationToken ct) =>
{
    await processor.ProcessAsync(snapshot, ct);
    return Results.Ok(new { processed = true, deviceCount = snapshot.Devices.Count });
});

app.Run();
