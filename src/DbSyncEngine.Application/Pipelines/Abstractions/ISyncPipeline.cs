using DbSyncEngine.Application.Helper;

namespace DbSyncEngine.Application.Pipelines.Abstractions;

public interface ISyncPipeline
{
    Task RunAsync(SyncDirection direction, CancellationToken ct);
}