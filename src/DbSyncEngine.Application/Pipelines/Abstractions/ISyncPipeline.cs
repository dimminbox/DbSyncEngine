using DbSyncEngine.Domain.SyncProcessAggregate.Enums;

namespace DbSyncEngine.Application.Pipelines.Abstractions;

public interface ISyncPipeline
{
    Task RunAsync(SyncDirection direction, CancellationToken ct);
}