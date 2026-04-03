namespace DbSyncEngine.Application.Strategies.Options;

public class DbEndpoint
{
    public string Name { get; set; } = default!;
    public string Provider { get; set; } = default!;
    public string ConnectionString { get; set; } = default!;
    public string Table { get; set; } = default!;
    public string? Schema { get; set; }
}