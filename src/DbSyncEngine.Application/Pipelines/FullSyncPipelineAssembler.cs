using DbSyncEngine.Application.Pipelines.Abstractions;
using DbSyncEngine.Application.Pipelines.Steps.FullSyncSteps;

namespace DbSyncEngine.Application.Pipelines;

public interface IFullSyncPipelineAssembler
{
    IReadOnlyList<ISyncStep> Steps { get; }
}

/// <summary>
/// Объявляет все шаги Full Sync пайплайна явно через конструктор.
/// DI-контейнер проверяет регистрацию каждого шага при старте приложения,
/// а не в момент первого вызова RunAsync.
/// </summary>
public class FullSyncPipelineAssembler : IFullSyncPipelineAssembler
{
    public IReadOnlyList<ISyncStep> Steps { get; }

    public FullSyncPipelineAssembler(
        GetSyncStep getSyncStep,
        EnsureTargetSchemaStep ensureTargetSchemaStep,
        ReadDataStep readDataStep,
        MapChunkStep mapChunkStep,
        PrepareToWriteDataStep prepareToWriteDataStep,
        WriteDataStep writeDataStep,
        UpdateSyncStep updateSyncStep,
        SyncSequencesStep syncSequencesStep)
    {
        Steps = new List<ISyncStep>
        {
            getSyncStep,
            ensureTargetSchemaStep,
            readDataStep,
            mapChunkStep,
            prepareToWriteDataStep,
            writeDataStep,
            updateSyncStep,
            syncSequencesStep,
        };
    }
}
