namespace DbSyncEngine.Application.Pipelines.Steps.FullSyncSteps;

public class EnsureSchemaOptions
{
    public bool DryRun { get; set; } = false;
    public bool ReplaceIfExists { get; set; } = true; // true = DROP+CREATE or ReplaceTableAsync
    public int Parallelism { get; set; } = 4;
    public int RetryCount { get; set; } = 2;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
}