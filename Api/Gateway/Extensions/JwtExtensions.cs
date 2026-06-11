using Gateway.Infrastructure;
using Gateway.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Gateway.Extensions;

public static class JwtExtensions
{
    public static JwtValidationOptions ConfigureJwtValidation(
        this IServiceCollection services,
        IConfiguration config,
        string repoRoot)
    {
        var jwtSection = config.GetSection("JwtOptions");
        var options = new JwtValidationOptions
        {
            PublicKeyPath = jwtSection["PublicKeyPath"]
                ?? throw new InvalidOperationException("JwtOptions:PublicKeyPath is not configured."),
            Issuer = jwtSection["Issuer"] ?? string.Empty,
            Audience = jwtSection["Audience"] ?? string.Empty
        };

        options.PublicKeyPath = RsaKeyLoader.ResolveKeyPath(repoRoot, options.PublicKeyPath);

        services.Configure<JwtValidationOptions>(configured =>
        {
            configured.PublicKeyPath = options.PublicKeyPath;
            configured.Issuer = options.Issuer;
            configured.Audience = options.Audience;
        });

        return options;
    }

    public static void AddJwtValidation(this IServiceCollection services, JwtValidationOptions options)
    {
        var publicKey = RsaKeyLoader.LoadPublicKey(options.PublicKeyPath);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, bearerOptions =>
            {
                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrEmpty(options.Issuer),
                    ValidIssuer = options.Issuer,
                    ValidateAudience = !string.IsNullOrEmpty(options.Audience),
                    ValidAudience = options.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = publicKey,
                    ValidAlgorithms = [SecurityAlgorithms.RsaSha256]
                };

                bearerOptions.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["GuardPass"];

                        if (string.IsNullOrEmpty(context.Token))
                        {
                            var header = context.Request.Headers.Authorization.ToString();
                            if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                                context.Token = header["Bearer ".Length..];
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
    }
}
