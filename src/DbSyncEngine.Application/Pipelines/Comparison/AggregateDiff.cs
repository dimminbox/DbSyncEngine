using DbSyncEngine.Application.Pipelines.Common;
using Orders.Core.Models.Entites;
using Buffer = Orders.Core.Models.Entites.Buffer;

namespace DbSyncEngine.Application.Pipelines.Comparison;

public class AggregateDiff
{
    public SyncOperation OrderOperation { get; set; }

    public List<(SyncOperation op, Product item)> ProductOps { get; set; } = new();
    public List<(SyncOperation op, OrderService item)> ServiceOps { get; set; } = new();

    public List<(SyncOperation op, Buffer? item)> BufferOps { get; set; } = new();
    public List<(SyncOperation op, Group? item)> GroupOps { get; set; } = new();
}