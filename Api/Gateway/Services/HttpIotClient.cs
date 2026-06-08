using Gateway.Types;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Gateway.Services;

public class HttpIotClient
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public HttpIotClient(HttpClient httpClient, IConfiguration configuration)
    {
        httpClient.BaseAddress = new Uri(configuration["Iot:BaseUrl"]!);
        httpClient.DefaultRequestHeaders.Add("X-Api-Key", configuration["Iot:ApiKey"]!);
        _httpClient = httpClient;
    }

    public async Task<Iot> GetMetersAsync()
    {
        var response = await _httpClient.GetAsync("/meters");
        response.EnsureSuccessStatusCode();

        var devices = await JsonSerializer.DeserializeAsync<List<IotDevice>>(
            await response.Content.ReadAsStreamAsync(),
            _jsonOptions
        );

        return new Iot { Devices = devices ?? [] };
    }
}
