using System.Data;
using Dapper;
using DbSyncEngine.Application.Persistence;
using DbSyncEngine.Application.Pipelines.Common;
using DbSyncEngine.Infrastructure.Persistence.Abstractions;
using MySqlConnector;

namespace DbSyncEngine.Infrastructure.Persistence.Repositories;

public class MySqlTableDataRepository : TableDataRepositoryBase, ITableDataRepository
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
        string? lastKey,
        string lastKeyType,
        int batchSize,
        CancellationToken ct)
    {
        var lastKeyTyped = ConvertKey(lastKey, lastKeyType);
        var whereClause = BuildWhereClause(keyColumn, lastKeyTyped);

        var sql = $@"
            SELECT {string.Join(",", columns)}
            FROM ""{tableName}""
            {whereClause}
            ORDER BY ""{keyColumn}""
            LIMIT @batchSize";

        var rows = await _connection.QueryAsync<IDictionary<string, object?>>(
            sql, new { lastKeyTyped, batchSize });

        return rows.Select(r => new RowData(r.AsReadOnly())).ToList();
    }

    public async Task WriteChunkAsync(string tableName, IReadOnlyList<string> columns, IReadOnlyList<RowData> rows,
        CancellationToken ct)
    {
        var bulk = new MySqlBulkCopy(_connection)
        {
            DestinationTableName = tableName,
            BulkCopyTimeout = 0
        };

        var first = rows.FirstOrDefault();
        var columnTypes = new Dictionary<string, Type>();

        if (first != null)
        {
            foreach (var kv in first.Values)
            {
                columnTypes[kv.Key] = kv.Value?.GetType() ?? typeof(object);
            }
        }
        
        var table = new DataTable();
        foreach (var col in columns)
        {
            var type = columnTypes[col];
            table.Columns.Add(col, type);
        }

        foreach (var row in rows)
        {
            var values = columns.Select(c => row.TryGetValue(c, out var v) ? v : null).ToArray();
            table.Rows.Add(values);
        }

        await bulk.WriteToServerAsync(table, ct);
    }
}