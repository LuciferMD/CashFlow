using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Gateway.Extensions;
using Gateway.Infrastructure;
using Gateway.Services;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Testcontainers.Kafka;

namespace GatewayTests.Contract;

/// <summary>
/// Gateway host on a real Kestrel TCP port with WireMock IoT upstream for GraphQL contract verification.
/// </summary>
public sealed class GatewayHttpContractFixture : IAsyncLifetime, IDisposable
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
    private RsaSecurityKey _signingKey = null!;
    private HttpClient _wireMockClient = null!;
    private WebApplication _app = null!;

    public Uri ServerUri { get; private set; } = null!;

    public string ContractJwt { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_kafka.StartAsync(), _wireMock.StartAsync());

        var (privateKeyPath, publicKeyPath) = GenerateRsaKeyPair();
        _privateKeyPath = privateKeyPath;
        _keyDirectory = Path.GetDirectoryName(privateKeyPath);
        _signingKey = LoadPrivateKey(_privateKeyPath);
        ContractJwt = CreateJwt();

        var host = _wireMock.Hostname;
        var wireMockPort = _wireMock.GetMappedPublicPort(WireMockPort);
        var iotBaseUrl = $"http://{host}:{wireMockPort}/";

        _wireMockClient = new HttpClient { BaseAddress = new Uri(iotBaseUrl) };

        var serverPort = AllocateTcpPort();
        ServerUri = new Uri($"http://127.0.0.1:{serverPort}");

        var repoRoot = RepoRoot.Find();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseKestrel();
        builder.WebHost.UseUrls(ServerUri.ToString());
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Kafka:Brokers"] = _kafka.GetBootstrapAddress(),
            ["Kafka:Topic:IotSnapshots"] = "iot.snapshots",
            ["Iot:BaseUrl"] = iotBaseUrl,
            ["Iot:ApiKey"] = "test-api-key",
            ["JwtOptions:PublicKeyPath"] = publicKeyPath,
            ["JwtOptions:Issuer"] = "CashFlow.Auth",
            ["JwtOptions:Audience"] = "CashFlow",
        });

        var jwtOptions = builder.Services.ConfigureJwtValidation(builder.Configuration, repoRoot);
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
                policy
                    .WithOrigins("https://localhost:5173", "https://localhost:3000")
                    .AllowCredentials()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });
        builder.Services.AddJwtValidation(jwtOptions);
        builder.AddGraphQL()
            .AddAuthorization()
            .AddTypes();
        builder.Services.AddHttpClient<HttpIotClient>();
        builder.Services.AddKafkaPublishing(builder.Configuration);

        _app = builder.Build();
        _app.UseCors("Frontend");
        _app.UseCookiePolicy(new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.Strict,
            HttpOnly = HttpOnlyPolicy.Always,
            Secure = CookieSecurePolicy.SameAsRequest,
        });
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapGraphQL();

        await _app.StartAsync();
        await ConfigureIotStubAsync();

        const string iotQuery =
            "query GetIot { iot { devices { type name payload { co2 pm25 humidity energy } } } }";

        using var warmup = new HttpClient { BaseAddress = ServerUri };
        using var warmupRequest = new HttpRequestMessage(HttpMethod.Post, "/graphql");
        warmupRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ContractJwt);
        warmupRequest.Content = new StringContent(
            $$"""{"query":"{{iotQuery}}"}""",
            Encoding.UTF8,
            "application/json");
        var warmupResponse = await warmup.SendAsync(warmupRequest);
        var warmupBody = await warmupResponse.Content.ReadAsStringAsync();
        if (!warmupResponse.IsSuccessStatusCode || warmupBody.Contains("\"errors\"", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Gateway contract host warmup failed ({(int)warmupResponse.StatusCode}): {warmupBody}");
        }
    }

    public async Task DisposeAsync()
    {
        _wireMockClient.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
        await _kafka.DisposeAsync();
        await _wireMock.DisposeAsync();

        if (_keyDirectory is not null && Directory.Exists(_keyDirectory))
        {
            Directory.Delete(_keyDirectory, recursive: true);
        }
    }

    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }

    private async Task ConfigureIotStubAsync()
    {
        await _wireMockClient.PostAsync("/__admin/reset", null);

        const string responseBody = """
            [{"type":"sensor","name":"Kitchen","payload":{"co2":400,"pm25":10,"humidity":60,"energy":1.5}}]
            """;

        var mapping = new
        {
            request = new
            {
                method = "GET",
                urlPath = "/meters",
                headers = new Dictionary<string, object>
                {
                    ["X-Api-Key"] = new { equalTo = "test-api-key" },
                },
            },
            response = new
            {
                status = 200,
                body = responseBody,
            },
        };

        using var content = new StringContent(
            JsonSerializer.Serialize(mapping),
            Encoding.UTF8,
            "application/json");

        var response = await _wireMockClient.PostAsync("/__admin/mappings", content);
        response.EnsureSuccessStatusCode();
    }

    private string CreateJwt(string userId = "contract-test-user")
    {
        var signingCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.RsaSha256);
        var token = new JwtSecurityToken(
            issuer: "CashFlow.Auth",
            audience: "CashFlow",
            claims: [new Claim("userId", userId)],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static RsaSecurityKey LoadPrivateKey(string privateKeyPath)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(privateKeyPath));
        return new RsaSecurityKey(rsa);
    }

    private static int AllocateTcpPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static (string PrivateKeyPath, string PublicKeyPath) GenerateRsaKeyPair()
    {
        using var rsa = RSA.Create(2048);

        var directory = Path.Combine(Path.GetTempPath(), $"gateway-contract-keys-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        var privateKeyPath = Path.Combine(directory, "jwt-private.pem");
        var publicKeyPath = Path.Combine(directory, "jwt-public.pem");

        File.WriteAllText(privateKeyPath, rsa.ExportPkcs8PrivateKeyPem());
        File.WriteAllText(publicKeyPath, rsa.ExportSubjectPublicKeyInfoPem());

        return (privateKeyPath, publicKeyPath);
    }
}

[CollectionDefinition(nameof(GatewayHttpContractCollection))]
public sealed class GatewayHttpContractCollection : ICollectionFixture<GatewayHttpContractFixture>;
