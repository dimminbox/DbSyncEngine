namespace DbSyncEngine.Application.Strategies.Options;

public class FullReloadOptions
{
    // сколько строк читаем за раз
    public int ChunkSize { get; set; } = 10_000;

    // сколько строк вставляем за раз
    public int InsertBatchSize { get; set; } = 5_000;

    // сколько раз повторять при ошибке
    public int MaxInsertRetries { get; set; } = 3;
}