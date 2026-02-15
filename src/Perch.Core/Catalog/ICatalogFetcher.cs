namespace Perch.Core.Catalog;

public interface ICatalogFetcher
{
    Task<string> FetchAsync(string relativePath, CancellationToken cancellationToken = default);
}
