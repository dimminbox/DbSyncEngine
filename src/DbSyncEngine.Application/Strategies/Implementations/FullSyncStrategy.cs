using DbSyncEngine.Application.Pipelines;
using DbSyncEngine.Application.Pipelines.Common;
using DbSyncEngine.Application.Strategies.Abstractions;
using DbSyncEngine.Application.Strategies.Options;
using DbSyncEngine.Domain.SyncProcessAggregate.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace DbSyncEngine.Application.Strategies.Implementations;

public class FullSyncStrategy : ISyncStrategy
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SyncEntityConfig _config;

    public FullSyncStrategy(IServiceScopeFactory scopeFactory, SyncEntityConfig config)
    {
        _scopeFactory = scopeFactory;
        _config = config;
    }

    public Task RunAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();

        var assembler = scope.ServiceProvider.GetRequiredService<IFullSyncPipelineAssembler>();
        var pipeline = new SyncPipeline(assembler.Steps, _config);

        return pipeline.RunAsync(SyncDirection.Full, ct);
    }
}
