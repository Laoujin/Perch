namespace Perch.Core.Catalog;

public sealed class FileCatalogCache : ICatalogCache
{
    private readonly string _cacheDir;

    public FileCatalogCache(string cacheDir)
    {
        _cacheDir = cacheDir;
    }

    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        string path = GetPath(key);
        if (!File.Exists(path))
        {
            return null;
        }

        return await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
    }

    public async Task SetAsync(string key, string content, CancellationToken cancellationToken = default)
    {
        string path = GetPath(key);
        string? dir = Path.GetDirectoryName(path);
        if (dir != null)
        {
            Directory.CreateDirectory(dir);
        }

        await File.WriteAllTextAsync(path, content, cancellationToken).ConfigureAwait(false);
    }

    public void InvalidateAll()
    {
        if (Directory.Exists(_cacheDir))
            Directory.Delete(_cacheDir, recursive: true);
    }

    private string GetPath(string key)
    {
        string sanitized = key.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        return Path.Combine(_cacheDir, sanitized);
    }
}
