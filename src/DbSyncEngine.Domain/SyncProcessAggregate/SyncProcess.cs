using DbSyncEngine.Domain.SyncProcessAggregate.Enums;

namespace DbSyncEngine.Domain.SyncProcessAggregate;

public class SyncProcess
{
    public long Id { get; private set; }
    public string EntityName { get; private set; }
    public string SourceProvider { get; private set; }
    public string TargetProvider { get; private set; }

    public SyncDirection Direction { get; private set; }
    public string DirectionString => Direction.ToString();

    public string LastProcessedKey { get; private set; }

    public bool IsCompleted { get; private set; }

    public bool RestartRequested { get; private set; }

    public DateTime LastUpdatedUtc { get; private set; }

    public long TotalProcessedRows { get; private set; }

    public int TotalWriteErrors { get; private set; }

    public int RestartCount { get; private set; }

    private SyncProcess()
    {
    }

    private SyncProcess(
        string entityName,
        string sourceProvider,
        string targetProvider,
        SyncDirection direction)
    {
        EntityName = entityName;
        SourceProvider = sourceProvider;
        TargetProvider = targetProvider;
        Direction = direction;
        ResetInternal();
    }

    public static SyncProcess CreateNew(
        string entityName,
        string sourceProvider,
        string targetProvider,
        SyncDirection direction)
        => new SyncProcess(entityName, sourceProvider, targetProvider, direction);

    public void MarkProcessed(string key, long rows)
    {
        LastProcessedKey = key;
        TotalProcessedRows += rows;
        LastUpdatedUtc = DateTime.UtcNow;
    }

    public void MarkWriteError()
    {
        TotalWriteErrors++;
        LastUpdatedUtc = DateTime.UtcNow;
    }

    public void MarkCompleted()
    {
        IsCompleted = true;
        LastUpdatedUtc = DateTime.UtcNow;
    }

    public void SetId(long id)
    {
        Id = id;
    }

    public void RequestRestart()
    {
        RestartRequested = true;
        LastUpdatedUtc = DateTime.UtcNow;
    }

    public void Reset()
    {
        RestartCount++;
        ResetInternal();
    }

    private void ResetInternal()
    {
        LastProcessedKey = null;
        IsCompleted = false;
        RestartRequested = false;
        TotalProcessedRows = 0;
        TotalWriteErrors = 0;
        LastUpdatedUtc = DateTime.UtcNow;
    }

    public void UpdateProgress(object newKey)
    {
        if (newKey is null)
            throw new ArgumentNullException(nameof(newKey));
        LastProcessedKey = newKey.ToString();
        TotalProcessedRows++;
        LastUpdatedUtc = DateTime.UtcNow;
    }
}