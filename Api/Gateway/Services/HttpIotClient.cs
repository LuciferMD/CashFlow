using System.Text.Json;
using System.Text.Json.Serialization;
using Gateway.Types;

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
        var baseUrl = configuration["Iot:BaseUrl"] ?? "http://localhost:8080";
        httpClient.BaseAddress = new Uri(baseUrl);
        httpClient.DefaultRequestHeaders.Add("X-Api-Key", "test");
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
