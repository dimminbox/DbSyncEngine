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
            [DbProviders.MySql]      = typeof(MySqlSchemaNormalizer),
            [DbProviders.PostgreSql] = typeof(PostgresSchemaNormalizer),
        };
    }

    public ISchemaNormalizer Create(NormalizerContext ctx)
    {
        if (ctx == null) throw new ArgumentNullException(nameof(ctx));

        if (!_map.TryGetValue(ctx.TargetProvider, out var type))
            throw new InvalidOperationException($"No schema normalizer found for provider '{ctx.TargetProvider}'");

        return (ISchemaNormalizer)_provider.GetRequiredService(type);
    }
}
