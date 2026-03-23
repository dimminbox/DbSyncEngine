using DbSyncEngine.Domain.Exceptions;

namespace DbSyncEngine.Domain.SyncEntityAggregate.Exceptions;

public class InvalidDescriptorException : DomainException
{
    public InvalidDescriptorException(string message) : base(message)
    {
    }
}