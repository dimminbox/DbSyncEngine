using System.Data;
using Dapper;

namespace DbSyncEngine.Infrastructure.Persistence.Schema.SyncProcess;

public class SyncProcessSchemaInitializer
{
    private const string CreateTableSql = @"
        CREATE TABLE IF NOT EXISTS sync_process (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            entity_name TEXT NOT NULL,
            source_provider TEXT NOT NULL,
            target_provider TEXT NOT NULL,
            direction TEXT NOT NULL,
            last_processed_key TEXT NULL,
            is_completed INTEGER NOT NULL DEFAULT 0,
            restart_requested INTEGER NOT NULL DEFAULT 0,
            last_updated_utc TEXT NOT NULL DEFAULT (datetime('now')),
            total_processed_rows INTEGER NOT NULL DEFAULT 0,
            total_write_errors INTEGER NOT NULL DEFAULT 0,
            restart_count INTEGER NOT NULL DEFAULT 0
        );

        CREATE UNIQUE INDEX IF NOT EXISTS ux_sync_process_entity
            ON sync_process(entity_name, source_provider, target_provider, direction);
    ";

    public static void EnsureCreated(IDbConnection connection)
    {
        connection.Execute(CreateTableSql);
    }
}