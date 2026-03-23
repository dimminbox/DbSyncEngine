namespace DbSyncEngine.Application.Strategies.Abstractions;

public interface ISyncStrategy
{
    Task RunAsync(CancellationToken ct);
}