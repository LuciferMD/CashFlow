using Auth.Infrastructure;
using Auth.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Extensions
{
    public static class ApiExtensions
    {
        public static void AddApiAuthentication(this IServiceCollection services, JwtOptions jwtOptions)
        {
            var publicKey = RsaKeyLoader.LoadPublicKey(jwtOptions.PublicKeyPath);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new()
                    {
                        ValidateIssuer = !string.IsNullOrEmpty(jwtOptions.Issuer),
                        ValidIssuer = jwtOptions.Issuer,
                        ValidateAudience = !string.IsNullOrEmpty(jwtOptions.Audience),
                        ValidAudience = jwtOptions.Audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = publicKey,
                        ValidAlgorithms = [SecurityAlgorithms.RsaSha256]
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Cookies["GuardPass"];
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();
        }

        public static JwtOptions ConfigureJwtOptions(this IServiceCollection services, IConfiguration config, string repoRoot)
        {
            var jwtOptions = config.GetSection(nameof(JwtOptions)).Get<JwtOptions>()
                ?? throw new InvalidOperationException("JwtOptions configuration section is missing.");

            jwtOptions.PrivateKeyPath = RsaKeyLoader.ResolveKeyPath(repoRoot, jwtOptions.PrivateKeyPath);
            jwtOptions.PublicKeyPath = RsaKeyLoader.ResolveKeyPath(repoRoot, jwtOptions.PublicKeyPath);

            services.Configure<JwtOptions>(options =>
            {
                options.ExpiersHours = jwtOptions.ExpiersHours;
                options.PrivateKeyPath = jwtOptions.PrivateKeyPath;
                options.PublicKeyPath = jwtOptions.PublicKeyPath;
                options.Issuer = jwtOptions.Issuer;
                options.Audience = jwtOptions.Audience;
            });

            return jwtOptions;
        }
    }
}
