using System.Data;
using System.Text.RegularExpressions;
using Dapper;
using DbSyncEngine.Application.Persistence;
using DbSyncEngine.Application.Pipelines.Common;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace DbSyncEngine.Infrastructure.Persistence.Repositories;

public class MySqlTableDataRepository : TableDataRepositoryBase, ITableDataRepository
{
    private readonly MySqlConnection _connection;
    private readonly ILogger<MySqlTableDataRepository> _logger;

    protected static readonly Regex SafeIdentifier =
        new("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

    protected static string QuoteMySqlIdentifier(string id)
    {
        if (!SafeIdentifier.IsMatch(id))
            throw new ArgumentException($"Unsafe identifier: {id}");
        return $"`{id}`";
    }

    public MySqlTableDataRepository(MySqlConnection connection, ILogger<MySqlTableDataRepository> logger)
    {
        _connection = connection;
        _logger = logger;
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
            var colList = string.Join(",", columns.Select(QuoteMySqlIdentifier));

            foreach (var chunk in rows.Chunk(chunkSize))
            {
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

                var sql = $"INSERT INTO `{tableName}` ({colList}) VALUES {string.Join(",", valuesList)};";

                try
                {
                    await _connection.ExecuteAsync(
                        new CommandDefinition(sql, parameters, tx, cancellationToken: ct));
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    var rowDumps = chunk
                        .Select(row => string.Join(", ", row.Values.Select(kv => $"{kv.Key}={kv.Value}")));
                    _logger.LogError(ex,
                        "Bad chunk detected in table {Table}. Rows: [{Rows}]",
                        tableName, string.Join(" | ", rowDumps));
                    throw;
                }
            }

            await tx.CommitAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await tx.RollbackAsync(ct);
            _logger.LogError(ex, "Insert batch failed, rolling back transaction for table {Table}", tableName);
            throw;
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }
}
