using Gateway.Services;
using HotChocolate.Authorization;

namespace Gateway.Types;

[QueryType]
public static class IotQuery
{
    [Authorize]
    public static Task<Iot> GetIot([Service] HttpIotClient client)
    {
        return client.GetMetersAsync();
    }
}
