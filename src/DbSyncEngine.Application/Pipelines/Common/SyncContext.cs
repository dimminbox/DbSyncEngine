using DbSyncEngine.Application.Strategies.Options;
using DbSyncEngine.Domain.SyncProcessAggregate;
using DbSyncEngine.Domain.SyncProcessAggregate.Enums;

namespace DbSyncEngine.Application.Pipelines.Common;

public class SyncContext
{
    // --- Неизменяемые поля, известны на старте пайплайна ---

    public SyncEntityConfig Config { get; }
    public SyncDirection Direction { get; }
    public DateTimeOffset Now { get; }
    public CancellationToken CancellationToken { get; }

    // --- Process: задаётся GetSyncStep, обязателен для всех последующих шагов ---

    private SyncProcess? _process;

    public SyncProcess Process
    {
        get => _process ?? throw new InvalidOperationException(
            $"SyncProcess не инициализирован — GetSyncStep должен выполниться до обращения к {nameof(Process)}.");
        set => _process = value ?? throw new ArgumentNullException(nameof(value));
    }

    // --- Текущий батч: контролируемый переход состояния между шагами ---

    public IReadOnlyList<RowData> CurrentBatch { get; private set; } = [];

    public void SetBatch(IReadOnlyList<RowData> batch)
        => CurrentBatch = batch ?? throw new ArgumentNullException(nameof(batch));

    public void ClearBatch()
        => CurrentBatch = [];

    // ---

    public SyncContext(SyncEntityConfig config, SyncDirection direction, CancellationToken ct)
    {
        Config = config;
        Direction = direction;
        Now = DateTimeOffset.UtcNow;
        CancellationToken = ct;
    }
}
