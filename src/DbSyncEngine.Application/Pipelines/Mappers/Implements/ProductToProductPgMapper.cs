using DbSyncEngine.Application.Pipelines.Mappers.Abstractions;
using Orders.Core.Models.Entites;

namespace DbSyncEngine.Application.Pipelines.Mappers.Implements;

public class ProductToProductPgMapper : IEntityMapper<Product, Product>
{
    public Product Map(Product source)
    {
        if (source == null) return null;

        return new Product()
        {
            Id = source.Id,
            ProductGuid = source.ProductGuid,
            OrderGuid = source.OrderGuid,
            Title = source.Title,
            Code1C = source.Code1C,
            Article = source.Article,
            Barcode = source.Barcode,
            Price = source.Price,
            Quantity = source.Quantity,
            PriceAdded = source.PriceAdded,
            Vat = source.Vat,
            Weight = source.Weight,
            Volume = source.Volume,
            CostPrice = source.CostPrice,
            Multiplicity = source.Multiplicity,
            Data = source.Data
        };
    }
}