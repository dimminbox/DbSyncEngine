using DbSyncEngine.Application.Persistence;
using DbSyncEngine.Application.Pipelines.Abstractions;
using DbSyncEngine.Application.Pipelines.Common;
using Microsoft.Extensions.Logging;

namespace DbSyncEngine.Application.Pipelines.Steps.FullSyncSteps;

public class ReadDataStep : ISyncStep
{
    private readonly ITableDataRepositoryFactory _factory;
    private readonly ILogger _logger;

    public ReadDataStep(
        ITableDataRepositoryFactory factory,
        ILogger<ReadDataStep> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task HandleAsync(SyncContext ctx, Func<Task> next)
    {
        var repo = _factory.Create(ctx.Config.Source.Provider, ctx.Config.Source.ConnectionString);

        _logger.LogInformation(
            "Reading chunk from {Table}, starting from key = {Key}, size = {Size}",
            ctx.Config.Source.Table, ctx.Process.LastProcessedKey, ctx.Config.ChunkSize);

        var rows = await repo.ReadChunkAsync(
            ctx.Config.Source.Table,
            ctx.Config.Source.Columns,
            ctx.Config.Source.Key,
            ctx.Process.LastProcessedKey,
            ctx.Process.LastProcessedKeyType,
            ctx.Config.ChunkSize,
            ctx.CancellationToken);

        _logger.LogInformation("Read {Count} rows", rows.Count);

        if (rows.Count > 0)
        {
            var lastRow = rows.Last();
            var newKey = lastRow.GetRaw(ctx.Config.Source.Key);

            if (newKey is null)
                throw new InvalidOperationException(
                    $"Key column '{ctx.Config.Source.Key}' returned null in last row");
        }

        ctx.CurrentBatch = rows;

        await next();
    }
}