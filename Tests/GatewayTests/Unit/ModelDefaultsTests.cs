using FluentAssertions;
using Gateway.Models;
using Gateway.Types;

namespace GatewayTests.Unit;

public sealed class ModelDefaultsTests
{
    // ── KafkaOptions ─────────────────────────────────────────────────────────

    [Fact]
    public void KafkaOptions_DefaultBrokers_IsLocalhost9092()
    {
        var options = new KafkaOptions();
        options.Brokers.Should().Be("localhost:9092");
    }

    [Fact]
    public void KafkaOptions_DefaultTopic_IsIotSnapshots()
    {
        var options = new KafkaOptions();
        options.Topic.IotSnapshots.Should().Be("iot.snapshots");
    }

    // ── JwtValidationOptions ─────────────────────────────────────────────────

    [Fact]
    public void JwtValidationOptions_DefaultPublicKeyPath_IsEmptyString()
    {
        var options = new JwtValidationOptions();
        options.PublicKeyPath.Should().Be(string.Empty);
    }

    [Fact]
    public void JwtValidationOptions_DefaultIssuer_IsEmptyString()
    {
        var options = new JwtValidationOptions();
        options.Issuer.Should().Be(string.Empty);
    }

    [Fact]
    public void JwtValidationOptions_DefaultAudience_IsEmptyString()
    {
        var options = new JwtValidationOptions();
        options.Audience.Should().Be(string.Empty);
    }

    // ── Iot domain types ─────────────────────────────────────────────────────

    [Fact]
    public void Iot_CanBeInitializedWithEmptyDeviceList()
    {
        var iot = new Iot { Devices = [] };
        iot.Devices.Should().BeEmpty();
    }

    [Fact]
    public void IotPayload_AllSensorFields_AreNullable()
    {
        var payload = new IotPayload();

        payload.Co2.Should().BeNull();
        payload.Pm25.Should().BeNull();
        payload.Humidity.Should().BeNull();
        payload.Energy.Should().BeNull();
    }
}
