using DbSyncEngine.Application.Persistence.Schema;
using Microsoft.Extensions.DependencyInjection;

namespace DbSyncEngine.Infrastructure.Persistence.Schema.Normalization;

public class SchemaNormalizerFactory : ISchemaNormalizerFactory
{
    private readonly IServiceProvider _provider;
    private readonly IDictionary<string, Type> _map;

    public SchemaNormalizerFactory(IServiceProvider provider)
    {
        _provider = provider;
        _map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["MySQL"] = typeof(MySqlSchemaNormalizer),
            ["PostgreSQL"] = typeof(PostgresSchemaNormalizer),
            // добавить другие провайдеры по мере необходимости
        };
    }

    public ISchemaNormalizer Create(NormalizerContext ctx)
    {
        if (ctx == null) throw new ArgumentNullException(nameof(ctx));

        if (!_map.TryGetValue(ctx.TargetProvider, out var type))
        {
            // fallback на универсальный нормализатор или бросаем
            throw new InvalidOperationException($"No schema provider found for {ctx.TargetProvider}");
        }

        return (ISchemaNormalizer)_provider.GetRequiredService(type);
    }
}