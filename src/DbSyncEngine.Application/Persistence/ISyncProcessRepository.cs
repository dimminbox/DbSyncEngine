using DbSyncEngine.Domain.SyncProcessAggregate;
using DbSyncEngine.Domain.SyncProcessAggregate.Enums;

namespace DbSyncEngine.Application.Persistence;

public interface ISyncProcessRepository
{
    void InitDb();
    Task<SyncProcess?> GetAsync(
        string entityName,
        string sourceProvider,
        string targetProvider,
        SyncDirection direction,
        CancellationToken ct);

    Task SaveAsync(SyncProcess process, CancellationToken ct);
}