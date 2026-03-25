using System.Data;
using DbSyncEngine.Infrastructure.Persistence.Abstractions;
using MySqlConnector;
using Npgsql;

namespace DbSyncEngine.Infrastructure.Persistence.Fabrics;

public class DbConnectionFactory : IDbConnectionFactory
{
    public IDbConnection Create(string provider, string connectionString)
    {
        return provider switch
        {
            "MySQL"    => new MySqlConnection(connectionString),
            "Postgres" => new NpgsqlConnection(connectionString),
            _ => throw new NotSupportedException($"Unsupported provider: {provider}")
        };
    }
}