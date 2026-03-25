using DbSyncEngine.Application.Persistence.Abstracts;
using DbSyncEngine.Infrastructure.Persistence.Abstractions;
using DbSyncEngine.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;

namespace DbSyncEngine.Infrastructure.Persistence.Fabrics;

public class SyncProcessRepositoryFactory : ISyncProcessRepositoryFactory
{
    private readonly IDbConnectionFactory _connections;
    private readonly string _connectionString;

    public SyncProcessRepositoryFactory(
        IDbConnectionFactory connections,
        IConfiguration config)
    {
        _connections = connections;
        _connectionString = config.GetConnectionString("SyncProcessDb")
                            ?? throw new InvalidOperationException("Missing SyncProcessDb connection string");
    }

    public ISyncProcessRepository Create()
    {
        var conn = _connections.Create("Postgres", _connectionString);
        return new SyncProcessRepository(conn);
    }
}