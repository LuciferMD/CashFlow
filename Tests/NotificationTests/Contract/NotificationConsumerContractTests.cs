// ┌─────────────────────────────────────────────────────────────────────────┐
// │  Consumer-Driven Contract (CDC) tests – Notification service            │
// │                                                                         │
// │  Role: CONSUMER of the "iot.snapshots" Kafka topic                      │
// │  Producer: Gateway service (publishes IotSnapshotMessage)               │
// │                                                                         │
// │  Tool: PactNet v5  (https://github.com/pact-foundation/pact-net)        │
// │                                                                         │
// │  Required packages to add when implementing:                            │
// │    PactNet                                                              │
// │    PactNet.Output.Xunit                                                 │
// │                                                                         │
// │  Pact workflow:                                                         │
// │    1. Consumer test (this file) generates a pact JSON file              │
// │       describing the message schema Notification expects.               │
// │    2. Producer verification (Gateway.Tests.Contract) runs against       │
// │       the real Gateway and verifies it satisfies the pact.              │
// │    3. Both sides share pacts via a Pact Broker                          │
// │       (self-hosted or pactflow.io) or via the file system in CI.        │
// └─────────────────────────────────────────────────────────────────────────┘

namespace NotificationTests.Contract;

// ----- Planned test cases ---------------------------------------------------
//
// [Fact] Notification_Expects_IotSnapshotMessage_WithRequiredFields
//   Defines the consumer pact:
//   {
//     "capturedAt": <ISO-8601 DateTime>,
//     "devices": [
//       {
//         "type":    <non-empty string>,
//         "name":    <non-empty string>,
//         "payload": {
//           "humidity": <nullable double>
//         }
//       }
//     ]
//   }
//   Verifies SnapshotProcessor.ProcessAsync handles this message correctly.
//
// [Fact] Notification_Expects_IotSnapshotMessage_WithNullPayload_IsHandledGracefully
//   Devices with null payload must not crash the consumer.
//
// [Fact] Notification_Expects_IotSnapshotMessage_WithMissingHumidity_IsHandledGracefully
//   Devices with payload but no humidity field must not crash the consumer.
//
// ---------------------------------------------------------------------------
//
// Example pact setup using PactNet v5 message pacts (Kafka / async):
//
//   public class NotificationConsumerContractTests
//   {
//       private readonly IMessagePactBuilderV4 _pact;
//
//       public NotificationConsumerContractTests()
//       {
//           _pact = Pact.V4("Notification", "Gateway", new PactConfig
//               {
//                   PactDir = Path.Combine(Directory.GetCurrentDirectory(), "pacts"),
//                   LogLevel = PactLogLevel.Warn,
//               })
//               .WithMessageInteractions();
//       }
//
//       [Fact]
//       public async Task Notification_Expects_IotSnapshotMessage_WithRequiredFields()
//       {
//           await _pact
//               .ExpectsToReceive("an IoT snapshot with one device")
//               .WithMetadata("contentType", "application/json")
//               .WithJsonContent(new
//               {
//                   capturedAt = Match.Type(DateTime.UtcNow),
//                   devices = Match.MinType(new
//                   {
//                       type    = Match.Type("sensor"),
//                       name    = Match.Type("room-1"),
//                       payload = new { humidity = Match.Decimal(65.4) },
//                   }, 1),
//               })
//               .VerifyAsync<IotSnapshotMessage>(async msg =>
//               {
//                   var processor = BuildProcessor(); // create with mocked deps
//                   await processor.ProcessAsync(msg);
//               });
//       }
//   }
//
// ---------------------------------------------------------------------------

public sealed class NotificationConsumerContractTests
{
    // Placeholder — implement contracts listed above.
}
