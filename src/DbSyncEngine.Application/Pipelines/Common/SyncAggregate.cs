using DbSyncEngine.Application.Pipelines.Comparison;
using Orders.Core.Models.Entites;
using Buffer = Orders.Core.Models.Entites.Buffer;

namespace DbSyncEngine.Application.Pipelines.Common;

public class SyncAggregate
{
    public Order Order { get; set; }
    public List<Buffer> Buffer { get; set; }
    public List<Group> Group { get; set; }
    public List<Product> Products { get; set; }
    public List<OrderService> Services { get; set; }
    public SyncOperation Operation { get; set; }
    public List<(SyncOperation Operation, Product Product)> ProductOperations { get; set; } = new();
    public List<(SyncOperation Operation, OrderService Service)> ServiceOperations { get; set; } = new();
    public List<(SyncOperation Operation, Buffer Buffer)> BufferOperations { get; set; } = new();
    public List<(SyncOperation Operation, Group Group)> GroupOperations { get; set; } = new();

    public void ApplyDiff(AggregateDiff diff)
    {
        // Order
        this.Operation = diff.OrderOperation;

        // Products
        foreach (var (op, item) in diff.ProductOps)
            this.ProductOperations.Add((op, item));

        // Services
        foreach (var (op, item) in diff.ServiceOps)
            this.ServiceOperations.Add((op, item));

        // Buffer
        foreach (var (op, item) in diff.BufferOps)
        {
            this.BufferOperations.Add((op, item));
        }

        // Group
        foreach (var (op, item) in diff.GroupOps)
        {
            this.GroupOperations.Add((op, item));
        }
    }
}