namespace DbSyncEngine.Infrastructure.Persistence.Schema.Ddl;

public interface ITargetDdlGeneratorFactory
{
    ITargetDdlGenerator Create(string provider, IDictionary<string,string>? options = null);
}