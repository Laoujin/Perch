using System.Collections.Immutable;

namespace Perch.Core.Catalog;

public sealed class CatalogService : ICatalogService
{
    private readonly ICatalogFetcher _fetcher;
    private readonly ICatalogCache _cache;
    private readonly CatalogParser _parser;

    private ImmutableArray<CatalogEntry>? _allApps;
    private ImmutableArray<FontCatalogEntry>? _allFonts;
    private ImmutableArray<TweakCatalogEntry>? _allTweaks;

    public CatalogService(ICatalogFetcher fetcher, ICatalogCache cache, CatalogParser parser)
    {
        _fetcher = fetcher;
        _cache = cache;
        _parser = parser;
    }

    public async Task<CatalogIndex> GetIndexAsync(CancellationToken cancellationToken = default)
    {
        string content = await FetchWithCacheAsync("index.yaml", cancellationToken).ConfigureAwait(false);
        var result = _parser.ParseIndex(content);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to parse catalog index: {result.Error}");
        }

        return result.Value!;
    }

    public async Task<CatalogEntry?> GetAppAsync(string id, CancellationToken cancellationToken = default)
    {
        string content = await FetchWithCacheAsync($"apps/{id}.yaml", cancellationToken).ConfigureAwait(false);
        var result = _parser.ParseApp(content, id);
        return result.Value;
    }

    public async Task<FontCatalogEntry?> GetFontAsync(string id, CancellationToken cancellationToken = default)
    {
        string content = await FetchWithCacheAsync($"fonts/{id}.yaml", cancellationToken).ConfigureAwait(false);
        var result = _parser.ParseFont(content, id);
        return result.Value;
    }

    public async Task<TweakCatalogEntry?> GetTweakAsync(string id, CancellationToken cancellationToken = default)
    {
        string content = await FetchWithCacheAsync($"tweaks/{id}.yaml", cancellationToken).ConfigureAwait(false);
        var result = _parser.ParseTweak(content, id);
        return result.Value;
    }

    public async Task<ImmutableArray<CatalogEntry>> GetAllAppsAsync(CancellationToken cancellationToken = default)
    {
        if (_allApps.HasValue)
            return _allApps.Value;

        var index = await GetIndexAsync(cancellationToken).ConfigureAwait(false);
        var apps = new List<CatalogEntry>();
        foreach (var entry in index.Apps)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var app = await GetAppAsync(entry.Id, cancellationToken).ConfigureAwait(false);
            if (app != null)
            {
                apps.Add(app);
            }
        }

        var result = apps.ToImmutableArray();
        _allApps = result;
        return result;
    }

    public async Task<ImmutableArray<FontCatalogEntry>> GetAllFontsAsync(CancellationToken cancellationToken = default)
    {
        if (_allFonts.HasValue)
            return _allFonts.Value;

        var index = await GetIndexAsync(cancellationToken).ConfigureAwait(false);
        var fonts = new List<FontCatalogEntry>();
        foreach (var entry in index.Fonts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var font = await GetFontAsync(entry.Id, cancellationToken).ConfigureAwait(false);
            if (font != null)
            {
                fonts.Add(font);
            }
        }

        var result = fonts.ToImmutableArray();
        _allFonts = result;
        return result;
    }

    public async Task<ImmutableArray<TweakCatalogEntry>> GetAllTweaksAsync(CancellationToken cancellationToken = default)
    {
        if (_allTweaks.HasValue)
            return _allTweaks.Value;

        var index = await GetIndexAsync(cancellationToken).ConfigureAwait(false);
        var tweaks = new List<TweakCatalogEntry>();
        foreach (var entry in index.Tweaks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tweak = await GetTweakAsync(entry.Id, cancellationToken).ConfigureAwait(false);
            if (tweak != null)
            {
                tweaks.Add(tweak);
            }
        }

        var result = tweaks.ToImmutableArray();
        _allTweaks = result;
        return result;
    }

    public async Task<ImmutableArray<CatalogEntry>> GetAllDotfileAppsAsync(CancellationToken cancellationToken = default)
    {
        var index = await GetIndexAsync(cancellationToken).ConfigureAwait(false);
        var dotfileEntries = index.Apps.Where(e => e.Kind == CatalogKind.Dotfile);
        var apps = new List<CatalogEntry>();
        foreach (var entry in dotfileEntries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var app = await GetAppAsync(entry.Id, cancellationToken).ConfigureAwait(false);
            if (app != null)
            {
                apps.Add(app);
            }
        }

        return apps.ToImmutableArray();
    }

    private async Task<string> FetchWithCacheAsync(string path, CancellationToken cancellationToken)
    {
        string? cached = await _cache.GetAsync(path, cancellationToken).ConfigureAwait(false);
        if (cached != null)
        {
            return cached;
        }

        string content = await _fetcher.FetchAsync(path, cancellationToken).ConfigureAwait(false);
        await _cache.SetAsync(path, content, cancellationToken).ConfigureAwait(false);
        return content;
    }
}
