using Confluent.Kafka;
using Gateway.Models;
using Gateway.Services;

namespace Gateway.Extensions;

public static class KafkaExtensions
{
    public static IServiceCollection AddKafkaPublishing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<KafkaOptions>(configuration.GetSection("Kafka"));

        services.AddSingleton<IProducer<string, string>>(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<KafkaOptions>>().Value;
            var config = new ProducerConfig
            {
                BootstrapServers = options.Brokers,
                Acks = Acks.Leader
            };

            return new ProducerBuilder<string, string>(config).Build();
        });

        services.AddSingleton<IIotSnapshotPublisher, IotSnapshotPublisher>();

        return services;
    }
}
