using DbSyncEngine.Application.Persistence;
using DbSyncEngine.Infrastructure.Persistence.Abstractions;
using DbSyncEngine.Infrastructure.Persistence.Repositories;
using MySqlConnector;
using Npgsql;

namespace DbSyncEngine.Infrastructure.Persistence.Fabrics;

public class TableDataRepositoryFactory : ITableDataRepositoryFactory
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TableDataRepositoryFactory(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    public ITableDataRepository Create(string provider, string connectionString)
    {
        var connection = _connectionFactory.Create(provider, connectionString);

        return provider switch
        {
            "MySQL"    => new MySqlTableDataRepository((MySqlConnection)connection),
            "Postgres" => new PostgresTableDataRepository((NpgsqlConnection)connection),
            _ => throw new NotSupportedException($"Unsupported provider: {provider}")
        };
    }
}