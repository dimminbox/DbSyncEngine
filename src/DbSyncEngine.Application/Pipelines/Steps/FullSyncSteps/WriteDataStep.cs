using DbSyncEngine.Application.Persistence.Abstracts;
using DbSyncEngine.Application.Pipelines.Abstractions;
using DbSyncEngine.Application.Pipelines.Common;
using Microsoft.Extensions.Logging;

namespace DbSyncEngine.Application.Pipelines.Steps.FullSyncSteps;

public class WriteDataStep : ISyncStep
{
    private readonly ITableDataRepositoryFactory _factory;
    private readonly ILogger<WriteDataStep> _logger;

    public WriteDataStep(
        ITableDataRepositoryFactory factory,
        ILogger<WriteDataStep> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task HandleAsync(SyncContext ctx, Func<Task> next)
    {
        var rows = ctx.CurrentBatch;

        if (rows == null || rows.Count == 0)
        {
            _logger.LogInformation("No rows to write for table {Table}", ctx.Config.Target.Table);
            await next();
            return;
        }

        _logger.LogInformation("Writing {Count} rows to {Table}", rows.Count, ctx.Config.Target.Table);

        var repo = _factory.Create(ctx.Config.Target.Provider, ctx.Config.Target.ConnectionString);

        await repo.WriteChunkAsync(
            tableName: ctx.Config.Target.Table,
            columns: ctx.Config.Columns,
            rows: rows,
            ct: ctx.CancellationToken);

        // обновляем прогресс
        var lastRow = rows.Last();
        var lastKeyValue = lastRow.GetRaw(ctx.Config.Key);

        if (lastKeyValue is null)
            throw new InvalidOperationException(
                $"Key column '{ctx.Config.Key}' returned null in last row");

        ctx.Process.UpdateProgress(lastKeyValue);

        // очищаем batch
        ctx.CurrentBatch = Array.Empty<RowData>();

        await next();
    }
}