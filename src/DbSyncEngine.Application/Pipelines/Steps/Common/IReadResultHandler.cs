using DbSyncEngine.Application.Pipelines.Common;
using Relef.Repository.Models;

namespace DbSyncEngine.Application.Pipelines.Steps.Common;

public interface IReadResultHandler<T> where T : class
{
    Task HandleAsync(SyncContext ctx, IReadOnlyList<T> data);
}