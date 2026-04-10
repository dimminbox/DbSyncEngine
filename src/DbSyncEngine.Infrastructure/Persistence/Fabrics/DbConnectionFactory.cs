using System.Data;
using DbSyncEngine.Infrastructure.Persistence.Abstractions;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;

namespace DbSyncEngine.Infrastructure.Persistence.Fabrics;

public class DbConnectionFactory : IDbConnectionFactory
{
    public IDbConnection Create(string provider, string connectionString)
    {
        return provider switch
        {
            "SQLite" => new SqliteConnection(connectionString),
            "MySQL"    => new MySqlConnection(connectionString),
            "PostgreSQL" => new NpgsqlConnection(connectionString),
            _ => throw new NotSupportedException($"Unsupported provider: {provider}")
        };
    }
}