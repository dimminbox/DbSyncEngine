namespace DbSyncEngine.Infrastructure;

/// <summary>
/// Строковые идентификаторы провайдеров БД.
/// Используются как ключи в конфигурации, фабриках и switch-выражениях.
/// Значения должны совпадать со строками в config.json (Provider: "MySQL" / "PostgreSQL" / "SQLite").
/// </summary>
public static class DbProviders
{
    public const string MySql      = "MySQL";
    public const string PostgreSql = "PostgreSQL";
    public const string SQLite     = "SQLite";
}
