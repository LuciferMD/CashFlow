using FluentAssertions;
using Notification.Models;

namespace NotificationTests.Unit;

public sealed class ModelDefaultsTests
{
    [Fact]
    public void NotificationOptions_DefaultHumidityThreshold_Is70()
    {
        var options = new NotificationOptions();
        options.HumidityThreshold.Should().Be(70);
    }

    [Fact]
    public void KafkaOptions_DefaultBrokers_IsLocalhost9092()
    {
        var options = new KafkaOptions();
        options.Brokers.Should().Be("localhost:9092");
    }

    [Fact]
    public void KafkaOptions_DefaultGroupId_IsNotificationService()
    {
        var options = new KafkaOptions();
        options.GroupId.Should().Be("notification-service");
    }
}
