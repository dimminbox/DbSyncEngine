namespace DbSyncEngine.Application.Helper;

public class SyncOptions
{
    public SyncDirection Direction { get; set; }
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(5);
    public int MaxInsertRetries { get; set; } = 3;
    public int ChunkSizeToRead { get; set; } = 10;
    public int InsertBatchSize { get; set; } = 10;
}