using Elastic.Apm.NetCoreAll;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Auth.Infrastructure;

public static class ObservabilityExtensions
{
    public static ConfigureHostBuilder UseCashFlowSerilog(this ConfigureHostBuilder host)
    {
        host.UseSerilog((context, services, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext();

            var elasticsearchUri = context.Configuration["Elasticsearch:Uri"];
            if (!string.IsNullOrWhiteSpace(elasticsearchUri))
            {
                loggerConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUri))
                {
                    IndexFormat = "cashflow-logs-{0:yyyy.MM.dd}",
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv8,
                    NumberOfShards = 1,
                    NumberOfReplicas = 0,
                    BatchPostingLimit = 50,
                    Period = TimeSpan.FromSeconds(2),
                });
            }
        });

        return host;
    }

    public static IServiceCollection AddCashFlowElasticApm(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration["ElasticApm:ServerUrl"]))
            services.AddAllElasticApm();

        return services;
    }
}
