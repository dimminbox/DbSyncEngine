using DbSyncEngine.Application.Exceptions;
using DbSyncEngine.Application.Strategies.Abstractions;
using DbSyncEngine.Application.Strategies.Options;
using DbSyncEngine.Domain.SyncProcessAggregate.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace DbSyncEngine.Application.Strategies.Implementations;

public class SyncStrategyFactory : ISyncStrategyFactory
{
    private readonly IServiceScopeFactory _scopeFactory;

    public SyncStrategyFactory(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public ISyncStrategy Create(SyncEntityConfig config)
    {
        return config.Direction switch
        {
            SyncDirection.Full => new FullSyncStrategy(_scopeFactory, config),
            _ => throw new InvalidStrategyException($"Invalid strategy {config.Direction}")
        };
    }
}
