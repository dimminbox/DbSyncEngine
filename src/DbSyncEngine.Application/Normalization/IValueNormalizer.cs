namespace DbSyncEngine.Application.Normalization;

public interface IValueNormalizer
{
    object? Normalize(object? value);
}