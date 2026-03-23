using DbSyncEngine.Application.Pipelines.Abstractions;
using DbSyncEngine.Application.Pipelines.Common;
using DbSyncEngine.Application.Pipelines.Steps.Common;
using DbSyncEngine.Application.Pipelines.Steps.FullReloadFromMySqlToPostgresSteps;
using DbSyncEngine.Application.Strategies.Abstractions;
using DbSyncEngine.Application.Strategies.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using DbSyncEngine.Application.Helper;
using Orders.Core.Models.Entites;

namespace DbSyncEngine.Application.Strategies.Implementations;

public class FullReloadMySqlToPostgresSyncStrategy : ISyncStrategy
{
    private readonly IServiceProvider _provider;
    private readonly IOptionsMonitor<FullReloadOptions> _options;

    public FullReloadMySqlToPostgresSyncStrategy(IServiceProvider provider,
        IOptionsMonitor<FullReloadOptions> options)
    {
        _options = options;
        _provider = provider;
    }

    public Task RunAsync(CancellationToken ct)
    {
        using var scope = _provider.CreateScope();

        var pipeline = new SyncPipeline(
            new ISyncStep[]
            {
                _provider.GetRequiredService<PrepareReloadStep>(),
                _provider.GetRequiredService<ReadDataStep>(),
                _provider.GetRequiredService<MapChunkStep<Product, Product>>(),
                _provider.GetRequiredService<WriteChunkToPostgresStep<Product>>(),
                _provider.GetRequiredService<UpdateFullReloadStateStep<Product>>()
            }, _options);

        return pipeline.RunAsync(SyncDirection.FullReloadMySqlToPostgres, ct);
    }
}