using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
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
        await action();
        return await ConsumeKafkaMessageAsync(bootstrapServers, topic, timeout);
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
