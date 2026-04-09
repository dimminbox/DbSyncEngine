using DbSyncEngine.Application.Strategies.Abstractions;
using DbSyncEngine.Application.Strategies.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;

namespace DbSyncEngine.Worker;

public class SyncBackgroundService : BackgroundService
{
    private readonly ISyncStrategyFactory _factory;
    private readonly IOptionsMonitor<SyncConfig> _config;

    public SyncBackgroundService(
        ISyncStrategyFactory factory,
        IOptionsMonitor<SyncConfig> config)
    {
        _factory = factory;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var config = _config.CurrentValue;

            foreach (var configEntity in config.Entities ?? [])
            {
                var strategy = _factory.Create(configEntity);

                var retryPolicy = Policy
                    .Handle<Exception>(ex => ex is not OperationCanceledException)
                    .WaitAndRetryAsync(
                        retryCount: configEntity.MaxInsertRetries,
                        sleepDurationProvider: attempt =>
                            TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                        onRetry: (exception, delay, attempt, _) =>
                        {
                            Console.WriteLine(
                                $"[{configEntity.Name}] failed on attempt {attempt}. Retrying in {delay}. Error: {exception.Message}");
                        });

                try
                {
                    await retryPolicy.ExecuteAsync(
                        ct => strategy.RunAsync(ct),
                        stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"[{configEntity.Name}] failed after all retries. Skipping. Error: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(configEntity.IntervalSeconds), stoppingToken);
            }
        }
    }
}