using DbSyncEngine.Application.Entities;
using DbSyncEngine.Application.Pipelines.Abstractions;
using DbSyncEngine.Application.Pipelines.Common;
using DbSyncEngine.Application.Pipelines.Steps.Configs;
using DbSyncEngine.Application.Providers.Abstractions;
using MicroOrm.Dapper.Repositories.Config;
using MicroOrm.Dapper.Repositories.SqlGenerator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Relef.Repository.Interfaces;
using Relef.Repository.Models;

namespace DbSyncEngine.Application.Pipelines.Steps.FullReloadFromMySqlToPostgresSteps;

public class UpdateFullReloadStateStep<T> : ISyncStep where T : Base
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<UpdateFullReloadStateStep<T>> _logger;
    private readonly ReloadEntityConfig<T> _config;

    public UpdateFullReloadStateStep(IPostgresProvider provider, ILogger<UpdateFullReloadStateStep<T>> logger,
        ReloadEntityConfig<T> config)
    {
        _provider = provider.Provider;
        _logger = logger;
        _config = config;
    }

    public async Task HandleAsync(SyncContext ctx, Func<Task> next)
    {
        MicroOrmConfig.SqlProvider = SqlProvider.PostgreSQL;

        var repo = _provider.GetRequiredService<IRepository<ReloadState>>();

        if (ctx.SourceChunk == null || ctx.SourceChunk.Count == 0)
        {
            ctx.ReloadState.IsCompleted = true;
            ctx.ReloadState.LastUpdatedUtc = DateTime.UtcNow;
            await repo.UpdateAsync(ctx.ReloadState);
            _logger.LogInformation("Full reload completed for {Entity}", typeof(T).Name);
            return;
        }

        var lastItem = (T)ctx.SourceChunk.Last();

        var lastKey = _config.KeyValueExtractor(lastItem);
        var lastKeyString = lastKey?.ToString();


        ctx.ReloadState.LastProcessedKey = lastKeyString;
        ctx.ReloadState.TotalProcessedRows += ctx.SourceChunk.Count;
        ctx.ReloadState.LastUpdatedUtc = DateTime.UtcNow;

        (await repo.UpdateAsync(ctx.ReloadState)).ThrowIfFailed();

        _logger.LogInformation("Updated full reload state for {Entity}: key={Key}, totalRows={Rows}", typeof(T).Name,
            lastKeyString, ctx.ReloadState.TotalProcessedRows);

        await next();
    }
}