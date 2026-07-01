using Notification.Kafka;

namespace Notification.Services;

public interface ISnapshotProcessor
{
    Task ProcessAsync(IotSnapshotMessage snapshot, CancellationToken ct = default);
}
