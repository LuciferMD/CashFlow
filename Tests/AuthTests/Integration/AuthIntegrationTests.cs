// ┌─────────────────────────────────────────────────────────────────────────┐
// │  Integration tests – Auth service                                       │
// │                                                                         │
// │  Scope: a real in-process ASP.NET Core host (WebApplicationFactory)     │
// │  against a real PostgreSQL database spun up via Testcontainers.         │
// │                                                                         │
// │  Required packages to add when implementing:                            │
// │    Testcontainers.PostgreSql                                            │
// │    Microsoft.AspNetCore.Mvc.Testing                                     │
// │                                                                         │
// │  RSA keys: generate a throw-away key pair in test setup (RSA.Create())  │
// │  rather than relying on file paths from the repo.                       │
// └─────────────────────────────────────────────────────────────────────────┘

namespace AuthTests.Integration;

// ----- Planned test cases ---------------------------------------------------
//
// [Fact] Register_WithValidBody_Returns200AndSetsCookie
//   POST /auth/register { userName, email, password }
//   → 200, response has Set-Cookie: GuardPass=<jwt>
//
// [Fact] Register_WithDuplicateEmail_ReturnsError
//   Register twice with the same email
//   → second request should return a non-200 status (DB constraint violation)
//
// [Fact] Login_WithCorrectCredentials_Returns200AndSetsCookie
//   Register a user, then POST /auth/login with same creds
//   → 200, response has Set-Cookie: GuardPass=<jwt>
//
// [Fact] Login_WithWrongPassword_Returns401
//   Register a user, then login with wrong password
//   → 401
//
// [Fact] Login_WithUnknownEmail_Returns401
//   POST /auth/login with an email that was never registered
//   → 401
//
// [Fact] Test_WithValidJwt_Returns200
//   Register → extract GuardPass cookie → GET /auth (authorized endpoint)
//   → 200 "My boy"
//
// [Fact] Test_WithoutJwt_Returns401
//   GET /auth without a cookie → 401
//
// ---------------------------------------------------------------------------
//
// Example factory setup (add when ready):
//
//   internal sealed class AuthFactory : WebApplicationFactory<Program>
//   {
//       private readonly string _connectionString;
//       private readonly string _privateKeyPath;
//       private readonly string _publicKeyPath;
//
//       public AuthFactory(string connectionString, string privateKeyPath, string publicKeyPath)
//       {
//           _connectionString = connectionString;
//           _privateKeyPath   = privateKeyPath;
//           _publicKeyPath    = publicKeyPath;
//       }
//
//       protected override void ConfigureWebHost(IWebHostBuilder builder)
//       {
//           builder.ConfigureAppConfiguration((_, cfg) =>
//               cfg.AddInMemoryCollection(new Dictionary<string, string?>
//               {
//                   ["ConnectionStrings:DefaultConnection"] = _connectionString,
//                   ["JwtOptions:PrivateKeyPath"]           = _privateKeyPath,
//                   ["JwtOptions:PublicKeyPath"]            = _publicKeyPath,
//                   ["JwtOptions:Issuer"]                   = "test-issuer",
//                   ["JwtOptions:Audience"]                 = "test-audience",
//                   ["JwtOptions:ExpiersHours"]             = "1",
//               }));
//       }
//   }
//
// ---------------------------------------------------------------------------

public sealed class AuthIntegrationTests
{
    // Placeholder — implement tests listed above using the factory pattern above.
}
