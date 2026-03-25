using DbSyncEngine.Application.Pipelines.Abstractions;
using DbSyncEngine.Application.Pipelines.Common;
using DbSyncEngine.Application.Pipelines.Steps.FullSyncSteps;
using DbSyncEngine.Application.Strategies.Abstractions;
using DbSyncEngine.Application.Strategies.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using DbSyncEngine.Domain.SyncProcessAggregate.Enums;

namespace DbSyncEngine.Application.Strategies.Implementations;

public class FullSyncStrategy : ISyncStrategy
{
    private readonly IServiceProvider _provider;
    private readonly IOptionsMonitor<SyncEntityConfig> _options;

    public FullSyncStrategy(
        IServiceProvider provider,
        IOptionsMonitor<SyncEntityConfig> options)
    {
        _provider = provider;
        _options = options;
    }

    public Task RunAsync(CancellationToken ct)
    {
        using var scope = _provider.CreateScope();

        var steps = new List<ISyncStep>
        {
            scope.ServiceProvider.GetRequiredService<GetSyncStep>(),
            scope.ServiceProvider.GetRequiredService<ReadDataStep>(),
            scope.ServiceProvider.GetRequiredService<MapChunkStep>(),
            scope.ServiceProvider.GetRequiredService<WriteDataStep>(),
            scope.ServiceProvider.GetRequiredService<UpdateSyncStep>(),
        };


        var pipeline = new SyncPipeline(steps, _options);
        return pipeline.RunAsync(SyncDirection.Full, ct);
    }
}