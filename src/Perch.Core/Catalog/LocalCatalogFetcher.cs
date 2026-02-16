namespace Perch.Core.Catalog;

public sealed class LocalCatalogFetcher : ICatalogFetcher
{
    private readonly string _basePath;

    public LocalCatalogFetcher(string basePath)
    {
        _basePath = basePath;
    }

    public async Task<string> FetchAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        string fullPath = Path.Combine(_basePath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        return await File.ReadAllTextAsync(fullPath, cancellationToken).ConfigureAwait(false);
    }
}
