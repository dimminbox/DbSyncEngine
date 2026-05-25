using DbSyncEngine.Application.Persistence;
using DbSyncEngine.Infrastructure.Persistence.Abstractions;
using DbSyncEngine.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Npgsql;

namespace DbSyncEngine.Infrastructure.Persistence.Fabrics;

public class TableDataRepositoryFactory : ITableDataRepositoryFactory
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILoggerFactory _loggerFactory;

    public TableDataRepositoryFactory(IDbConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
    {
        _connectionFactory = connectionFactory;
        _loggerFactory = loggerFactory;
    }

    public ITableDataRepository Create(string provider, string connectionString, string? schema = null)
    {
        var connection = _connectionFactory.Create(provider, connectionString);

        return provider switch
        {
            DbProviders.MySql => new MySqlTableDataRepository(
                (MySqlConnection)connection,
                _loggerFactory.CreateLogger<MySqlTableDataRepository>()),
            DbProviders.PostgreSql => new PostgresTableDataRepository(
                (NpgsqlConnection)connection,
                _loggerFactory.CreateLogger<PostgresTableDataRepository>(),
                schema),
            _ => throw new NotSupportedException($"Unsupported provider: {provider}")
        };
    }
}
