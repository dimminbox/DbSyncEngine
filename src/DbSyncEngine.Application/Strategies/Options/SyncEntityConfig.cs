namespace DbSyncEngine.Application.Strategies.Options;

public class SyncEntityConfig
{
    public string Name { get; set; } = default!;

    public DbEndpoint Source { get; set; } = default!;
    public DbEndpoint Target { get; set; } = default!;

    public List<string> Columns { get; set; } = new();
    public string Key { get; set; } = default!;

    public int ChunkSize { get; set; } = 10000;
    public int InsertBatchSize { get; set; } = 5000;
    public int MaxInsertRetries { get; set; } = 3;
    public int IntervalSeconds { get; set; } = 60;
}