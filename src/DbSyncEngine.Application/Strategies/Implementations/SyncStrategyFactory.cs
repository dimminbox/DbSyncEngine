using DbSyncEngine.Application.Exceptions;
using DbSyncEngine.Application.Strategies.Abstractions;
using DbSyncEngine.Application.Strategies.Options;
using DbSyncEngine.Domain.SyncProcessAggregate.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace DbSyncEngine.Application.Strategies.Implementations;

public class SyncStrategyFactory : ISyncStrategyFactory
{
    private readonly IServiceProvider _provider;

    public SyncStrategyFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public ISyncStrategy Create(SyncEntityConfig config)
    {
        return config.Direction switch
        {
            SyncDirection.Full => new FullSyncStrategy(_provider, config),
            _ => throw new InvalidStrategyException($"Invalid strategy {config.Direction}")
        };
    }
}