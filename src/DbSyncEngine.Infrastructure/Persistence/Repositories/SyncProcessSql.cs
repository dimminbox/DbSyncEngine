namespace DbSyncEngine.Infrastructure.Persistence.Repositories;

public static class SyncProcessSql
{
    public const string GetById = @"
        SELECT 
            id,
            last_processed_key,
            is_completed,
            restart_requested,
            last_updated_utc,
            total_processed_rows,
            total_write_errors,
            restart_count,
            sync_direction
        FROM orders.sync_process
        WHERE id = @id;
    ";

    public const string GetByDirection = @"
        SELECT 
            id,
            last_processed_key,
            is_completed,
            restart_requested,
            last_updated_utc,
            total_processed_rows,
            total_write_errors,
            restart_count,
            sync_direction
        FROM orders.sync_process
        WHERE sync_direction = @direction
        LIMIT 1;
    ";

    public const string Insert = @"
        INSERT INTO orders.sync_process (
            last_processed_key,
            is_completed,
            restart_requested,
            last_updated_utc,
            total_processed_rows,
            total_write_errors,
            restart_count,
            sync_direction
        )
        VALUES (
            @LastProcessedKey,
            @IsCompleted,
            @RestartRequested,
            @LastUpdatedUtc,
            @TotalProcessedRows,
            @TotalWriteErrors,
            @RestartCount,
            @Direction
        )
        RETURNING id;
    ";

    public const string Update = @"
        UPDATE orders.sync_process
        SET
            last_processed_key = @LastProcessedKey,
            is_completed = @IsCompleted,
            restart_requested = @RestartRequested,
            last_updated_utc = @LastUpdatedUtc,
            total_processed_rows = @TotalProcessedRows,
            total_write_errors = @TotalWriteErrors,
            restart_count = @RestartCount,
            sync_direction = @Direction
        WHERE id = @Id;
    ";
}