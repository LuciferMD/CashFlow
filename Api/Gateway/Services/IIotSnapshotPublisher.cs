using Gateway.Types;

namespace Gateway.Services;

public interface IIotSnapshotPublisher
{
    Task PublishAsync(Iot iot, CancellationToken cancellationToken = default);
}
