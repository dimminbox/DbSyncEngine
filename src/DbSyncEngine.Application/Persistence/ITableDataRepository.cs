
using DbSyncEngine.Application.Pipelines.Common;

namespace DbSyncEngine.Application.Persistence;

public interface ITableDataRepository
{
    Task<IReadOnlyList<RowData>> ReadChunkAsync(
        string tableName,
        IReadOnlyList<string> columns,
        string keyColumn,
        string? lastKey,
        string lastKeyType,
        int batchSize,
        CancellationToken ct);

    Task WriteChunkAsync(
        string tableName,
        IReadOnlyList<string> columns,
        IReadOnlyList<RowData> rows,
        CancellationToken ct);
}