using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;

namespace NotificationTests.Service;

internal static class ServiceTestHelpers
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    internal static object BuildSnapshot(
        double? humidity = 45,
        string deviceName = "Kitchen",
        DateTime? capturedAt = null) =>
        new
        {
            capturedAt = capturedAt ?? DateTime.UtcNow,
            devices = new[]
            {
                new
                {
                    type = "air_quality",
                    name = deviceName,
                    payload = new
                    {
                        co2 = (double?)400,
                        pm25 = (double?)10,
                        humidity,
                        energy = (double?)null,
                    },
                },
            },
        };

    internal static StringContent ToJsonContent(object body) =>
        new(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");

    internal static async Task<HubConnection> ConnectHubAsync(WebApplicationFactory<Program> factory)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(factory.Server.BaseAddress, "hubs/notifications"), options =>
            {
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();
        return connection;
    }

    internal static async Task ProduceKafkaMessageAsync(
        string bootstrapServers,
        object snapshot,
        CancellationToken ct = default)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All,
        };

        using var producer = new ProducerBuilder<string, string>(config).Build();
        var json = JsonSerializer.Serialize(snapshot, JsonOptions);

        await producer.ProduceAsync(
            "iot.snapshots",
            new Message<string, string> { Key = "test", Value = json },
            ct);
    }

    internal static async Task<T> WaitForHubEventAsync<T>(
        HubConnection connection,
        string eventName,
        TimeSpan timeout)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

        IDisposable subscription = connection.On<T>(eventName, payload => tcs.TrySetResult(payload));

        try
        {
            using var cts = new CancellationTokenSource(timeout);
            return await tcs.Task.WaitAsync(cts.Token);
        }
        finally
        {
            subscription.Dispose();
        }
    }
}
