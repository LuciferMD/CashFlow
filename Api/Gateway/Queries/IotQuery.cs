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
        CancellationToken cancellationToken)
    {
        var iot = await client.GetMetersAsync();

        if (iot.Devices.Count > 0)
            await publisher.PublishAsync(iot, cancellationToken);

        return iot;
    }
}
