using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.Json;
using Auth.Controllers;
using Auth.Extensions;
using Auth.Infrastructure;
using Auth.Interfaces;
using Auth.Models;
using Auth.Repositories;
using Auth.Repositories.Context;
using Auth.Repositories.Models;
using Auth.Services;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace AuthTests.Contract;

/// <summary>
/// Auth host on a real Kestrel TCP port with Testcontainers PostgreSQL for Pact provider verification.
/// </summary>
public sealed class AuthContractFixture : IAsyncLifetime, IDisposable
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private string? _keyDirectory;
    private WebApplication _app = null!;

    public Uri ServerUri { get; private set; } = null!;

    public IServiceProvider Services => _app.Services;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var (privateKeyPath, publicKeyPath) = GenerateRsaKeyPair();
        _keyDirectory = Path.GetDirectoryName(privateKeyPath);

        var serverPort = AllocateTcpPort();
        ServerUri = new Uri($"http://127.0.0.1:{serverPort}");

        var repoRoot = RepoRoot.Find();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseKestrel();
        builder.WebHost.UseUrls(ServerUri.ToString());
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString(),
            ["JwtOptions:PrivateKeyPath"] = privateKeyPath,
            ["JwtOptions:PublicKeyPath"] = publicKeyPath,
            ["JwtOptions:Issuer"] = "CashFlow.Auth",
            ["JwtOptions:Audience"] = "CashFlow",
            ["JwtOptions:ExpiersHours"] = "1",
        });

        var jwtOptions = builder.Services.ConfigureJwtOptions(builder.Configuration, repoRoot);
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
                policy
                    .WithOrigins("https://localhost:5173", "https://localhost:3000")
                    .AllowCredentials()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(AuthController).Assembly);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IJwtProvider, JwtProvider>();
        builder.Services.AddApiAuthentication(jwtOptions);
        builder.Services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
                .UseSnakeCaseNamingConvention());

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

        _app.MapPost("/pact/provider-states", async (HttpContext context) =>
        {
            var request = await JsonSerializer.DeserializeAsync<ProviderStateRequest>(
                context.Request.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (request?.State is not null)
            {
                await HandleProviderStateAsync(request.State);
            }
        });

        _app.MapControllers();
        await _app.StartAsync();

        await MigrateDatabaseAsync();
    }

    public async Task HandleProviderStateAsync(string state)
    {
        switch (state)
        {
            case "user exists with valid credentials":
                await ResetDatabaseAsync();
                var constants = ContractPaths.LoadConstants();
                await SeedLoginUserAsync(
                    constants.LoginEmail,
                    constants.LoginPassword,
                    constants.UserName);
                break;

            case "registration is available":
            case "credentials are invalid":
                await ResetDatabaseAsync();
                break;

            default:
                throw new InvalidOperationException($"Unknown provider state: {state}");
        }
    }

    public async Task ResetDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await db.Database.ExecuteSqlRawAsync("""TRUNCATE TABLE users RESTART IDENTITY CASCADE;""");
    }

    public async Task SeedLoginUserAsync(string email, string password, string userName)
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var passwordHash = PasswordHasher.Generate(password);
        var user = User.Create(Guid.NewGuid(), userName, email, passwordHash);
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    public string CreateContractJwt(Guid userId)
    {
        using var scope = Services.CreateScope();
        var jwtProvider = scope.ServiceProvider.GetRequiredService<IJwtProvider>();
        var user = User.Create(userId, "contract-user", ContractPaths.LoadConstants().LoginEmail, "hash");
        return jwtProvider.GenerateToken(user);
    }

    public async Task DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
        await _postgres.DisposeAsync();

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

    private async Task MigrateDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .Options;

        await using var db = new AuthDbContext(options);
        await db.Database.MigrateAsync();
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

        var directory = Path.Combine(Path.GetTempPath(), $"auth-contract-keys-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        var privateKeyPath = Path.Combine(directory, "jwt-private.pem");
        var publicKeyPath = Path.Combine(directory, "jwt-public.pem");

        File.WriteAllText(privateKeyPath, rsa.ExportPkcs8PrivateKeyPem());
        File.WriteAllText(publicKeyPath, rsa.ExportSubjectPublicKeyInfoPem());

        return (privateKeyPath, publicKeyPath);
    }

    private sealed class ProviderStateRequest
    {
        public string? State { get; set; }
    }
}

[CollectionDefinition(nameof(AuthContractCollection))]
public sealed class AuthContractCollection : ICollectionFixture<AuthContractFixture>;
