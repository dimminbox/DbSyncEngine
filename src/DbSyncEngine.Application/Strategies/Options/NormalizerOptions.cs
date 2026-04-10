namespace DbSyncEngine.Application.Strategies.Options;

public class NormalizerOptions
{
    /// <summary>Сохранять ли регистр имён колонок/таблиц как в источнике. Если false — приводим к lower_case.</summary>
    public bool PreserveCase { get; set; } = false;

    /// <summary>Максимальная длина varchar по умолчанию, если не указана в исходной колонке.</summary>
    public int DefaultVarcharLength { get; set; } = 255;

    /// <summary>Если true — сохранять DEFAULT выражения из источника (если поддерживается целевой СУБД).</summary>
    public bool PreserveDefaults { get; set; } = true;

    /// <summary>Если true — при замене таблицы пытаться копировать данные в новую временную таблицу.</summary>
    public bool CopyDataOnReplace { get; set; } = false;

    /// <summary>Правила переименования колонок: ключ — исходное имя, значение — целевое имя.</summary>
    public IDictionary<string, string> ColumnRenameMap { get; set; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>Дополнительные опции, специфичные для провайдера (ключ/значение).</summary>
    public IDictionary<string, string> ProviderOptions { get; set; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}