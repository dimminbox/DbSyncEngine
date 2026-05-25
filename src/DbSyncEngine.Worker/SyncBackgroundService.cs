using DbSyncEngine.Application.Strategies.Abstractions;
using DbSyncEngine.Application.Strategies.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace DbSyncEngine.Worker;

public class SyncBackgroundService : BackgroundService
{
    private readonly ISyncStrategyFactory _factory;
    private readonly IOptionsMonitor<SyncConfig> _config;
    private readonly ILogger<SyncBackgroundService> _logger;

    // Кешированные политики повтора, ключ — имя сущности.
    // Перестраиваются при горячей перезагрузке конфига.
    private volatile IReadOnlyDictionary<string, IAsyncPolicy> _policies;

    public SyncBackgroundService(
        ISyncStrategyFactory factory,
        IOptionsMonitor<SyncConfig> config,
        ILogger<SyncBackgroundService> logger)
    {
        _factory = factory;
        _config = config;
        _logger = logger;

        _policies = BuildPolicies(_config.CurrentValue);
        _config.OnChange(cfg => _policies = BuildPolicies(cfg));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var config = _config.CurrentValue;
            var policies = _policies;

            foreach (var configEntity in config.Entities ?? [])
            {
                var strategy = _factory.Create(configEntity);

                // Fallback на случай если сущность появилась после последней сборки политик
                if (!policies.TryGetValue(configEntity.Name, out var retryPolicy))
                    retryPolicy = BuildEntityPolicy(configEntity);

                try
                {
                    await retryPolicy.ExecuteAsync(
                        ct => strategy.RunAsync(ct),
                        stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex,
                        "[{Entity}] failed after all retries, skipping until next interval",
                        configEntity.Name);
                }

                await Task.Delay(TimeSpan.FromSeconds(configEntity.IntervalSeconds), stoppingToken);
            }
        }
    }

    private IReadOnlyDictionary<string, IAsyncPolicy> BuildPolicies(SyncConfig config) =>
        (config.Entities ?? [])
            .ToDictionary(
                e => e.Name,
                e => BuildEntityPolicy(e));

    private IAsyncPolicy BuildEntityPolicy(SyncEntityConfig entity) =>
        Policy
            .Handle<Exception>(ex => ex is not OperationCanceledException)
            .WaitAndRetryAsync(
                retryCount: entity.MaxInsertRetries,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, delay, attempt, _) =>
                    _logger.LogWarning(exception,
                        "[{Entity}] failed on attempt {Attempt}, retrying in {Delay}",
                        entity.Name, attempt, delay));
}
