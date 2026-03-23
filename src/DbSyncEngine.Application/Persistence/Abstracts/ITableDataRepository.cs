
using DbSyncEngine.Application.Pipelines.Common;

namespace DbSyncEngine.Application.Persistence.Abstracts;

public interface ITableDataRepository
{
    Task<IReadOnlyList<RowData>> ReadChunkAsync(
        string tableName,
        IReadOnlyList<string> columns,
        string keyColumn,
        object? lastKey,
        int batchSize,
        CancellationToken ct);

    Task WriteChunkAsync(
        string tableName,
        IReadOnlyList<string> columns,
        IReadOnlyList<RowData> rows,
        CancellationToken ct);
}