using DbSyncEngine.Application.Normalization;
using Microsoft.Extensions.DependencyInjection;

namespace DbSyncEngine.Infrastructure.Persistence.Normalization.Implementation;

public class ValueNormalizerFactory : IValueNormalizerFactory
{
    private readonly IServiceProvider _provider;
    private readonly Dictionary<string, Type> _map;
    
    public ValueNormalizerFactory(IServiceProvider provider)
    {
        _provider = provider;

        _map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["MySQL"]    = typeof(MySqlValueNormalizer),
            ["Postgres"] = typeof(PostgresValueNormalizer),
        };
    }
    
    public IValueNormalizer Create(string provider)
    {
        if (!_map.TryGetValue(provider, out var normalizerType))
            throw new NotSupportedException($"Unsupported provider: {provider}");

        return (IValueNormalizer)_provider.GetRequiredService(normalizerType);
    }
}