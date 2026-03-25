using DbSyncEngine.Application.Strategies.Options;
using DbSyncEngine.Domain.SyncProcessAggregate;
using DbSyncEngine.Domain.SyncProcessAggregate.Enums;

namespace DbSyncEngine.Application.Pipelines.Common;

public class SyncContext
{
    public SyncEntityConfig Config { get; }

    public SyncProcess Process { get; set; }
    public IReadOnlyList<RowData> CurrentBatch { get; set; } = Array.Empty<RowData>();

    public SyncDirection Direction { get; init; }
    public DateTimeOffset Now { get; init; } = DateTimeOffset.UtcNow;
    public CancellationToken CancellationToken { get; set; }

    public SyncContext(SyncEntityConfig config)
    {
        Config = config;
    }
}