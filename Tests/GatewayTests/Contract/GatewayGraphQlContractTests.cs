using PactNet;
using PactNet.Output.Xunit;
using PactNet.Verifier;
using Xunit.Abstractions;

namespace GatewayTests.Contract;

/// <summary>
/// Provider verification for Web -> Gateway GraphQL HTTP contract.
/// </summary>
[Collection(nameof(GatewayHttpContractCollection))]
[Trait("Category", "Contract")]
public sealed class GatewayGraphQlContractTests : IDisposable
{
    private readonly GatewayHttpContractFixture _fixture;
    private readonly PactVerifier _verifier;

    public GatewayGraphQlContractTests(GatewayHttpContractFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _verifier = new PactVerifier(
            "Gateway",
            new PactVerifierConfig
            {
                LogLevel = PactLogLevel.Warn,
                Outputters = [new XunitOutput(output)],
            });
    }

    [Fact]
    public void Gateway_SatisfiesGraphQlContract_ForWebConsumer()
    {
        var pactPath = ContractTestHelpers.WebGatewayPactPath;
        Assert.True(File.Exists(pactPath), $"Pact file not found: {pactPath}. Run Web consumer contract tests first.");

        var patchedPact = PactFileHelpers.PatchBearerToken(pactPath, _fixture.ContractJwt);

        try
        {
            _verifier
                .WithHttpEndpoint(_fixture.ServerUri)
                .WithFileSource(new FileInfo(patchedPact))
                .Verify();
        }
        finally
        {
            if (File.Exists(patchedPact))
            {
                File.Delete(patchedPact);
            }
        }
    }

    public void Dispose()
    {
        _verifier.Dispose();
        GC.SuppressFinalize(this);
    }
}
