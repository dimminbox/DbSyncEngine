using DbSyncEngine.Application.Providers.Abstractions;

namespace DbSyncEngine.Application.Providers.Implementations;

public class PostgresProvider : IPostgresProvider
{
    public IServiceProvider Provider { get; }
    public PostgresProvider(IServiceProvider provider) => Provider = provider;
}