using DbSyncEngine.Application.Pipelines.Common;

namespace DbSyncEngine.Application.Persistence.Schema;

public interface ISchemaBootstrapper
{
    Task<TableDefinition> ReadSourceSchemaAsync(SyncContext ctx, CancellationToken ct);
    Task<TableDefinition> ReadTargetSchemaAsync(SyncContext ctx, CancellationToken ct);
    Task ApplyNormalizedTableAsync(SyncContext ctx, TableDefinition normalized, CancellationToken ct);
    Task<bool> TableExistsAsync(SyncContext ctx, string tableName, CancellationToken ct);
    Task CreateTableAsync(SyncContext ctx, TableDefinition table, CancellationToken ct);
    Task DropTableAsync(SyncContext ctx, string tableName, CancellationToken ct);
    Task ReplaceTableAsync(SyncContext ctx, TableDefinition table, CancellationToken ct);
}