using System.Globalization;
using DbSyncEngine.Application.Pipelines.Abstractions;
using DbSyncEngine.Application.Pipelines.Common;
using DbSyncEngine.Application.Providers.Abstractions;
using MicroOrm.Dapper.Repositories.Config;
using MicroOrm.Dapper.Repositories.SqlGenerator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Relef.Repository.Interfaces;
using Relef.Repository.Models;

namespace DbSyncEngine.Application.Pipelines.Steps.FullReloadFromMySqlToPostgresSteps;

public class WriteChunkToPostgresStep<TTarget> : ISyncStep where TTarget : Base
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<WriteChunkToPostgresStep<TTarget>> _logger;

    public WriteChunkToPostgresStep(IPostgresProvider provider, ILogger<WriteChunkToPostgresStep<TTarget>> logger)
    {
        _provider = provider.Provider;
        _logger = logger;
    }

    public async Task HandleAsync(SyncContext ctx, Func<Task> next)
    {
        MicroOrmConfig.SqlProvider = SqlProvider.PostgreSQL;

        if (ctx.TargetChunk == null || ctx.TargetChunk.Count == 0)
        {
            _logger.LogInformation("No rows to write for {Entity}", typeof(TTarget).Name);
            await next();
            return;
        }

        var repo = _provider.GetRequiredService<IRepository<TTarget>>();
        var rows = ctx.TargetChunk.Cast<TTarget>().ToList();
        int batchSize = ctx.Options.InsertBatchSize;

        _logger.LogInformation("Writing {Count} rows of {Entity} to Postgres", rows.Count,
            typeof(TTarget).Name);

        var (conn, tx) = await repo.BeginTransactionAsync();

        try
        {
            foreach (var batch in rows.Chunk(batchSize))
            {
                (await repo.InsertBunchAsync(batch.ToList(), default, tx)).ThrowIfFailed();
            }

            var last = rows.Max(r => r.Id);
            ctx.ReloadState.LastProcessedKey = last.ToString(CultureInfo.InvariantCulture);
            ctx.TargetChunk = null;
            ctx.SourceChunk = null;

            await repo.CommitAsync(tx);
        }
        catch
        {
            await repo.RollbackAsync(tx);
            throw;
        }
        finally
        {
            conn.Dispose();
        }


        await next();
    }
}