using DbSyncEngine.Domain.SyncProcessAggregate;
using DbSyncEngine.Domain.SyncProcessAggregate.Enums;

namespace DbSyncEngine.Application.Persistence.Abstracts;

public interface ISyncProcessRepository
{
    Task<SyncProcess?> GetAsync(long id, CancellationToken ct);
    Task<SyncProcess?> GetByDirectionAsync(SyncDirection direction, CancellationToken ct);
    Task SaveAsync(SyncProcess process, CancellationToken ct);
}