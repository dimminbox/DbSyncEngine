namespace DbSyncEngine.Application.Helper;

public class Result<T>
{
    public bool IsSuccess => this.Errors.Count == 0;

    public List<string> Errors { get; private set; } = new List<string>();

    public T Data { get; set; }

    public void Merge(Result<T> result)
    {
        if ((object) result.Data != null)
            this.Data = result.Data;
        this.Errors.AddRange((IEnumerable<string>) result.Errors);
    }

    public string GetErrorsString() => string.Join(", ", (IEnumerable<string>) this.Errors);

    public Result<T> AddError(string err)
    {
        this.Errors.Add(err);
        return this;
    }
}