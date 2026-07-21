using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GatewayTests.Service;

internal static class ServiceTestHelpers
{
    internal const string IotQuery = "query { iot { devices { name type } } }";

    internal const string SingleDeviceJson = """
        [{"type":"sensor","name":"Kitchen","payload":{"co2":400,"pm25":10,"humidity":60,"energy":1.5}}]
        """;

    internal const string EmptyDevicesJson = "[]";

    internal static StringContent ToGraphQlContent(string? query = null) =>
        new(
            JsonSerializer.Serialize(new { query = query ?? IotQuery }),
            Encoding.UTF8,
            "application/json");

    internal static async Task<HttpResponseMessage> PostGraphQlAsync(
        WebApplicationFactory<Program> factory,
        string? jwt = null)
    {
        var client = factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "/graphql")
        {
            Content = ToGraphQlContent(),
        };

        if (jwt is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        }

        return await client.SendAsync(request);
    }

    internal static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(body);
    }

    internal static void AssertGraphQlUnauthorized(JsonDocument doc)
    {
        doc.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetArrayLength().Should().BeGreaterThan(0);

        if (!doc.RootElement.TryGetProperty("data", out var data))
        {
            return;
        }

        if (data.ValueKind == JsonValueKind.Null)
        {
            return;
        }

        data.TryGetProperty("iot", out var iot).Should().BeTrue();
        iot.ValueKind.Should().Be(JsonValueKind.Null);
    }

    internal static async Task<string?> ConsumeKafkaMessageAfterAsync(
        string bootstrapServers,
        string topic,
        Func<Task> action,
        TimeSpan timeout)
    {
        await EnsureTopicExistsAsync(bootstrapServers, topic);

        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = $"gateway-svc-{Guid.NewGuid():N}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(topic);

        using var cts = new CancellationTokenSource(timeout);
        await WaitForPartitionAssignmentAsync(consumer, cts.Token);

        // Produce only after the consumer is assigned so the message cannot be missed
        // while the group is still joining (common flake on slow CI runners).
        await action();

        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromMilliseconds(500));
                if (result?.Message?.Value is not null)
                {
                    return result.Message.Value;
                }
            }
            catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
            {
                await Task.Delay(200, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        return null;
    }

    internal static async Task EnsureTopicExistsAsync(string bootstrapServers, string topic)
    {
        using var admin = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = bootstrapServers,
        }).Build();

        try
        {
            await admin.CreateTopicsAsync(
            [
                new TopicSpecification
                {
                    Name = topic,
                    NumPartitions = 1,
                    ReplicationFactor = 1,
                },
            ]);
        }
        catch (CreateTopicsException ex) when (ex.Results.All(r =>
            r.Error.Code is ErrorCode.TopicAlreadyExists or ErrorCode.NoError))
        {
            // Topic already present from a previous produce/auto-create.
        }
    }

    internal static async Task WaitForPartitionAssignmentAsync(
        IConsumer<string, string> consumer,
        CancellationToken cancellationToken)
    {
        // Trigger join / metadata refresh; assignment may appear after a few Consume polls.
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                _ = consumer.Consume(TimeSpan.FromMilliseconds(200));
            }
            catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
            {
                // Keep polling until metadata catches up.
            }

            if (consumer.Assignment.Count > 0)
            {
                return;
            }

            await Task.Delay(100, cancellationToken);
        }

        throw new TimeoutException("Timed out waiting for Kafka consumer partition assignment.");
    }

    internal static async Task<string?> ConsumeKafkaMessageAsync(
        string bootstrapServers,
        string topic,
        TimeSpan timeout)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = $"gateway-svc-{Guid.NewGuid():N}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(topic);

        using var cts = new CancellationTokenSource(timeout);
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromMilliseconds(500));
                if (result?.Message?.Value is not null)
                {
                    return result.Message.Value;
                }
            }
            catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
            {
                await Task.Delay(200, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        return null;
    }
}
