using Auth.Extensions;
using Auth.Interfaces;
using Auth.Infrastructure;
using Auth.Models;
using Auth.Repositories;
using Auth.Repositories.Context;
using Auth.Services;
using DotNetEnv;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.EntityFrameworkCore;

var repoRoot = RepoRoot.Find();
var envPath = Path.Combine(repoRoot, ".env");
if (File.Exists(envPath))
    Env.Load(envPath);

var builder = WebApplication.CreateBuilder(args);

var jwtOptions = builder.Services.ConfigureJwtOptions(builder.Configuration, repoRoot);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy
            .WithOrigins("https://localhost:5173")
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository,UserRepository>();
builder.Services.AddScoped<IJwtProvider,JwtProvider>();
builder.Services.AddApiAuthentication(jwtOptions);

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    .UseSnakeCaseNamingConvention());
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");

// Comment out or remove HttpsRedirection in dev ù it causes preflight redirects
//app.UseHttpsRedirection();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.SameAsRequest
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
