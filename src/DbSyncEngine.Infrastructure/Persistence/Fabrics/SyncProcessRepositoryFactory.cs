using System.Reflection;
using DbSyncEngine.Application.Persistence;
using DbSyncEngine.Application.Strategies.Options;
using DbSyncEngine.Infrastructure.Persistence.Abstractions;
using DbSyncEngine.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DbSyncEngine.Infrastructure.Persistence.Fabrics;

public class SyncProcessRepositoryFactory : ISyncProcessRepositoryFactory
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly string _connectionString;

    public SyncProcessRepositoryFactory(
        IDbConnectionFactory connectionFactory,
        IOptionsMonitor<SyncConfig> config)
    {
        _connectionFactory = connectionFactory;
        _connectionString = config.CurrentValue.SyncProcessDb ??
                            throw new InvalidOperationException("Missing SyncProcessDb connection string");
    }

    public ISyncProcessRepository Create()
    {
        var conn = _connectionFactory.Create("SQLite", _connectionString);
        return new SyncProcessRepository(conn);
    }
}