using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.Kafka;

namespace NotificationTests.Service;

/// <summary>
/// Shared fixture: one Testcontainers Kafka broker + one in-process Notification host.
/// </summary>
public sealed class NotificationServiceFixture : IAsyncLifetime
{
    private readonly KafkaContainer _kafka = new KafkaBuilder().Build();

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public string BootstrapServers => _kafka.GetBootstrapAddress();

    public async Task InitializeAsync()
    {
        await _kafka.StartAsync();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Kafka:Brokers", _kafka.GetBootstrapAddress());
                builder.UseSetting("Kafka:GroupId", $"notification-svc-{Guid.NewGuid():N}");
                builder.UseSetting("Kafka:Topic:IotSnapshots", "iot.snapshots");
                builder.UseSetting("Telegram:BotToken", "");
                builder.UseSetting("Telegram:ChatId", "");
                builder.UseSetting("Notification:HumidityThreshold", "60");
            });

        // Allow the Kafka background consumer to connect and subscribe.
        await Task.Delay(TimeSpan.FromSeconds(3));
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _kafka.DisposeAsync();
    }
}

[CollectionDefinition(nameof(NotificationServiceCollection))]
public sealed class NotificationServiceCollection
    : ICollectionFixture<NotificationServiceFixture>;
