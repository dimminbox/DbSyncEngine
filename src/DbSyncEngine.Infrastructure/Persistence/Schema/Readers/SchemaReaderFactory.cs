using DbSyncEngine.Application.Persistence;
using DbSyncEngine.Application.Persistence.Schema;
using Microsoft.Extensions.DependencyInjection;

namespace DbSyncEngine.Infrastructure.Persistence.Schema.Readers;

public class SchemaReaderFactory : ISchemaReaderFactory
{
    private readonly IServiceProvider _provider;
    private readonly Dictionary<string, Type> _map;
    
    
    public SchemaReaderFactory(IServiceProvider provider)
    {
        _provider = provider;

        _map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["MySQL"]    = typeof(MySqlSchemaReader),
            ["PostgreSQL"] = typeof(PostgresSchemaReader),
        };
    }
    
    public ISchemaReader Create(string provider)
    {
        if (!_map.TryGetValue(provider, out var type))
            throw new NotSupportedException($"Unsupported schema reader provider: {provider}");

        return (ISchemaReader)_provider.GetRequiredService(type);
    }
}