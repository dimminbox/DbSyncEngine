namespace DbSyncEngine.Application.Persistence.Schema;

public interface ISchemaReaderFactory
{
    ISchemaReader Create(string provider);
}