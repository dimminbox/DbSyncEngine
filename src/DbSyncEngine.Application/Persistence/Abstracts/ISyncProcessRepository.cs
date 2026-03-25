using DbSyncEngine.Domain.SyncProcessAggregate;
using DbSyncEngine.Domain.SyncProcessAggregate.Enums;

namespace DbSyncEngine.Application.Persistence.Abstracts;

public interface ISyncProcessRepository
{
    Task<SyncProcess?> GetAsync(
        string entityName,
        string sourceProvider,
        string targetProvider,
        SyncDirection direction,
        CancellationToken ct);

    Task SaveAsync(SyncProcess process, CancellationToken ct);
}