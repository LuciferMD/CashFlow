using DotNetEnv;
using Gateway.Extensions;
using Gateway.Infrastructure;
using Gateway.Services;
using Microsoft.AspNetCore.CookiePolicy;

var repoRoot = RepoRoot.Find();
var envPath = System.IO.Path.Combine(repoRoot, ".env");
if (File.Exists(envPath))
    Env.Load(envPath);

var builder = WebApplication.CreateBuilder(args);

var jwtOptions = builder.Services.ConfigureJwtValidation(builder.Configuration, repoRoot);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy
            .WithOrigins("https://localhost:5173")
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddJwtValidation(jwtOptions);

builder.AddGraphQL()
    .AddAuthorization()
    .AddTypes();

builder.Services.AddHttpClient<HttpIotClient>();

var app = builder.Build();

app.UseCors("Frontend");

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.SameAsRequest
});

app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL();

app.RunWithGraphQLCommands(args);
