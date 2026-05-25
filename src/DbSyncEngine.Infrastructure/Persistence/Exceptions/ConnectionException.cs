namespace DbSyncEngine.Infrastructure.Persistence.Exceptions;

public class ConnectionException : Exception
{
    public ConnectionException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}