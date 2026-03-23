using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DbSyncEngine.Application.Helper;

namespace DbSyncEngine.Application.Entities;

[Table("reload_state")]
public class ReloadState : Base
{
    [Key]
    [Column("id")]
    public override long Id { get; set; }
 
    [Column("sync_direction")]
    public string SyncDirectionRaw { get; private set; } = default!;
    
    [NotMapped]
    public SyncDirection SyncDirection
    {
        get => Enum.Parse<SyncDirection>(SyncDirectionRaw, true);
        set => SyncDirectionRaw = value.ToString();
    }

    [Column("last_processed_key")]
    // Универсальный ключ прогресса (Id, DateUpdate, Guid, Timestamp)
    public string LastProcessedKey { get; set; }

    // Флаг, что полная выгрузка завершена
    [Column("is_completed")]
    public bool IsCompleted { get; set; }

    // Флаг, что пользователь запросил перезапуск
    [Column("restart_requested")]
    public bool RestartRequested { get; set; }

    // Время последнего обновления состояния
    [Column("last_updated_utc")]
    public DateTime LastUpdatedUtc { get; set; }

    // Количество успешно обработанных строк
    [Column("total_processed_rows")]
    public long TotalProcessedRows { get; set; }

    // Количество ошибок записи (для статистики)
    [Column("total_write_errors")]
    public int TotalWriteErrors { get; set; }

    // Количество попыток перезапуска (для мониторинга)
    [Column("restart_count")]
    public int RestartCount { get; set; }

    public static ReloadState CreateNew(SyncDirection direction)
    {
        return new ReloadState
        {
            SyncDirection = direction,
            IsCompleted = false,
            RestartRequested = false,
            LastUpdatedUtc = DateTime.UtcNow,
            TotalProcessedRows = 0,
            TotalWriteErrors = 0,
            RestartCount = 0
        };
    }
    
    // Удобный метод для сброса состояния
    public void Reset()
    {
        LastProcessedKey = null;
        IsCompleted = false;
        RestartRequested = false;
        TotalProcessedRows = 0;
        TotalWriteErrors = 0;
        LastUpdatedUtc = DateTime.UtcNow;
        RestartCount++;
    }
}
