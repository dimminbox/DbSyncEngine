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
    private readonly SyncEntityConfig _config;

    public FullSyncStrategy(
        IServiceProvider provider,
        SyncEntityConfig config)
    {
        _provider = provider;
        _config = config;
    }

    public Task RunAsync(CancellationToken ct)
    {
        using var scope = _provider.CreateScope();

        var steps = new List<ISyncStep>
        {
            scope.ServiceProvider.GetRequiredService<GetSyncStep>(),
            scope.ServiceProvider.GetRequiredService<EnsureTargetSchemaStep>(),
            scope.ServiceProvider.GetRequiredService<ReadDataStep>(),
            scope.ServiceProvider.GetRequiredService<MapChunkStep>(),
            scope.ServiceProvider.GetRequiredService<PrepareToWriteDataStep>(),
            scope.ServiceProvider.GetRequiredService<WriteDataStep>(),
            scope.ServiceProvider.GetRequiredService<UpdateSyncStep>(),
        };


        var pipeline = new SyncPipeline(steps, _config);
        return pipeline.RunAsync(SyncDirection.Full, ct);
    }
}