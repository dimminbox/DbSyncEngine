using System.Linq.Expressions;
using DbSyncEngine.Application.Pipelines.Common;
using DbSyncEngine.Application.Pipelines.Steps.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orders.Core.Models.Entites;
using Relef.Repository.Helpers;
using Relef.Repository.Interfaces;
using Relef.Repository.Models;
using Buffer = Orders.Core.Models.Entites.Buffer;

namespace DbSyncEngine.Application.Pipelines.Steps.Common;

public class EnrichLoaderStep
{
    protected readonly IServiceProvider _provider;
    private readonly ILogger<EnrichLoaderStep> _logger;
    private readonly ReloadEntityConfig<Order> _config;
    private readonly string _source;

    protected EnrichLoaderStep(IServiceProvider provider, ILogger<EnrichLoaderStep> logger,
        ReloadEntityConfig<Order> config, string source)
    {
        _provider = provider;
        _logger = logger;
        _config = config;
        _source = source;
    }

    public async Task<List<SyncAggregate>> LoadAggregatesAsync(List<Guid> orderGuids)
    {
        // Репозитории
        var orderRepo = _provider.GetRequiredService<IRepository<Order>>();
        var productRepo = _provider.GetRequiredService<IRepository<Product>>();
        var serviceRepo = _provider.GetRequiredService<IRepository<OrderService>>();
        var bufferRepo = _provider.GetRequiredService<IRepository<Buffer>>();
        var groupRepo = _provider.GetRequiredService<IRepository<Group>>();

        _logger.LogInformation($"Loading Orders from {_source}");
        var orders = await LoadInChunks(orderRepo, orderGuids, o => o.OrderGuid, _config.KeySelector);
        
        _logger.LogInformation($"Loading Products from {_source}");
        var products = await LoadInChunks(productRepo, orderGuids, p => p.OrderGuid);

        _logger.LogInformation($"Loading Services from {_source}");
        var services = await LoadInChunks(serviceRepo, orderGuids, s => s.OrderGuid);

        // Собираем GUID буферов и групп
        var bufferGuids = orders
            .Select(o => o.Buffer)
            .Where(g => g.HasValue)
            .Select(g => g.Value)
            .Distinct()
            .ToList();

        var groupGuids = orders
            .Select(o => o.GroupGuid)
            .Where(g => g.HasValue)
            .Select(g => g.Value)
            .Distinct()
            .ToList();

        _logger.LogInformation($"Loading Buffers from {_source}");
        var buffers = await LoadInChunks(bufferRepo, bufferGuids, b => b.BufferGuid);

        _logger.LogInformation($"Loading Groups from {_source}");
        var groups = await LoadInChunks(groupRepo, groupGuids, g => g.GroupGuid);

        // Собираем агрегаты
        var result = new List<SyncAggregate>();

        foreach (var order in orders)
        {
            result.Add(new SyncAggregate
            {
                Order = order,
                Products = products.Where(p => p.OrderGuid == order.OrderGuid).ToList(),
                Services = services.Where(s => s.OrderGuid == order.OrderGuid).ToList(),
                Buffer = buffers,
                Group = groups
            });
        }

        return result;
    }

    protected async Task<List<T>> LoadInChunks<T>(
        IRepository<T> repo,
        List<Guid> keys,
        Expression<Func<T, Guid>> keySelector,
        Expression<Func<T, object>>? sortSelector = null,
        int chunkSize = 300) where T : Base
    {
        var result = new List<T>(capacity: keys.Count * 5);

        var originalParam = keySelector.Parameters[0];
        var originalBody = (MemberExpression)keySelector.Body;

        var sort = sortSelector != null
            ? new List<Sort<T>> { new() { Expression = sortSelector, Direction = Sort<T>.Directions.ASC } }
            : null;

        for (int i = 0; i < keys.Count; i += chunkSize)
        {
            var chunk = keys.Skip(i).Take(chunkSize).ToList();
            var holder = new Holder<Guid> { Values = chunk };
            var holderExpr = Expression.Constant(holder);
            var valuesProp = typeof(Holder<Guid>).GetProperty(nameof(Holder<Guid>.Values));
            var valuesMember = Expression.Property(holderExpr, valuesProp);
            var param = Expression.Parameter(typeof(T), "x");

            var bodyMember = (MemberExpression)new ReplaceParameterVisitor(originalParam, param)
                .Visit(originalBody);

            var containsCall =
                Expression.Call(
                    typeof(Enumerable),
                    nameof(Enumerable.Contains),
                    new[] { typeof(Guid) },
                    valuesMember,
                    bodyMember);

            var lambda = Expression.Lambda<Func<T, bool>>(containsCall, param);

            var batch = await repo.ListAsync(
                new[] { new List<Expression<Func<T, bool>>> { lambda } },
                FilterLogic.AND,
                sort);

            batch.ThrowIfFailed();

            if (batch.IsSuccess)
                result.AddRange(batch.Data);
        }

        return result;
    }

    class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _from;
        private readonly ParameterExpression _to;

        public ReplaceParameterVisitor(ParameterExpression from, ParameterExpression to)
        {
            _from = from;
            _to = to;
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => node == _from ? _to : node;
    }

    class Holder<TValue>
    {
        public List<TValue> Values { get; set; }
    }
}