using System.Data;
using System.Text.RegularExpressions;
using Dapper;
using DbSyncEngine.Application.Persistence;
using DbSyncEngine.Application.Pipelines.Common;
using MySqlConnector;

namespace DbSyncEngine.Infrastructure.Persistence.Repositories;

public class MySqlTableDataRepository : TableDataRepositoryBase, ITableDataRepository
{
    private readonly MySqlConnection _connection;

    protected static readonly Regex SafeIdentifier =
        new("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

    protected static string QuoteMySqlIdentifier(string id)
    {
        if (!SafeIdentifier.IsMatch(id))
            throw new ArgumentException($"Unsafe identifier: {id}");
        return $"`{id}`";
    }

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
        var safeTable = QuoteMySqlIdentifier(tableName);
        var safeKey = QuoteMySqlIdentifier(keyColumn);

        var lastKeyTyped = ConvertKey(lastKey, lastKeyType);
        var whereClause = BuildWhereClause(keyColumn, lastKeyTyped);

        var safeCols = columns.Any()
            ? string.Join(",", columns.Select(QuoteMySqlIdentifier))
            : "*";

        var sql = $@"SELECT {safeCols} FROM {safeTable} {whereClause} ORDER BY {safeKey} LIMIT @batchSize";

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
        int chunkSize,
        CancellationToken ct)
    {
        if (rows.Count == 0)
            return;

        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync(ct);

        using var tx = await _connection.BeginTransactionAsync(ct);

        try
        {
            var colList = string.Join(",", columns.Select(c => $"`{c}`"));

            // Разбиваем на чанки
            foreach (var chunk in rows.Chunk(chunkSize))
            {
                // Формируем VALUES (...), (...), (...)
                var valuesList = new List<string>();
                var parameters = new DynamicParameters();
                var rowIndex = 0;

                foreach (var row in chunk)
                {
                    var paramNames = new List<string>();

                    foreach (var col in columns)
                    {
                        var paramName = $"{col}_{rowIndex}";
                        row.TryGetValue(col, out var v);
                        parameters.Add(paramName, v);
                        paramNames.Add($"@{paramName}");
                    }

                    valuesList.Add($"({string.Join(",", paramNames)})");
                    rowIndex++;
                }

                var sql =
                    $"INSERT INTO `{tableName}` ({colList}) VALUES {string.Join(",", valuesList)};";

                try
                {
                    await _connection.ExecuteAsync(
                        new CommandDefinition(sql, parameters, tx, cancellationToken: ct));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Bad chunk detected. Dumping rows:");

                    foreach (var row in chunk)
                    {
                        foreach (var kv in row.Values)
                            Console.WriteLine($"   {kv.Key} = {kv.Value}");
                        Console.WriteLine("---");
                    }

                    Console.WriteLine($"   Error: {ex.Message}");
                    throw;
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