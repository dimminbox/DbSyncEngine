using DbSyncEngine.Application.Providers.Abstractions;

namespace DbSyncEngine.Application.Providers.Implementations;

public class MySqlProvider : IMySqlProvider
{
    public IServiceProvider Provider { get; }
    public MySqlProvider(IServiceProvider provider) => Provider = provider;
}