using DbSyncEngine.Application.Strategies.Abstractions;
using DbSyncEngine.Application.Strategies.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;

namespace DbSyncEngine.Worker;

public class SyncBackgroundService : BackgroundService
{
    private readonly ISyncStrategyFactory _factory;
    private readonly IOptionsMonitor<List<SyncEntityConfig>> _configs;

    public SyncBackgroundService(
        ISyncStrategyFactory factory,
        IOptionsMonitor<List<SyncEntityConfig>> configs)
    {
        _factory = factory;
        _configs = configs;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var configs = _configs.CurrentValue;

            foreach (var config in configs)
            {
                var strategy = _factory.Create(config);

                var retryPolicy = Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(
                        retryCount: config.MaxInsertRetries,
                        sleepDurationProvider: attempt =>
                            TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                        onRetry: (exception, delay, attempt, _) =>
                        {
                            Console.WriteLine(
                                $"[{config.Name}] failed on attempt {attempt}. Retrying in {delay}. Error: {exception.Message}");
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
                        $"[{config.Name}] failed after all retries. Skipping. Error: {ex.Message}");
                }
            }

            // глобальная пауза между циклами
            var minInterval = configs.Min(c => c.IntervalSeconds);
            await Task.Delay(TimeSpan.FromSeconds(minInterval), stoppingToken);
        }
    }
}
