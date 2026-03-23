using System.Linq.Expressions;
using DbSyncEngine.Application.Entities;
using DbSyncEngine.Application.Helper;
using DbSyncEngine.Application.Pipelines.Abstractions;
using DbSyncEngine.Application.Pipelines.Common;
using DbSyncEngine.Application.Providers.Abstractions;
using MicroOrm.Dapper.Repositories.Config;
using MicroOrm.Dapper.Repositories.SqlGenerator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Relef.Repository.Helpers;
using Relef.Repository.Interfaces;

namespace DbSyncEngine.Application.Pipelines.Steps.Common;

public class PrepareReloadStep : ISyncStep
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PrepareReloadStep> _logger;

    public PrepareReloadStep(IPostgresProvider postgresProvider, ILogger<PrepareReloadStep> logger)
    {
        _serviceProvider = postgresProvider.Provider;
        _logger = logger;
    }

    public async Task HandleAsync(SyncContext ctx, Func<Task> next)
    {
        var filter = new List<IReadOnlyList<Expression<Func<ReloadState, bool>>>>()
        {
            new List<Expression<Func<ReloadState, bool>>>()
            {
                c => c.Id > 0 && c.SyncDirectionRaw == ctx.Direction.ToString()
            },
        };
        //TODO
        Result<IReadOnlyList<ReloadState>> result = new();
        result.ThrowIfFailed();

        ReloadState state;
        if (result.Data?.Count == 0)
        {
            state = ReloadState.CreateNew(ctx.Direction);
            (await repo.InsertAsync(state).ConfigureAwait(false)).ThrowIfFailed();
            _logger.LogInformation("Full reload state created.");
        }
        else
        {
            state = result.Data.Single();
            if (state.RestartRequested)
            {
                _logger.LogInformation("Restart requested. Resetting state.");
                state.Reset();
                (await repo.UpdateAsync(state)).ThrowIfFailed();
            }
        }

        ctx.ReloadState = state;

        await next();
    }
}