namespace Perch.Core.Catalog;

public interface ICatalogCache
{
    Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task SetAsync(string key, string content, CancellationToken cancellationToken = default);
}
