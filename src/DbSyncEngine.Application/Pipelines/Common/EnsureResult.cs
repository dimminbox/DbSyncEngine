using Relef.Repository.Helpers;

namespace DbSyncEngine.Application.Pipelines.Common;

public static class ResultExtensions
{
    public static void ThrowIfFailed<T>(this Result<IReadOnlyList<T>> result)
    {
        if (!result.IsSuccess)
            throw new InvalidOperationException(string.Join("; ", result.Errors));
    }

    public static void ThrowIfFailed<T>(this Result<T> result)
    {
        if (!result.IsSuccess)
            throw new InvalidOperationException(string.Join("; ", result.Errors));
    }

    public static void ThrowIfFailed(this Result<bool> result)
    {
        if (!result.IsSuccess)
            throw new InvalidOperationException(string.Join("; ", result.Errors));
    }
}