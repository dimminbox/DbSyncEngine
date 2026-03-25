namespace DbSyncEngine.Application.Normalization;

public interface IValueNormalizerFactory
{
    IValueNormalizer Create(string provider);
}