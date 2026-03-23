using System.Data;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Npgsql;

namespace DbSyncEngine.Application.Helper;

/// <summary>
/// Creates IDbConnection instances based on configured database engine.
/// </summary>
public static class ConnectionFactory
{
    private const string MysqlConnectionStringCode = "MySQL:ConnectionString";
    private const string PostgresConnectionStringCode = "PostgreSQL:ConnectionString";

    private static readonly Dictionary<ConnectionEngine, Func<IConfiguration, IDbConnection>> _factories =
        new()
        {
            { ConnectionEngine.MySqL, cfg => new MySqlConnection(cfg[MysqlConnectionStringCode]) },
            { ConnectionEngine.Postgres, cfg => new NpgsqlConnection(cfg[PostgresConnectionStringCode]) }
        };

    /// <summary>
    /// Returns a database connection for the configured engine (MySQL/PostgreSQL).
    /// </summary>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="engine">Database's engine </param>
    /// <returns>IDbConnection instance.</returns>
    /// <exception cref="Exception">Thrown when engine is unknown.</exception>
    public static IDbConnection GetConnection(IConfiguration configuration, ConnectionEngine? engine = null)
    {
        if (engine == null)
        {
            string engineName = configuration?["DatabaseEngine"].ToUpperInvariant();
            engine = engineName switch
            {
                "MYSQL" => ConnectionEngine.MySqL,
                "POSTGRESQL" => ConnectionEngine.Postgres,
                _ => throw new InvalidOperationException("Unknown database engine")
            };
        }

        if (_factories.TryGetValue(engine.Value, out var factory))
            return factory(configuration);

        throw new InvalidOperationException("Unknown database engine");
    }
}

/// <summary>
/// Enum for Databases's engine
/// </summary>
public enum ConnectionEngine
{
    /// <summary>
    /// Postgres
    /// </summary>
    Postgres,

    /// <summary>
    /// Mysql
    /// </summary>
    MySqL
}