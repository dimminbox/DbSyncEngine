using System.Linq.Expressions;
using System.Reflection;
using DbSyncEngine.Application.Persistence.Abstracts;
using DbSyncEngine.Application.Pipelines.Abstractions;
using DbSyncEngine.Application.Pipelines.Common;
using DbSyncEngine.Application.Pipelines.Steps.Configs;
using DbSyncEngine.Application.Providers.Abstractions;
using DbSyncEngine.Domain.SyncEntityAggregate;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DbSyncEngine.Application.Pipelines.Steps.Common;

public class ReadDataStep : ISyncStep
{
    private readonly ITableDataRepository _repository;
    private readonly SyncEntityDescriptor _descriptor;
    private readonly ILogger<ReadDataStep> _logger;

    public ReadDataStep(
        ITableDataRepository repository,
        SyncEntityDescriptor descriptor,
        ILogger<ReadDataStep> logger)
    {
        _repository = repository;
        _descriptor = descriptor;
        _logger = logger;
    }

    public async Task HandleAsync(SyncContext ctx, Func<Task> next)
    {
        var table = _descriptor.SourceTable.Value;
        var columns = _descriptor.Columns.Select(c => c.Value).ToList();
        var key = _descriptor.Keys.First();

        var lastKeyString = ctx.Process.LastProcessedKey?.ToString();
        object? lastKey = null;

        if (!string.IsNullOrEmpty(lastKeyString))
        {
            lastKey = Convert.ChangeType(lastKeyString, key.Type);
        }

        _logger.LogInformation(
            "Reading chunk from {Table}, starting from key = {Key}, size = {Size}",
            table, lastKeyString, ctx.Options.ChunkSize);

        var rows = await _repository.ReadChunkAsync(
            tableName: table,
            columns: columns,
            keyColumn: key.Column.Value,
            lastKey: lastKey,
            batchSize: ctx.Options.ChunkSize,
            ct: ctx.CancellationToken);

        _logger.LogInformation("Read {Count} rows", rows.Count);

        if (rows.Count > 0)
        {
            var lastRow = rows.Last();
            var newKey = key.Extract(lastRow); // домен извлекает ключ из RowData
            ctx.Process.UpdateProgress(newKey);
        }

        ctx.CurrentBatch = rows;

        await next();
    }
}