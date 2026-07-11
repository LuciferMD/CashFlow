using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Auth.Models;
using Auth.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PactNet;
using PactNet.Output.Xunit;
using PactNet.Verifier;
using Xunit.Abstractions;

namespace AuthTests.Contract;

/// <summary>
/// Provider verification for Web -> Auth HTTP contracts and JWT shape assertions.
/// </summary>
[Collection(nameof(AuthContractCollection))]
[Trait("Category", "Contract")]
public sealed class AuthContractTests : IDisposable
{
    private readonly AuthContractFixture _fixture;
    private readonly PactVerifier _verifier;

    public AuthContractTests(AuthContractFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _verifier = new PactVerifier(
            "Auth",
            new PactVerifierConfig
            {
                LogLevel = PactLogLevel.Warn,
                Outputters = [new XunitOutput(output)],
            });
    }

    [Fact]
    public void Auth_SatisfiesHttpContract_ForWebConsumer()
    {
        var pactPath = ContractPaths.WebAuthPact;
        Assert.True(File.Exists(pactPath), $"Pact file not found: {pactPath}. Run Web consumer contract tests first.");

        _verifier
            .WithHttpEndpoint(_fixture.ServerUri)
            .WithFileSource(new FileInfo(pactPath))
            .WithProviderStateUrl(new Uri(_fixture.ServerUri, "/pact/provider-states"))
            .Verify();
    }

    [Fact]
    public void JwtProvider_GeneratedToken_ContainsUserIdClaim()
    {
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var token = _fixture.CreateContractJwt(userId);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "userId" && c.Value == userId.ToString());
    }

    [Fact]
    public void JwtProvider_GeneratedToken_HasCorrectIssuerAndAudience()
    {
        var token = _fixture.CreateContractJwt(Guid.NewGuid());

        using var scope = _fixture.Services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<JwtOptions>>().Value;

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Issuer.Should().Be(options.Issuer);
        jwt.Audiences.Should().Contain(options.Audience);
    }

    public void Dispose()
    {
        _verifier.Dispose();
        GC.SuppressFinalize(this);
    }
}
