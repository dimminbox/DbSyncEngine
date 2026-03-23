using DbSyncEngine.Application.Strategies.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using DbSyncEngine.Application.Helper;

namespace DbSyncEngine.Application.Strategies.Implementations;

public class SyncStrategyFactory : ISyncStrategyFactory
{
    private readonly IServiceProvider _provider;

    public SyncStrategyFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public ISyncStrategy Create(SyncDirection direction)
    {
        return direction switch
        {
            SyncDirection.FullReloadMySqlToPostgres => _provider
                .GetRequiredService<FullReloadMySqlToPostgresSyncStrategy>(),
            _ => throw new NotSupportedException()
        };
    }
}