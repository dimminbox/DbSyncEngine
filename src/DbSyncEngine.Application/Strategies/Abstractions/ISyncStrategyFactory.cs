using DbSyncEngine.Application.Strategies.Options;

namespace DbSyncEngine.Application.Strategies.Abstractions;

public interface ISyncStrategyFactory
{
    ISyncStrategy Create(SyncEntityConfig config);
}