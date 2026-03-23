using DbSyncEngine.Application.Pipelines.Mappers.Abstractions;
using Orders.Core.Models.Entites;

namespace DbSyncEngine.Application.Pipelines.Mappers.Implements;

public class OrderToOrderPgMapper : IEntityMapper<Order, Order>
{
    public Order Map(Order source)
    {
        if (source == null) return null;

        return source;
    }
}