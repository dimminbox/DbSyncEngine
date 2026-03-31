using Dapper;
using DbSyncEngine.Application.Persistence.Schema;
using DbSyncEngine.Application.Pipelines.Common;
using DbSyncEngine.Application.Strategies.Options;
using DbSyncEngine.Infrastructure.Persistence.Abstractions;
using DbSyncEngine.Infrastructure.Persistence.Schema.Ddl;
using Microsoft.Extensions.Logging;

namespace DbSyncEngine.Infrastructure.Persistence.Schema;

public class SchemaBootstrapper : ISchemaBootstrapper
{
    private readonly ISchemaReaderFactory _readerFactory;
    private readonly ITargetDdlGeneratorFactory _ddlFactory;
    private readonly IDbConnectionFactory _connections;
    private readonly ILogger<SchemaBootstrapper> _logger;

    public SchemaBootstrapper(
        ISchemaReaderFactory readerFactory,
        ITargetDdlGeneratorFactory ddlFactory,
        IDbConnectionFactory connections,
        ILogger<SchemaBootstrapper> logger)
    {
        _readerFactory = readerFactory;
        _ddlFactory = ddlFactory;
        _connections = connections;
        _logger = logger;
    }

    public async Task<TableDefinition> ReadSourceSchemaAsync(SyncContext ctx, CancellationToken ct)
    {
        var reader = _readerFactory.Create(ctx.Config.Source.Provider);
        var schema = await reader.ReadAsync(ctx);
        return schema;
    }

    public async Task<TableDefinition> ReadTargetSchemaAsync(SyncContext ctx, CancellationToken ct)
    {
        var reader = _readerFactory.Create(ctx.Config.Target.Provider);
        var schema = await reader.ReadAsync(ctx);
        return schema;
    }

    /// <summary>
    /// Apply normalized schema: create missing tables and replace existing ones according to options.
    /// </summary>
    public async Task ApplyNormalizedTableAsync(SyncContext ctx, TableDefinition normalizedTable,
        CancellationToken ct)
    {
        if (normalizedTable == null) throw new ArgumentNullException(nameof(normalizedTable));

        var provider = ctx.Config.Target.Provider;
        var ddl = _ddlFactory.Create(provider);

        // Проверяем наличие таблицы на целевом хосте
        var exists = await TableExistsAsync(ctx, normalizedTable.Name, ct);

        if (!exists)
        {
            _logger.LogInformation("Creating missing table {Table}", normalizedTable.Name);
            var createSql = ddl.GenerateCreateTable(normalizedTable, ctx.Config.Target.Schema);
            _logger.LogDebug("Create SQL for {Table}: {Sql}", normalizedTable.Name, createSql);
            using (var conn = _connections.Create(provider, ctx.Config.Target.ConnectionString))
            {
                await conn.ExecuteAsync(new CommandDefinition(createSql, cancellationToken: ct,
                    commandTimeout: ddl.CommandTimeout));
            }

            return;
        }

        // Стратегия замены/пропуска — берём из конфигурации
        var normalizerOptions = ctx.Config.NormalizerOptions ?? new NormalizerOptions();
        var replace = normalizerOptions.CopyDataOnReplace;

        if (replace)
        {
            _logger.LogInformation("Replacing existing table {Table}", normalizedTable.Name);
            await ReplaceTableAsync(ctx, normalizedTable, ct);
        }
        else
        {
            _logger.LogInformation("Skipping existing table {Table}", normalizedTable.Name);
        }
    }

    public async Task<bool> TableExistsAsync(SyncContext ctx, string tableName, CancellationToken ct)
    {
        var targetProvider = ctx.Config.Target.Provider;
        using var conn = _connections.Create(targetProvider, ctx.Config.Target.ConnectionString);
        var ddl = _ddlFactory.Create(targetProvider);

        var existsSql = ddl.GenerateTableExistsSql(tableName, ctx.Config.Target.Schema);
        var exists = await conn.ExecuteScalarAsync<int>(existsSql);
        return exists > 0;
    }

    public async Task CreateTableAsync(SyncContext ctx, TableDefinition table, CancellationToken ct)
    {
        var targetProvider = ctx.Config.Target.Provider;
        using var conn = _connections.Create(targetProvider, ctx.Config.Target.ConnectionString);
        var ddl = _ddlFactory.Create(targetProvider);

        var createSql = ddl.GenerateCreateTable(table, ctx.Config.Target.Schema);
        _logger.LogDebug("Create SQL for {Table}: {Sql}", table.Name, createSql);

        await conn.ExecuteAsync(createSql);
    }

    public async Task DropTableAsync(SyncContext ctx, string tableName, CancellationToken ct)
    {
        var targetProvider = ctx.Config.Target.Provider;
        using var conn = _connections.Create(targetProvider, ctx.Config.Target.ConnectionString);
        var ddl = _ddlFactory.Create(targetProvider);

        var dropSql = ddl.GenerateDropTable(tableName, ctx.Config.Target.Schema);
        _logger.LogDebug("Drop SQL for {Table}: {Sql}", tableName, dropSql);

        await conn.ExecuteAsync(dropSql);
    }

    public async Task ReplaceTableAsync(SyncContext ctx, TableDefinition table, CancellationToken ct)
    {
        var targetProvider = ctx.Config.Target.Provider;
        using var conn = _connections.Create(targetProvider, ctx.Config.Target.ConnectionString);
        var ddl = _ddlFactory.Create(targetProvider);

        // Strategy:
        // 1. Create temp table with new definition
        // 2. Optionally copy data (if desired) - here we do not copy by default, but generator can implement copy
        // 3. Swap/rename temp -> real (provider specific)
        // 4. Drop old if needed
        // All steps are executed sequentially; provider may wrap in transaction if supported.

        var tempName = ddl.GenerateTempTableName(table.Name);

        var createTempSql = ddl.GenerateCreateTableWithName(table, tempName, ctx.Config.Target.Schema);
        _logger.LogDebug("Create temp SQL for {Temp}: {Sql}", tempName, createTempSql);
        await conn.ExecuteAsync(createTempSql);

        // Optional data copy: generator may return SQL or null
        var copySql = ddl.GenerateCopyDataSql(table.Name, tempName, ctx.Config.Target.Schema);
        if (!string.IsNullOrWhiteSpace(copySql))
        {
            _logger.LogDebug("Copy data SQL from {Old} to {Temp}: {Sql}", table.Name, tempName, copySql);
            await conn.ExecuteAsync(copySql);
        }

        // Swap/rename
        var swapSql = ddl.GenerateSwapTableSql(table.Name, tempName, ctx.Config.Target.Schema);
        _logger.LogDebug("Swap SQL for {Old} <-> {Temp}: {Sql}", table.Name, tempName, swapSql);
        await conn.ExecuteAsync(swapSql);

        // Cleanup old (if generator requires explicit drop)
        var cleanupSql = ddl.GenerateCleanupAfterSwapSql(table.Name, tempName, ctx.Config.Target.Schema);
        if (!string.IsNullOrWhiteSpace(cleanupSql))
        {
            _logger.LogDebug("Cleanup SQL after swap: {Sql}", cleanupSql);
            await conn.ExecuteAsync(cleanupSql);
        }
    }
}