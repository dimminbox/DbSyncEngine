using Dapper;
using DbSyncEngine.Application.Persistence;
using DbSyncEngine.Application.Pipelines.Common;
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

        var columnList = columns.Any() ? string.Join(",", columns.Select(c => $"\"{c}\"")) : "*";

        var sql = $@"
            SELECT {columnList}
            FROM {tableName}
            {whereClause}
            ORDER BY {keyColumn}
            LIMIT @batchSize";


        await using var reader = await _connection.ExecuteReaderAsync(
            new CommandDefinition(sql, new { lastKeyTyped, batchSize }, cancellationToken: ct));

        var result = new List<RowData>();
        while (await reader.ReadAsync(ct))
        {
            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                dict[name] = value;
            }
            result.Add(new RowData(dict));
        }

        return result;
    }

    public async Task WriteChunkAsync(
        string tableName,
        IReadOnlyList<string> columns,
        IReadOnlyList<RowData> rows,
        CancellationToken ct)
    {
        if (rows.Count == 0)
            return;

        await _connection.OpenAsync(ct);
        using var tx = await _connection.BeginTransactionAsync(ct);

        try
        {
            var colList = string.Join(",", columns.Select(c => $"`{c}`"));
            var paramList = string.Join(",", columns.Select(c => $"@{c}"));
            var sql = $"INSERT INTO `{tableName}` ({colList}) VALUES ({paramList});";

            foreach (var row in rows)
            {
                try
                {
                    var param = new DynamicParameters();

                    foreach (var col in columns)
                    {
                        row.TryGetValue(col, out var v);
                        param.Add(col, v);
                    }

                    await _connection.ExecuteAsync(
                        new CommandDefinition(sql, param, tx, cancellationToken: ct));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Bad row detected:");
                    foreach (var kv in row.Values)
                        Console.WriteLine($"   {kv.Key} = {kv.Value}");

                    Console.WriteLine($"   Error: {ex.Message}");
                    throw; // пробрасываем, чтобы сработал внешний catch и откатил транзакцию
                }
            }

            await tx.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            Console.WriteLine($"[ERROR] Insert batch failed. Rolling back. Error: {ex.Message}");
            throw;
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }
}