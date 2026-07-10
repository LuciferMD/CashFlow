// ┌─────────────────────────────────────────────────────────────────────────┐
// │  Integration tests – Notification service                               │
// │                                                                         │
// │  Scope: a real in-process ASP.NET Core host (WebApplicationFactory)     │
// │  plus real infrastructure spun up via Testcontainers.                   │
// │                                                                         │
// │  Required packages to add when implementing:                            │
// │    Testcontainers.Kafka                                                 │
// │    Microsoft.AspNetCore.Mvc.Testing                                     │
// │    Microsoft.AspNetCore.SignalR.Client                                  │
// │                                                                         │
// │  Run order: unit tests first, integration tests in CI only              │
// └─────────────────────────────────────────────────────────────────────────┘

namespace NotificationTests.Integration;

// ----- Planned test cases ---------------------------------------------------
//
// [Fact] HealthEndpoint_Returns200Ok
//   GET /health → 200
//
// [Fact] TestSnapshotEndpoint_WithValidBody_Returns200AndProcessedTrue
//   POST /test/snapshot with a valid IotSnapshotMessage body
//   → 200, body contains { processed: true, deviceCount: N }
//
// [Fact] TestSnapshotEndpoint_WithHighHumidity_BroadcastsHumidityAlertToSignalRClients
//   1. Connect a SignalR client to /hubs/notifications
//   2. POST /test/snapshot with humidity > threshold
//   3. Assert client receives "HumidityAlert" event with correct DeviceName
//
// [Fact] TestSnapshotEndpoint_Always_BroadcastsIotSnapshotToSignalRClients
//   1. Connect a SignalR client
//   2. POST /test/snapshot
//   3. Assert client receives "IotSnapshot" event
//
// [Fact] KafkaConsumerService_WhenMessageProduced_CallsSnapshotProcessor
//   1. Start Testcontainers Kafka
//   2. Produce IotSnapshotMessage to "iot.snapshots" topic
//   3. Assert SnapshotProcessor.ProcessAsync was invoked
//      (use a spy / ISnapshotProcessor mock registered via WebApplicationFactory override)
//
// [Fact] KafkaConsumerService_WhenMalformedJsonProduced_DoesNotCrash
//   Produce garbage JSON → service keeps running, no unhandled exception
//
// ---------------------------------------------------------------------------
//
// Example WebApplicationFactory setup (add when ready):
//
//   internal sealed class NotificationFactory : WebApplicationFactory<Program>
//   {
//       private readonly string _kafkaBrokers;
//
//       public NotificationFactory(string kafkaBrokers)
//           => _kafkaBrokers = kafkaBrokers;
//
//       protected override void ConfigureWebHost(IWebHostBuilder builder)
//       {
//           builder.ConfigureAppConfiguration((_, cfg) =>
//               cfg.AddInMemoryCollection(new Dictionary<string, string?>
//               {
//                   ["Kafka:Brokers"] = _kafkaBrokers,
//                   ["Telegram:BotToken"] = "",   // disable real Telegram calls
//               }));
//
//           builder.ConfigureServices(services =>
//           {
//               // Replace TelegramService with a mock so no real HTTP calls are made
//               services.AddSingleton<ITelegramService, Mock<ITelegramService>>(...);
//           });
//       }
//   }
//
// ---------------------------------------------------------------------------

public sealed class NotificationIntegrationTests
{
    // Placeholder — implement tests listed above using the factory pattern above.
}
