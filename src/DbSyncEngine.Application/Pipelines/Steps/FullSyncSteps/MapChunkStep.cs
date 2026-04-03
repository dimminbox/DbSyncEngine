using DbSyncEngine.Application.Normalization;
using DbSyncEngine.Application.Pipelines.Abstractions;
using DbSyncEngine.Application.Pipelines.Common;
using Microsoft.Extensions.Logging;

namespace DbSyncEngine.Application.Pipelines.Steps.FullSyncSteps;

public class MapChunkStep : ISyncStep
{
    private readonly IValueNormalizerFactory _normalizerFactory;
    private readonly ILogger<MapChunkStep> _logger;

    public MapChunkStep(
        IValueNormalizerFactory normalizerFactory,
        ILogger<MapChunkStep> logger)
    {
        _normalizerFactory = normalizerFactory;
        _logger = logger;
    }

    public Task HandleAsync(SyncContext ctx, Func<Task> next)
    {
        var normalizer = _normalizerFactory.Create(ctx.Config.Source.Provider);

        var batch = ctx.CurrentBatch;

        if (batch == null || batch.Count == 0)
        {
            _logger.LogInformation("No rows to map for table {Table}", ctx.Config.Source.Table);
            return next();
        }

        _logger.LogInformation("Normalizing {Count} rows for table {Table}", batch.Count, ctx.Config.Source.Table);

        var normalized = new List<RowData>(batch.Count);

        var columns = ctx.Config.Columns?.Count > 0
            ? ctx.Config.Columns
            : batch[0].Values.Keys.ToList();

        foreach (var row in batch)
        {
            var newRow = new RowData();

            foreach (var column in columns)
            {
                row.TryGetValue(column, out var value);
                var normalizedValue = normalizer.Normalize(value);
                newRow.Set(column, normalizedValue);
            }   

            normalized.Add(newRow);
        }

        ctx.CurrentBatch = normalized;

        return next();
    }
}