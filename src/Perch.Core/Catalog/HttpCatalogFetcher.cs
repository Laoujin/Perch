using System.Net.Http;

namespace Perch.Core.Catalog;

public sealed class HttpCatalogFetcher : ICatalogFetcher
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public HttpCatalogFetcher(HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public async Task<string> FetchAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        string url = $"{_baseUrl}/{relativePath.TrimStart('/')}";
        var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }
}
