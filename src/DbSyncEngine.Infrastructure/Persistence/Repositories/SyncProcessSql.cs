namespace DbSyncEngine.Infrastructure.Persistence.Repositories;

public static class SyncProcessSql
{
    public const string GetByCompositeKey = @"
        SELECT 
            id AS Id,
            entity_name AS EntityName,
            source_provider AS SourceProvider,
            target_provider AS TargetProvider,
            direction AS DirectionString,
            last_processed_key AS LastProcessedKey,
            last_processed_key_type AS LastProcessedKeyType,
            is_completed AS IsCompleted,
            restart_requested AS RestartRequested,
            last_updated_utc AS LastUpdatedUtc,
            total_processed_rows AS TotalProcessedRows,
            total_write_errors AS TotalWriteErrors,
            restart_count AS RestartCount
        FROM sync_process
        WHERE entity_name = @EntityName
          AND source_provider = @SourceProvider
          AND target_provider = @TargetProvider
          AND direction = @DirectionString
        LIMIT 1;";

    public const string GetById = @"
        SELECT 
            id,
            entity_name,
            source_provider,
            target_provider,
            direction,
            last_processed_key,
            last_processed_key_type,
            is_completed,
            restart_requested,
            last_updated_utc,
            total_processed_rows,
            total_write_errors,
            restart_count
        FROM sync_process
        WHERE id = @Id;
    ";

    public const string Insert = @"
        INSERT INTO sync_process (
            entity_name,
            source_provider,
            target_provider,
            direction,
            last_processed_key,
            last_processed_key_type,
            is_completed,
            restart_requested,
            last_updated_utc,
            total_processed_rows,
            total_write_errors,
            restart_count
        )
        VALUES (
            @EntityName,
            @SourceProvider,
            @TargetProvider,
            @DirectionString,
            @LastProcessedKey,
            @IsCompleted,
            @RestartRequested,
            @LastUpdatedUtc,
            @TotalProcessedRows,
            @TotalWriteErrors,
            @RestartCount
        );

        SELECT last_insert_rowid();
    ";

    public const string Update = @"
        UPDATE sync_process
        SET
            last_processed_key = @LastProcessedKey,
            is_completed = @IsCompleted,
            restart_requested = @RestartRequested,
            last_updated_utc = @LastUpdatedUtc,
            total_processed_rows = @TotalProcessedRows,
            total_write_errors = @TotalWriteErrors,
            restart_count = @RestartCount
        WHERE id = @Id;
    ";
}