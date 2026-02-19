namespace Perch.Core.Catalog;

public sealed class NoOpCatalogCache : ICatalogCache
{
    public Task<string?> GetAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>(null);

    public Task SetAsync(string key, string content, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public void InvalidateAll() { }
}
