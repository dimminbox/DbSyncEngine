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
            DbProviders.SQLite     => new SqliteConnection(connectionString),
            DbProviders.MySql      => new MySqlConnection(connectionString),
            DbProviders.PostgreSql => new NpgsqlConnection(connectionString),
            _ => throw new NotSupportedException($"Unsupported provider: {provider}")
        };
    }
}
