using Gateway.Types;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gateway.Services;

public class HttpIotClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpIotClient> _logger;
    private readonly ResiliencePipeline<Iot> _pipeline;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public HttpIotClient(HttpClient httpClient, IConfiguration configuration, ILogger<HttpIotClient> logger)
    {
        httpClient.BaseAddress = new Uri(configuration["Iot:BaseUrl"]!);
        httpClient.DefaultRequestHeaders.Add("X-Api-Key", configuration["Iot:ApiKey"]!);
        _httpClient = httpClient;
        _logger = logger;
        _pipeline = BuildPipeline();
    }

    private static ResiliencePipeline<Iot> BuildPipeline() =>
        new ResiliencePipelineBuilder<Iot>()
            .AddRetry(new RetryStrategyOptions<Iot>
            {
                ShouldHandle = new PredicateBuilder<Iot>()
                    .Handle<HttpRequestException>()
                    .Handle<IotServiceException>(),
                MaxRetryAttempts = 5,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromSeconds(0.5),
                DelayGenerator = args =>
                {
                    if (args.Outcome.Exception is IotServiceException { RetryAfter: { } delay })
                        return ValueTask.FromResult<TimeSpan?>(delay);

                    return ValueTask.FromResult<TimeSpan?>(null);
                },
                OnRetry = args =>
                {

                    return ValueTask.CompletedTask;
                }
            })
            .Build();

    public async Task<Iot> GetMetersAsync()
    {
        try
        {
            return await _pipeline.ExecuteAsync(FetchAndParseAsync);
        }
        catch (Exception ex)
        {
            _logger.LogError("IoT /meters failed after all 5 attempts — {Reason}. Returning empty result.", ex.Message);
            return new Iot { Devices = [] };
        }
    }

    private async ValueTask<Iot> FetchAndParseAsync(CancellationToken ct)
    {
        var response = await _httpClient.GetAsync("/meters", ct);

        // 429 – rate limited: carry Retry-After so the pipeline uses the right delay
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(5);
            throw new IotServiceException("Rate limit exceeded", retryAfter);
        }

        // 500, 504 and any other non-success
        if (!response.IsSuccessStatusCode)
            throw new IotServiceException($"Upstream error: {(int)response.StatusCode}");

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        // 200 {"error": "data corrupted"} or any unexpected object shape
        if (doc.RootElement.ValueKind == JsonValueKind.Object)
            throw new IotServiceException("Corrupted payload received");

        // 200 [] – empty array, nothing to show yet
        if (doc.RootElement.ValueKind != JsonValueKind.Array
            || doc.RootElement.GetArrayLength() == 0)
            throw new IotServiceException("Empty device list");

        var devices = doc.RootElement.Deserialize<List<IotDevice>>(_jsonOptions);
        return new Iot { Devices = devices ?? [] };
    }
}
