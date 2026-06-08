using Gateway.Services;

namespace Gateway.Types;

[QueryType]
public static class IotQuery
{    
    public static Task<Iot> GetIot([Service] HttpIotClient client)
        => client.GetMetersAsync();
}
