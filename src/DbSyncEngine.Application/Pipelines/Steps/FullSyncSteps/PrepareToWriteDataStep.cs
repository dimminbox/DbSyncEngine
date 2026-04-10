using DbSyncEngine.Application.Pipelines.Abstractions;
using DbSyncEngine.Application.Pipelines.Common;
using Microsoft.Extensions.Logging;

namespace DbSyncEngine.Application.Pipelines.Steps.FullSyncSteps;

public class PrepareToWriteDataStep : ISyncStep
{
    private readonly ILogger<PrepareToWriteDataStep> _logger;

    public PrepareToWriteDataStep(ILogger<PrepareToWriteDataStep> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(SyncContext ctx, Func<Task> next)
    {
        var rows = ctx.CurrentBatch;

        if (rows == null || rows.Count == 0)
            return next();

        var key = ctx.Config.Source.Key;

        // группируем по ключу
        var grouped = rows.GroupBy(r => r.GetRaw(key));

        // считаем дубли
        int duplicates = grouped.Sum(g => g.Count() - 1);

        if (duplicates > 0)
        {
            _logger.LogWarning(
                "PrepareToWriteStep: detected {Count} duplicate rows for table {Table} by key {Key}",
                duplicates, ctx.Config.Source.Table, key);
        }

        // оставляем только первую строку из каждой группы
        var deduped = grouped.Select(g => g.First()).ToList();

        ctx.CurrentBatch = deduped;

        return next();
    }
}