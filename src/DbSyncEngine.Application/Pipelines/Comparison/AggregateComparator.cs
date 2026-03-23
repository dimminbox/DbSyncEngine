using DbSyncEngine.Application.Pipelines.Common;
using Orders.Core.Models.Entites;
using Relef.Config.Model;
using Buffer = Orders.Core.Models.Entites.Buffer;

namespace DbSyncEngine.Application.Pipelines.Comparison;

public class AggregateComparator : IAggregateComparator
{
    public AggregateDiff Compare(SyncAggregate mysql, SyncAggregate postgres)
    {
        var diff = new AggregateDiff();
        diff.OrderOperation = mysql.Operation;

        CompareProducts(mysql, postgres, diff);
        CompareServices(mysql, postgres, diff);
        CompareBuffer(mysql, postgres, diff);
        CompareGroup(mysql, postgres, diff);

        return diff;
    }

    private void CompareProducts(SyncAggregate mysql, SyncAggregate pg, AggregateDiff diff)
    {
        var mysqlDict = mysql.Products
            .GroupBy(p => p.ProductGuid)
            .ToDictionary(p => p.Key, p => p.ToList());

        var pgDict = pg?.Products
            .GroupBy(p => p.ProductGuid)
            .ToDictionary(p => p.Key, p => p.ToList());

        foreach (var (guid, mysqlProducts) in mysqlDict)
        {
            List<Product>? pgProducts = null;
            if (pgDict?.Count == 0 || pgDict?.TryGetValue(guid, out pgProducts) != true)
            {
                foreach (Product mysqlProduct in mysqlProducts)
                {
                    diff.ProductOps.Add((SyncOperation.Create, mysqlProduct));
                }

                continue;
            }

            if (mysqlProducts.Count > 1 || pgProducts?.Count > 1)
            {
                foreach (var pgProduct in pgProducts ?? new List<Product>())
                {
                    diff.ProductOps.Add((SyncOperation.Delete, pgProduct));
                }

                foreach (var mysqlProduct in mysqlProducts)
                {
                    diff.ProductOps.Add((SyncOperation.Create, mysqlProduct));
                }
            }
            else
            {
                var mysqlProduct = mysqlProducts.FirstOrDefault();
                var pgProduct = pgProducts.FirstOrDefault();
                if (!AreProductsEqual(mysqlProduct, pgProduct))
                {
                    mysqlProduct.Id = pgProduct.Id;
                    diff.ProductOps.Add((SyncOperation.Update, mysqlProduct));
                }
            }
        }

        foreach (var (guid, pgProducts) in pgDict ?? new Dictionary<Guid, List<Product>>())
        {
            if (!mysqlDict.ContainsKey(guid))
            {
                foreach (var pgProduct in pgProducts)
                {
                    diff.ProductOps.Add((SyncOperation.Delete, pgProduct));
                }
            }
        }
    }

    private void CompareServices(SyncAggregate mysql, SyncAggregate pg, AggregateDiff diff)
    {
        var mysqlDict = mysql.Services.ToDictionary(s => s.ServiceGuid);

        var pgDict = pg?.Services
            .GroupBy(p => p.ServiceGuid)
            .ToDictionary(p => p.Key, p => p.ToList());


        foreach (var (guid, mysqlService) in mysqlDict)
        {
            List<OrderService> pgServices = null;
            if (pgDict?.Count == 0 || !pgDict?.TryGetValue(guid, out pgServices) != true)
            {
                diff.ServiceOps.Add((SyncOperation.Create, mysqlService));
                continue;
            }

            if (pgServices.Count > 1)
            {
                foreach (var pgService in pgServices)
                {
                    diff.ServiceOps.Add((SyncOperation.Create, pgService));
                }

                diff.ServiceOps.Add((SyncOperation.Create, mysqlService));
            }
            else
            {
                var pgService = pgServices.FirstOrDefault();
                if (!AreServicesEqual(mysqlService, pgService))
                {
                    mysqlService.Id = pgService.Id;
                    diff.ServiceOps.Add((SyncOperation.Update, mysqlService));
                }
            }
        }

        foreach (var (guid, pgServices) in pgDict ?? new Dictionary<Guid, List<OrderService>>())
        {
            if (!mysqlDict.ContainsKey(guid))
            {
                foreach (var pgService in pgServices)
                {
                    diff.ServiceOps.Add((SyncOperation.Delete, pgService));
                }
            }
        }
    }

