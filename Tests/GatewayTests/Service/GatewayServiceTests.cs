// ┌─────────────────────────────────────────────────────────────────────────┐
// │  Service tests – Gateway service                                        │
// │                                                                         │
// │  Scope: a real in-process HotChocolate GraphQL host plus real           │
// │  infrastructure spun up via Testcontainers.                             │
// │                                                                         │
// │  Required packages to add when implementing:                            │
// │    Testcontainers.Kafka                                                 │
// │    Microsoft.AspNetCore.Mvc.Testing                                     │
// │                                                                         │
// │  The WeakApp IoT upstream can be mocked via a lightweight WireMock      │
// │  server (WireMock.Net) or by replacing HttpIotClient with a stub.       │
// └─────────────────────────────────────────────────────────────────────────┘

namespace GatewayTests.Service;

// ----- Planned test cases ---------------------------------------------------
//
// [Fact] GetIot_WithoutAuth_Returns401
//   POST /graphql { query: "{ iot { devices { name } } }" } — no cookie
//   → 401 Unauthorized
//
// [Fact] GetIot_WithValidJwt_ReturnsDevices
//   POST /graphql with GuardPass cookie containing a valid JWT
//   → 200, body contains expected devices from stubbed WeakApp
//
// [Fact] GetIot_WhenWeakAppReturnsData_PublishesToKafka
//   1. Start Testcontainers Kafka + stub WeakApp (WireMock)
//   2. Execute authenticated GraphQL query
//   3. Consume from "iot.snapshots" topic
//   4. Assert a message was produced with the correct device data
//
// [Fact] GetIot_WhenWeakAppReturnsEmptyArray_DoesNotPublishToKafka
//   WeakApp returns [] → publisher.PublishAsync is never called
//   → no message in Kafka
//
// [Fact] GetIot_WhenWeakAppIs429_StillReturnsEmptyDevices
//   WeakApp always returns 429 → all retries exhausted → empty Iot returned
//   (run with a very short retry delay in test config)
//
// [Fact] GetIot_WhenWeakAppIs500_StillReturnsEmptyDevices
//   WeakApp always returns 500 → all retries exhausted → empty Iot returned
//
// ---------------------------------------------------------------------------
//
// Example factory setup (add when ready):
//
//   internal sealed class GatewayFactory : WebApplicationFactory<Program>
//   {
//       private readonly string _kafkaBrokers;
//       private readonly string _iotBaseUrl;   // WireMock server URL
//       private readonly string _publicKeyPath;
//
//       protected override void ConfigureWebHost(IWebHostBuilder builder)
//       {
//           builder.ConfigureAppConfiguration((_, cfg) =>
//               cfg.AddInMemoryCollection(new Dictionary<string, string?>
//               {
//                   ["Kafka:Brokers"]      = _kafkaBrokers,
//                   ["Iot:BaseUrl"]        = _iotBaseUrl,
//                   ["Iot:ApiKey"]         = "test-key",
//                   ["JwtOptions:PublicKeyPath"] = _publicKeyPath,
//                   ["JwtOptions:Issuer"]  = "test",
//                   ["JwtOptions:Audience"] = "test",
//               }));
//       }
//   }
//
// ---------------------------------------------------------------------------

public sealed class GatewayServiceTests
{
    // Placeholder — implement tests listed above using the factory pattern above.
}
