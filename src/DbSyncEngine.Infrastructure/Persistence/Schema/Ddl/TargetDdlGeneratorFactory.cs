namespace DbSyncEngine.Infrastructure.Persistence.Schema.Ddl;

public class TargetDdlGeneratorFactory : ITargetDdlGeneratorFactory
{
    private readonly IReadOnlyDictionary<string, Func<IDictionary<string, string>?, ITargetDdlGenerator>> _map;

    public TargetDdlGeneratorFactory()
    {
        _map = new Dictionary<string, Func<IDictionary<string, string>?, ITargetDdlGenerator>>(StringComparer
            .OrdinalIgnoreCase)
        {
            ["MySQL"] = opts => new MySqlDdlGenerator(),
            ["PostgreSQL"] = opts => new PostgresDdlGenerator()
        };
    }

    public ITargetDdlGenerator Create(string provider, IDictionary<string, string>? options = null)
    {
        if (!_map.TryGetValue(provider, out var factory))
            throw new NotSupportedException($"Unsupported DDL provider: '{provider}'");

        return factory(options);
    }
}