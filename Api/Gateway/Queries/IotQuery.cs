using Gateway.Services;
using HotChocolate.Authorization;

namespace Gateway.Types;

[QueryType]
public static class IotQuery
{
    [Authorize]
    public static async Task<Iot> GetIot(
        [Service] HttpIotClient client,
        [Service] IIotSnapshotPublisher publisher,
        [Service] ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(typeof(IotQuery));
        logger.LogInformation("GraphQL GetIot query started.");

        var iot = await client.GetMetersAsync();

        if (iot.Devices.Count > 0)
        {
            logger.LogInformation("Publishing snapshot with {DeviceCount} device(s) to Kafka.", iot.Devices.Count);
            await publisher.PublishAsync(iot, cancellationToken);
        }
        else
        {
            logger.LogWarning("GetIot returned no devices; skipping Kafka publish.");
        }

        logger.LogInformation("GraphQL GetIot query completed with {DeviceCount} device(s).", iot.Devices.Count);

        return iot;
    }
}
