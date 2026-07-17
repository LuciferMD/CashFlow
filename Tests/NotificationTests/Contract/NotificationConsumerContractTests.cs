using System.Text.Json;
using Notification.Kafka;
using Notification.Services;
using PactNet;
using PactNet.Matchers;
using PactNet.Output.Xunit;
using Xunit.Abstractions;

namespace NotificationTests.Contract;

/// <summary>
/// Consumer-driven contract: Notification expects IoT snapshot messages from Gateway on "iot.snapshots".
/// Generates Tests/pacts/Notification-Gateway.json for Gateway provider verification.
/// </summary>
[Trait("Category", "Contract")]
public sealed class NotificationConsumerContractTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private static int _pactInitialized;

    private readonly IMessagePactBuilderV4 _messagePact;

    public NotificationConsumerContractTests(ITestOutputHelper output)
    {
        Directory.CreateDirectory(ContractPaths.PactsDirectory);

        if (Interlocked.CompareExchange(ref _pactInitialized, 1, 0) == 0)
        {
            var pactFile = ContractPaths.NotificationGatewayPact;
            if (File.Exists(pactFile))
            {
                File.Delete(pactFile);
            }
        }

        var pact = Pact.V4("Notification", "Gateway", new PactConfig
        {
            PactDir = ContractPaths.PactsDirectory,
            LogLevel = PactLogLevel.Warn,
            DefaultJsonSettings = JsonOptions,
            Outputters = [new XunitOutput(output)],
        });

        _messagePact = pact.WithMessageInteractions();
    }

    [Fact]
    public void Notification_Expects_IotSnapshotMessage_WithDeviceReadings()
    {
        _messagePact
            .ExpectsToReceive("an IoT snapshot with device readings")
            .Given("Gateway published a snapshot to iot.snapshots")
            .WithMetadata("contentType", "application/json")
            .WithJsonContent(new
            {
                capturedAt = Match.Type("2024-06-15T10:00:00.000Z"),
                devices = Match.MinType(new
                {
                    type = Match.Type("sensor"),
                    name = Match.Type("Kitchen"),
                    payload = new
                    {
                        co2 = Match.Type(400),
                        pm25 = Match.Type(10),
                        humidity = Match.Type(60),
                        energy = Match.Type(1.5),
                    },
                }, 1),
            })
            .Verify<IotSnapshotMessage>(snapshot =>
            {
                var processor = ContractTestHelpers.CreateProcessor();
                processor.ProcessAsync(snapshot).GetAwaiter().GetResult();
            });
    }

    [Fact]
    public void Notification_Expects_IotSnapshotMessage_WithNullablePayloadFields()
    {
        _messagePact
            .ExpectsToReceive("an IoT snapshot with nullable payload fields")
            .Given("Gateway published a snapshot with sparse device payload")
            .WithMetadata("contentType", "application/json")
            .WithJsonContent(new
            {
                capturedAt = Match.Type("2024-06-15T10:00:00.000Z"),
                devices = Match.MinType(new
                {
                    type = Match.Type("sensor"),
                    name = Match.Type("Hall"),
                    payload = new
                    {
                        humidity = Match.Type(55),
                    },
                }, 1),
            })
            .Verify<IotSnapshotMessage>(snapshot =>
            {
                var processor = ContractTestHelpers.CreateProcessor();
                processor.ProcessAsync(snapshot).GetAwaiter().GetResult();
            });
    }
}
