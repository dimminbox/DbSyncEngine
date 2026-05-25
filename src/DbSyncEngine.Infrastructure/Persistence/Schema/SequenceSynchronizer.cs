using Dapper;
using DbSyncEngine.Application.Persistence.Schema;
using DbSyncEngine.Application.Pipelines.Common;
using DbSyncEngine.Infrastructure.Persistence.Abstractions;
using DbSyncEngine.Infrastructure.Persistence.Schema.Ddl;
using Microsoft.Extensions.Logging;

namespace DbSyncEngine.Infrastructure.Persistence.Schema;

public class SequenceSynchronizer : ISequenceSynchronizer
{
    private readonly ITargetDdlGeneratorFactory _ddlFactory;
    private readonly IDbConnectionFactory _connections;
    private readonly ILogger<SequenceSynchronizer> _logger;

    public SequenceSynchronizer(
        ITargetDdlGeneratorFactory ddlFactory,
        IDbConnectionFactory connections,
        ILogger<SequenceSynchronizer> logger)
    {
        _ddlFactory = ddlFactory;
        _connections = connections;
        _logger = logger;
    }

    public async Task SyncAsync(SyncContext ctx, string columnName, CancellationToken ct)
    {
        var provider = ctx.Config.Target.Provider;
        var ddl = _ddlFactory.Create(provider);
        var sql = ddl.GenerateSyncSequenceSql(ctx.Config.Target.Table, columnName, ctx.Config.Target.Schema);

        if (string.IsNullOrWhiteSpace(sql))
        {
            _logger.LogDebug("Sequence sync not required for provider {Provider}", provider);
            return;
        }

        _logger.LogInformation("Syncing sequence for {Table}.{Column}", ctx.Config.Target.Table, columnName);
        _logger.LogDebug("Sequence sync SQL: {Sql}", sql);

        using var conn = _connections.Create(provider, ctx.Config.Target.ConnectionString);
        await conn.ExecuteAsync(new CommandDefinition(sql, cancellationToken: ct, commandTimeout: ddl.CommandTimeout));
    }
}
