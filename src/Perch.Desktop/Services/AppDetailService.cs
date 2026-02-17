using System.Collections.Immutable;

using Perch.Core.Catalog;
using Perch.Core.Config;
using Perch.Core.Modules;
using Perch.Desktop.Models;

namespace Perch.Desktop.Services;

public sealed class AppDetailService : IAppDetailService
{
    private readonly IModuleDiscoveryService _moduleDiscovery;
    private readonly ICatalogService _catalog;
    private readonly ISettingsProvider _settings;

    public AppDetailService(
        IModuleDiscoveryService moduleDiscovery,
        ICatalogService catalog,
        ISettingsProvider settings)
    {
        _moduleDiscovery = moduleDiscovery;
        _catalog = catalog;
        _settings = settings;
    }

    public async Task<AppDetail> LoadDetailAsync(AppCardModel card, CancellationToken cancellationToken = default)
    {
        var perchSettings = await _settings.LoadAsync(cancellationToken);
        var configRepoPath = perchSettings.ConfigRepoPath;

        if (string.IsNullOrWhiteSpace(configRepoPath))
        {
            return new AppDetail(card, null, null, null, null, []);
        }

        var discovery = await _moduleDiscovery.DiscoverAsync(configRepoPath, cancellationToken);

        var (owningModule, manifest, manifestYaml, manifestPath) = await FindModuleByGalleryIdAsync(
            discovery.Modules, card.Id, cancellationToken);

        var alternatives = await FindAlternativesAsync(card.Category, card.Id, cancellationToken);

        return new AppDetail(card, owningModule, manifest, manifestYaml, manifestPath, alternatives);
    }

    internal static async Task<(AppModule? Module, AppManifest? Manifest, string? Yaml, string? Path)>
        FindModuleByGalleryIdAsync(
            ImmutableArray<AppModule> modules,
            string galleryId,
            CancellationToken cancellationToken)
    {
        foreach (var module in modules)
        {
            var manifestPath = System.IO.Path.Combine(module.ModulePath, "manifest.yaml");
            if (!File.Exists(manifestPath))
                continue;

            var yaml = await File.ReadAllTextAsync(manifestPath, cancellationToken);
            var parser = new ManifestParser();
            var result = parser.Parse(yaml, module.Name);
            if (!result.IsSuccess)
                continue;

            if (string.Equals(result.Manifest!.GalleryId, galleryId, StringComparison.OrdinalIgnoreCase))
            {
                return (module, result.Manifest, yaml, manifestPath);
            }
        }

        return (null, null, null, null);
    }

    private async Task<ImmutableArray<CatalogEntry>> FindAlternativesAsync(
        string category,
        string galleryId,
        CancellationToken cancellationToken)
    {
        var allApps = await _catalog.GetAllAppsAsync(cancellationToken);
        return allApps
            .Where(a => string.Equals(a.Category, category, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(a.Id, galleryId, StringComparison.OrdinalIgnoreCase))
            .ToImmutableArray();
    }
}
