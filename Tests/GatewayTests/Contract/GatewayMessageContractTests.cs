using System.Text.Json;
using System.Text.Json.Serialization;
using PactNet;
using PactNet.Output.Xunit;
using PactNet.Verifier;
using Xunit.Abstractions;

namespace GatewayTests.Contract;

/// <summary>
/// Provider verification for Kafka message contracts published by Gateway.
/// </summary>
[Trait("Category", "Contract")]
public sealed class GatewayMessageContractTests
{
    private readonly ITestOutputHelper _output;

    public GatewayMessageContractTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Gateway_SatisfiesIotSnapshotContract_ForNotificationConsumer()
    {
        using var verifier = CreateVerifier(_output);
        VerifyMessagePact(
            verifier,
            ContractTestHelpers.NotificationGatewayPactPath,
            "Notification consumer contract tests");
    }

    [Fact]
    public void Gateway_SatisfiesIotSnapshotContract_ForHistoryStoreConsumer()
    {
        using var verifier = CreateVerifier(_output);
        VerifyMessagePact(
            verifier,
            ContractTestHelpers.HistoryStoreGatewayPactPath,
            "HistoryStore consumer contract tests");
    }

    private static PactVerifier CreateVerifier(ITestOutputHelper? output = null) =>
        new(
            "Gateway",
            new PactVerifierConfig
            {
                LogLevel = PactLogLevel.Warn,
                Outputters = output is null ? [] : [new XunitOutput(output)],
            });

    private static void VerifyMessagePact(PactVerifier verifier, string pactPath, string missingHint)
    {
        Assert.True(File.Exists(pactPath), $"Pact file not found: {pactPath}. Run {missingHint} first.");

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        verifier
            .WithHttpEndpoint(new Uri("http://localhost:49152"))
            .WithMessages(scenarios =>
            {
                scenarios
                    .Add("an IoT snapshot with device readings", () =>
                        ContractTestHelpers.CreateIotSnapshotMessage())
                    .Add("an IoT snapshot with nullable payload fields", () =>
                        new
                        {
                            capturedAt = new DateTime(2024, 6, 15, 10, 0, 0, DateTimeKind.Utc),
                            devices = new[]
                            {
                                new
                                {
                                    type = "sensor",
                                    name = "Hall",
                                    payload = new { humidity = 55 },
                                },
                            },
                        });
            }, jsonOptions)
            .WithFileSource(new FileInfo(pactPath))
            .Verify();
    }
}
