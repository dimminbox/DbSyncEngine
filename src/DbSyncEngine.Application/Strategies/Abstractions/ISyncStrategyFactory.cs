using DbSyncEngine.Application.Helper;

namespace DbSyncEngine.Application.Strategies.Abstractions;

public interface ISyncStrategyFactory
{
    ISyncStrategy Create(SyncDirection direction);
}