    private void CompareBuffer(SyncAggregate mysql, SyncAggregate pg, AggregateDiff diff)
    {
        var mysqlBuf = mysql.Buffer.FirstOrDefault();
        var pgBufs = pg?.Buffer;

        if (pgBufs?.Count > 1)
        {
            foreach (var pgBufItem in pgBufs)
            {
                diff.BufferOps.Add((SyncOperation.Delete, pgBufItem));
            }

            diff.BufferOps.Add((SyncOperation.Create, mysqlBuf));
        }
        else
        {
            var pgBuf = pgBufs?.FirstOrDefault();
            if (mysqlBuf == null && pgBuf == null)
                return;

            if (mysqlBuf == null && pgBuf != null)
            {
                diff.BufferOps.Add((SyncOperation.Delete, pgBuf));
                return;
            }

            if (mysqlBuf != null && pgBuf == null)
            {
                diff.BufferOps.Add((SyncOperation.Create, mysqlBuf));
                return;
            }

            if (!AreBuffersEqual(mysqlBuf!, pgBuf!))
            {
                mysqlBuf.Id = pgBuf.Id;
                diff.BufferOps.Add((SyncOperation.Update, mysqlBuf!));
            }
        }
    }

    private void CompareGroup(SyncAggregate mysql, SyncAggregate pg, AggregateDiff diff)
    {
        var mysqlGroup = mysql.Group.FirstOrDefault();
        var pgGroups = pg?.Group;

        if (pgGroups?.Count > 1)
        {
            foreach (var pgGroupItem in pgGroups)
            {
                diff.GroupOps.Add((SyncOperation.Delete, pgGroupItem));
            }

            diff.GroupOps.Add((SyncOperation.Create, mysqlGroup));
        }
        else
        {
            var pgGroup = pgGroups?.FirstOrDefault();
            if (mysqlGroup == null && pgGroup == null)
                return;

            if (mysqlGroup == null && pgGroup != null)
            {
                diff.GroupOps.Add((SyncOperation.Delete, pgGroup));
                return;
            }

            if (mysqlGroup != null && pgGroup == null)
            {
                diff.GroupOps.Add((SyncOperation.Create, mysqlGroup));
                return;
            }

            if (!AreGroupsEqual(mysqlGroup!, pgGroup!))
            {
                mysqlGroup.Id = pgGroup.Id;
                diff.GroupOps.Add((SyncOperation.Update, mysqlGroup));
            }
        }
    }

    private bool AreProductsEqual(Product? a, Product? b) =>
        a?.Price == b?.Price &&
        a?.Quantity == b?.Quantity &&
        a?.Title == b?.Title &&
        a?.Article == b?.Article &&
        a?.Barcode == b?.Barcode &&
        a?.Vat == b?.Vat;

    private bool AreServicesEqual(OrderService? a, OrderService? b) =>
        a?.Price == b?.Price && a?.Quantity == b?.Quantity;

    private bool AreBuffersEqual(Buffer? a, Buffer? b) =>
        a?.Contractor == b?.Contractor &&
        a?.DateEnd == b?.DateEnd &&
        a?.DateStart == b?.DateStart &&
        a?.MinAmount == b?.MinAmount &&
        a?.MergeAmount == b?.MergeAmount &&
        a?.CostAdd == b?.CostAdd &&
        a?.IsMerge == b?.IsMerge &&
        a?.Amount == b?.Amount &&
        a?.Store == b?.Store &&
        a?.DeliveryAddress == b?.DeliveryAddress;

    private bool AreGroupsEqual(Group a, Group b) => a?.GroupGuid == b?.GroupGuid;
}