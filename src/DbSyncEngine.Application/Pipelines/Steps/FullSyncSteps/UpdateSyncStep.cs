using DbSyncEngine.Application.Persistence;
using DbSyncEngine.Application.Pipelines.Abstractions;
using DbSyncEngine.Application.Pipelines.Common;
using Microsoft.Extensions.Logging;

namespace DbSyncEngine.Application.Pipelines.Steps.FullSyncSteps;

public class UpdateSyncStep : ISyncStep
{
    private readonly ISyncProcessRepositoryFactory _factory;
    private readonly ILogger<UpdateSyncStep> _logger;

    public UpdateSyncStep(
        ISyncProcessRepositoryFactory factory,
        ILogger<UpdateSyncStep> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task HandleAsync(SyncContext ctx, Func<Task> next)
    {
        var repo = _factory.Create();
        var rows = ctx.CurrentBatch;
        var config = ctx.Config;

        // если данных нет — значит синхронизация завершена
        if (rows == null || rows.Count == 0)
        {
            ctx.Process.MarkCompleted();
            await repo.SaveAsync(ctx.Process, ctx.CancellationToken);

            _logger.LogInformation(
                "Full sync completed for entity {Entity} ({Source} → {Target})",
                config.Name,
                config.Source.Provider,
                config.Target.Provider);

            return;
        }

        // обновляем прогресс по ключу
        var lastRow = rows.Last();
        var lastKeyValue = lastRow.GetRaw(config.Key);

        if (lastKeyValue is null)
            throw new InvalidOperationException(
                $"Key column '{config.Key}' returned null in last row");

        ctx.Process.UpdateProgress(lastKeyValue);

        await repo.SaveAsync(ctx.Process, ctx.CancellationToken);

        _logger.LogInformation(
            "Updated sync state for {Entity}: key={Key}, totalRows={Rows}",
            config.Name,
            lastKeyValue,
            ctx.Process.TotalProcessedRows);

        await next();
    }
}