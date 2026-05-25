using DbSyncEngine.Application.Pipelines.Common;

namespace DbSyncEngine.Application.Persistence.Schema;

public interface ISequenceSynchronizer
{
    Task SyncAsync(SyncContext ctx, string columnName, CancellationToken ct);
}
