using DbSyncEngine.Application.Entities;
using DbSyncEngine.Application.Strategies.Options;
using DbSyncEngine.Application.Helper;
using DbSyncEngine.Domain.SyncProcessAggregate;
using DbSyncEngine.Domain.SyncProcessAggregate.Enums;

namespace DbSyncEngine.Application.Pipelines.Common;

public class SyncContext
{
    public SyncDirection Direction { get; init; }
    public DateTimeOffset Now { get; init; } = DateTimeOffset.UtcNow;
    public SyncProcess Process { get; set; }
    public IReadOnlyList<RowData> CurrentBatch { get; set; } = Array.Empty<RowData>();
    public FullReloadOptions Options { get; }
    public CancellationToken CancellationToken { get; set; }

    public SyncContext(FullReloadOptions options)
    {
        Options = options;
    }
}