using DbSyncEngine.Application.Pipelines.Common;

namespace DbSyncEngine.Application.Persistence.Schema;

public interface ISchemaReader
{
    Task<TableDefinition> ReadAsync(SyncContext ctx);
}