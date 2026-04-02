using System.Data;
using Dapper;
using DbSyncEngine.Application.Persistence;
using DbSyncEngine.Domain.SyncProcessAggregate;
using DbSyncEngine.Domain.SyncProcessAggregate.Enums;
using DbSyncEngine.Infrastructure.Persistence.Common;
using DbSyncEngine.Infrastructure.Persistence.Schema.SyncProcess;
using Microsoft.Data.Sqlite;

namespace DbSyncEngine.Infrastructure.Persistence.Repositories;

public class SyncProcessRepository : DapperRepository<SyncProcess>, ISyncProcessRepository
{
    private readonly IDbConnection _connection;
    public SyncProcessRepository(IDbConnection connection)
        : base(connection)
    {
        _connection = connection;
    }

    public Task<SyncProcess?> GetAsync(long id, CancellationToken ct)
        => QuerySingleAsync(SyncProcessSql.GetById, new { id });

    public void InitDb()
    {
        _connection.Open();
        SyncProcessSchemaInitializer.EnsureCreated(_connection);
    }

    public Task<SyncProcess?> GetAsync(
        string entityName,
        string sourceProvider,
        string targetProvider,
        SyncDirection direction,
        CancellationToken ct)
    {
        return QuerySingleAsync(
            SyncProcessSql.GetByCompositeKey,
            new
            {
                EntityName = entityName,
                SourceProvider = sourceProvider,
                TargetProvider = targetProvider,
                DirectionString = direction.ToString()
            });
    }

    public async Task SaveAsync(SyncProcess process, CancellationToken ct)
    {
        if (process.Id == 0)
        {
            var id = await Connection.ExecuteScalarAsync<long>(
                SyncProcessSql.Insert, process);
            process.SetId(id);
        }
        else
        {
            await ExecuteAsync(SyncProcessSql.Update, process);
        }
    }
}