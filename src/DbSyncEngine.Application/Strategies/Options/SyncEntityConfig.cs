using DbSyncEngine.Domain.SyncProcessAggregate.Enums;

namespace DbSyncEngine.Application.Strategies.Options;

public class SyncConfig
{
    public List<SyncEntityConfig> Entities { get; set; } = new();
    public string SyncProcessDb { get; set; } = "";
}

public class SyncEntityConfig
{
    public string Name { get; set; } = default!;
    public DbEndpoint Source { get; set; } = default!;
    public DbEndpoint Target { get; set; } = default!;
    public NormalizerOptions NormalizerOptions { get; init; }
    public int ChunkSize { get; set; } = 10;
    public int InsertBatchSize { get; set; } = 2;
    public int MaxInsertRetries { get; set; } = 3;
    public int IntervalSeconds { get; set; } = 10;
    public SyncDirection Direction { get; set; }
}