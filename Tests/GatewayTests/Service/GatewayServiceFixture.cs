using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.Kafka;

namespace GatewayTests.Service;

/// <summary>
/// Shared fixture: Testcontainers Kafka + WireMock (IoT upstream) + in-process Gateway host.
/// </summary>
public sealed class GatewayServiceFixture : IAsyncLifetime
{
    private const ushort WireMockPort = 8080;

    private readonly KafkaContainer _kafka = new KafkaBuilder().Build();
    private readonly IContainer _wireMock = new ContainerBuilder()
        .WithImage("wiremock/wiremock:3.5.4")
        .WithPortBinding(WireMockPort, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(
            request => request.ForPort(WireMockPort).ForPath("/__admin/health")))
        .Build();

    private string? _keyDirectory;
    private string _privateKeyPath = string.Empty;
    private Microsoft.IdentityModel.Tokens.RsaSecurityKey _signingKey = null!;
    private HttpClient _wireMockClient = null!;

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public string BootstrapServers => _kafka.GetBootstrapAddress();

    public string IotApiKey => "test-api-key";

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_kafka.StartAsync(), _wireMock.StartAsync());

        var (privateKeyPath, publicKeyPath) = GenerateRsaKeyPair();
        _privateKeyPath = privateKeyPath;
        _keyDirectory = Path.GetDirectoryName(privateKeyPath);
        _signingKey = LoadPrivateKey(_privateKeyPath);

        var host = _wireMock.Hostname;
        var port = _wireMock.GetMappedPublicPort(WireMockPort);
        var iotBaseUrl = $"http://{host}:{port}/";

        _wireMockClient = new HttpClient { BaseAddress = new Uri(iotBaseUrl) };

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Kafka:Brokers", _kafka.GetBootstrapAddress());
                builder.UseSetting("Kafka:Topic:IotSnapshots", "iot.snapshots");
                builder.UseSetting("Iot:BaseUrl", iotBaseUrl);
                builder.UseSetting("Iot:ApiKey", IotApiKey);
                builder.UseSetting("JwtOptions:PublicKeyPath", publicKeyPath);
                builder.UseSetting("JwtOptions:Issuer", "CashFlow.Auth");
                builder.UseSetting("JwtOptions:Audience", "CashFlow");
            });

        await ConfigureIotStubAsync(ServiceTestHelpers.SingleDeviceJson);
    }

    public async Task ConfigureIotStubAsync(
        string responseBody,
        int statusCode = 200,
        IDictionary<string, string>? responseHeaders = null)
    {
        await _wireMockClient.PostAsync("/__admin/reset", null);

        var response = new Dictionary<string, object>
        {
            ["status"] = statusCode,
            ["body"] = responseBody,
        };

        if (responseHeaders is not null)
        {
            response["headers"] = responseHeaders;
        }

        var mapping = new
        {
            request = new
            {
                method = "GET",
                urlPath = "/meters",
                headers = new Dictionary<string, object>
                {
                    ["X-Api-Key"] = new { equalTo = IotApiKey },
                },
            },
            response,
        };

        var json = JsonSerializer.Serialize(mapping);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var mappingResponse = await _wireMockClient.PostAsync("/__admin/mappings", content);
        mappingResponse.EnsureSuccessStatusCode();
    }

    public string CreateJwt(string userId = "gateway-test-user")
    {
        var signingCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            _signingKey,
            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.RsaSha256);

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: "CashFlow.Auth",
            audience: "CashFlow",
            claims: [new System.Security.Claims.Claim("userId", userId)],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: signingCredentials);

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task DisposeAsync()
    {
        _wireMockClient.Dispose();
        await Factory.DisposeAsync();
        await _kafka.DisposeAsync();
        await _wireMock.DisposeAsync();

        if (_keyDirectory is not null && Directory.Exists(_keyDirectory))
        {
            Directory.Delete(_keyDirectory, recursive: true);
        }
    }

    private static (string PrivateKeyPath, string PublicKeyPath) GenerateRsaKeyPair()
    {
        using var rsa = RSA.Create(2048);

        var directory = Path.Combine(Path.GetTempPath(), $"gateway-test-keys-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        var privateKeyPath = Path.Combine(directory, "jwt-private.pem");
        var publicKeyPath = Path.Combine(directory, "jwt-public.pem");

        File.WriteAllText(privateKeyPath, rsa.ExportPkcs8PrivateKeyPem());
        File.WriteAllText(publicKeyPath, rsa.ExportSubjectPublicKeyInfoPem());

        return (privateKeyPath, publicKeyPath);
    }

    private static Microsoft.IdentityModel.Tokens.RsaSecurityKey LoadPrivateKey(string privateKeyPath)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(privateKeyPath));
        return new Microsoft.IdentityModel.Tokens.RsaSecurityKey(rsa);
    }
}

[CollectionDefinition(nameof(GatewayServiceCollection))]
public sealed class GatewayServiceCollection : ICollectionFixture<GatewayServiceFixture>;
