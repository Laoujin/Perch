using System.Net.Http;

using Perch.Core.Config;

namespace Perch.Core.Catalog;

public sealed class SettingsAwareCatalogFetcher : ICatalogFetcher
{
    private readonly ISettingsProvider _settingsProvider;
    private readonly HttpClient _httpClient;

    public SettingsAwareCatalogFetcher(ISettingsProvider settingsProvider, HttpClient httpClient)
    {
        _settingsProvider = settingsProvider;
        _httpClient = httpClient;
    }

    public async Task<string> FetchAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var settings = await _settingsProvider.LoadAsync(cancellationToken).ConfigureAwait(false);

        ICatalogFetcher fetcher = !string.IsNullOrWhiteSpace(settings.GalleryLocalPath)
            ? new LocalCatalogFetcher(settings.GalleryLocalPath)
            : new HttpCatalogFetcher(_httpClient, settings.GalleryUrl);

        return await fetcher.FetchAsync(relativePath, cancellationToken).ConfigureAwait(false);
    }
}
