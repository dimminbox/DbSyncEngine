using System.Linq.Expressions;

namespace DbSyncEngine.Application.Pipelines.Steps.Configs;

public class ReloadEntityConfig<T>
{
    // Поле, по которому читаем чанки (Id, DateUpdate, Guid, Timestamp)
    public Expression<Func<T, object>> KeySelector { get; init; }

    // Как сравнивать (>, >=, <, <=)
    public Func<object, object, bool> Comparison { get; init; }

    // Преобразование значения ключа в object (например, DateTime → long ticks)
    public Func<T, object> KeyValueExtractor { get; init; }
    
    public Func<string, object> ParseKey { get; init; }
    
    public Expression<Func<T, bool>> InitialFilter { get; init; }
}