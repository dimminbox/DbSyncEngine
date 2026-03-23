using Dapper;
using DbSyncEngine.Application.Persistence.Abstracts;
using DbSyncEngine.Application.Pipelines.Common;
using MySqlConnector;

namespace DbSyncEngine.Infrastructure.Persistence.Repositories;

public class MySqlTableDataRepository : ITableDataRepository
{
    private readonly MySqlConnection _connection;

    public MySqlTableDataRepository(MySqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<IReadOnlyList<RowData>> ReadChunkAsync(
        string tableName,
        IReadOnlyList<string> columns,
        string keyColumn,
        object? lastKey,
        int batchSize,
        CancellationToken ct)
    {
        var sql = $@"
            SELECT {string.Join(",", columns)}
            FROM {tableName}
            {(lastKey != null ? $"WHERE {keyColumn} > @lastKey" : "")}
            ORDER BY {keyColumn}
            LIMIT @batchSize";

        var rows = await _connection.QueryAsync<IDictionary<string, object?>>(
            sql, new { lastKey, batchSize });

        return rows.Select(r => new RowData(r.AsReadOnly())).ToList();
    }

    public async Task WriteChunkAsync(string tableName, IReadOnlyList<string> columns, IReadOnlyList<RowData> rows,
        CancellationToken ct)
    {
        var sql = $@"
            INSERT INTO {tableName} ({string.Join(",", columns)})
            VALUES ({string.Join(",", columns.Select(c => "@" + c))})
            ON DUPLICATE KEY UPDATE
            {string.Join(",", columns.Select(c => $"{c} = VALUES({c})"))}";

        foreach (var row in rows)
        {
            await _connection.ExecuteAsync(sql, row.Values);
        }
    }
}