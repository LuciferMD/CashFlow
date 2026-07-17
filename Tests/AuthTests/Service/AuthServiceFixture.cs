using System.Security.Cryptography;
using Auth.Repositories.Context;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace AuthTests.Service;

/// <summary>
/// Shared fixture: Testcontainers PostgreSQL + in-process Auth host with throw-away RSA keys.
/// </summary>
public sealed class AuthServiceFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private string? _keyDirectory;

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var (privateKeyPath, publicKeyPath) = GenerateRsaKeyPair();
        _keyDirectory = Path.GetDirectoryName(privateKeyPath);

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:DefaultConnection", _postgres.GetConnectionString());
                builder.UseSetting("JwtOptions:PrivateKeyPath", privateKeyPath);
                builder.UseSetting("JwtOptions:PublicKeyPath", publicKeyPath);
                builder.UseSetting("JwtOptions:Issuer", "CashFlow.Auth");
                builder.UseSetting("JwtOptions:Audience", "CashFlow");
                builder.UseSetting("JwtOptions:ExpiersHours", "1");
            });

        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _postgres.DisposeAsync();

        if (_keyDirectory is not null && Directory.Exists(_keyDirectory))
        {
            Directory.Delete(_keyDirectory, recursive: true);
        }
    }

    private static (string PrivateKeyPath, string PublicKeyPath) GenerateRsaKeyPair()
    {
        using var rsa = RSA.Create(2048);

        var directory = Path.Combine(Path.GetTempPath(), $"auth-test-keys-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        var privateKeyPath = Path.Combine(directory, "jwt-private.pem");
        var publicKeyPath = Path.Combine(directory, "jwt-public.pem");

        File.WriteAllText(privateKeyPath, rsa.ExportPkcs8PrivateKeyPem());
        File.WriteAllText(publicKeyPath, rsa.ExportSubjectPublicKeyInfoPem());

        return (privateKeyPath, publicKeyPath);
    }
}

[CollectionDefinition(nameof(AuthServiceCollection))]
public sealed class AuthServiceCollection : ICollectionFixture<AuthServiceFixture>;
