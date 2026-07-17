using CashFlow.Shared.Middleware;
using DotNetEnv;
using Microsoft.Extensions.Options;
using Notification.Hubs;
using Notification.Infrastructure;
using Notification.Kafka;
using Notification.Models;
using Notification.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var repoRoot = RepoRoot.Find();
    var envPath = Path.Combine(repoRoot, ".env");
    if (File.Exists(envPath))
        Env.Load(envPath);

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseCashFlowSerilog();
    builder.Services.AddCashFlowElasticApm(builder.Configuration);

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

    app.UseSerilogRequestLogging();


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
        ILogger<Program> logger,
        CancellationToken ct) =>
    {
        logger.LogInformation(
            "Test snapshot endpoint invoked with {DeviceCount} device(s).",
            snapshot.Devices.Count);
        await processor.ProcessAsync(snapshot, ct);
        return Results.Ok(new { processed = true, deviceCount = snapshot.Devices.Count });
    });

    var kafka = app.Services.GetRequiredService<IOptions<KafkaOptions>>().Value;
    Log.Information(
        "Notification service starting in {Environment}. Kafka brokers: {Brokers}, topic: {Topic}, group: {GroupId}",
        app.Environment.EnvironmentName,
        kafka.Brokers,
        kafka.Topic.IotSnapshots,
        kafka.GroupId);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Notification service terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Exposed for WebApplicationFactory in integration tests.
public partial class Program;
