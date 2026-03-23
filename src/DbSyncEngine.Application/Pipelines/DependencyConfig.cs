using Orders.Core.Models.Entites;
using Relef.Config.Model;

namespace DbSyncEngine.Application.Pipelines;

public class DependencyConfig
{
    public static readonly Dictionary<Type, IReadOnlyList<Type>> Dependencies = new()
    {
        { typeof(Order), new[] { typeof(Product) } },
        { typeof(Order), new[] { typeof(Group) } },
        { typeof(Product), new[] { typeof(Service) } }
    };
}