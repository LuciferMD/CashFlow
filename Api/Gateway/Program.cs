using DotNetEnv;
using Gateway.Extensions;
using Gateway.Infrastructure;
using Gateway.Services;
using Microsoft.AspNetCore.CookiePolicy;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var repoRoot = RepoRoot.Find();
    var envPath = System.IO.Path.Combine(repoRoot, ".env");
    if (File.Exists(envPath))
        Env.Load(envPath);

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseCashFlowSerilog();
    builder.Services.AddCashFlowElasticApm(builder.Configuration);

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

    var app = builder.Build();

    app.UseSerilogRequestLogging();

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

    var kafkaBrokers = builder.Configuration["Kafka:Brokers"];
    var kafkaTopic = builder.Configuration["Kafka:Topic:IotSnapshots"];
    var iotBaseUrl = builder.Configuration["Iot:BaseUrl"];
    Log.Information(
        "Gateway service starting in {Environment}. IoT base URL: {IotBaseUrl}, Kafka brokers: {Brokers}, topic: {Topic}",
        app.Environment.EnvironmentName,
        iotBaseUrl,
        kafkaBrokers,
        kafkaTopic);

    app.RunWithGraphQLCommands(args);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Gateway service terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Exposed for WebApplicationFactory in service tests.
public partial class Program;
