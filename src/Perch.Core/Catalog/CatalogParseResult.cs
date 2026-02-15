namespace Perch.Core.Catalog;

public sealed record CatalogParseResult<T>
{
    public T? Value { get; }
    public string? Error { get; }
    public bool IsSuccess => Value != null;

    private CatalogParseResult(T? value, string? error)
    {
        Value = value;
        Error = error;
    }

    public static CatalogParseResult<T> Ok(T value) => new(value, null);
    public static CatalogParseResult<T> Failure(string error) => new(default, error);
}
