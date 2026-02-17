using System.Collections.Immutable;

using Perch.Core;
using Perch.Core.Catalog;
using Perch.Core.Config;
using Perch.Core.Modules;
using Perch.Desktop.Models;

namespace Perch.Desktop.Services;

public sealed class DotfileDetailService : IDotfileDetailService
{
    private readonly IModuleDiscoveryService _moduleDiscovery;
    private readonly ICatalogService _catalog;
    private readonly ISettingsProvider _settings;
    private readonly IPlatformDetector _platformDetector;

    public DotfileDetailService(
        IModuleDiscoveryService moduleDiscovery,
        ICatalogService catalog,
        ISettingsProvider settings,
        IPlatformDetector platformDetector)
    {
        _moduleDiscovery = moduleDiscovery;
        _catalog = catalog;
        _settings = settings;
        _platformDetector = platformDetector;
    }

    public async Task<DotfileDetail> LoadDetailAsync(DotfileGroupCardModel group, CancellationToken cancellationToken = default)
    {
        var perchSettings = await _settings.LoadAsync(cancellationToken);
        var configRepoPath = perchSettings.ConfigRepoPath;

        if (string.IsNullOrWhiteSpace(configRepoPath))
        {
            return new DotfileDetail(group, null, null, null, null, []);
        }

        var discovery = await _moduleDiscovery.DiscoverAsync(configRepoPath, cancellationToken);
        var platform = _platformDetector.CurrentPlatform;

        var firstFilePath = group.Files.IsDefaultOrEmpty ? null : group.Files[0].FullPath;
        var owningModule = firstFilePath is not null
            ? FindOwningModule(discovery.Modules, firstFilePath, platform)
            : FindOwningModuleByGalleryId(discovery.Modules, group.Id);

        if (owningModule is null)
        {
            owningModule = FindOwningModuleByGalleryId(discovery.Modules, group.Id);
        }

        if (owningModule is null)
        {
            return new DotfileDetail(group, null, null, null, null, []);
        }

        var manifestPath = Path.Combine(owningModule.ModulePath, "manifest.yaml");
        string? manifestYaml = null;
        AppManifest? manifest = null;

        if (File.Exists(manifestPath))
        {
            manifestYaml = await File.ReadAllTextAsync(manifestPath, cancellationToken);
            var parser = new ManifestParser();
            var result = parser.Parse(manifestYaml, owningModule.Name);
            if (result.IsSuccess)
            {
                manifest = result.Manifest;
            }
        }

        var alternatives = await FindAlternativesAsync(manifest?.GalleryId ?? group.Id, cancellationToken);

        return new DotfileDetail(group, owningModule, manifest, manifestYaml, manifestPath, alternatives);
    }

    internal static AppModule? FindOwningModuleByGalleryId(
        ImmutableArray<AppModule> modules,
        string galleryId)
    {
        foreach (var module in modules)
        {
            if (string.Equals(module.Name, galleryId, StringComparison.OrdinalIgnoreCase))
                return module;
        }

        return null;
    }

    internal static AppModule? FindOwningModule(
        ImmutableArray<AppModule> modules,
        string dotfilePath,
        Platform platform)
    {
        var normalizedDotfile = NormalizePath(dotfilePath);

        foreach (var module in modules)
        {
            foreach (var link in module.Links)
            {
                var target = link.GetTargetForPlatform(platform);
                if (target is null)
                {
                    continue;
                }

                var expandedTarget = NormalizePath(EnvironmentExpander.Expand(target));
                if (string.Equals(expandedTarget, normalizedDotfile, StringComparison.OrdinalIgnoreCase))
                {
                    return module;
                }
            }
        }

        return null;
    }

    private async Task<ImmutableArray<CatalogEntry>> FindAlternativesAsync(
        string? galleryId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(galleryId))
        {
            return [];
        }

        var catalogEntry = await _catalog.GetAppAsync(galleryId, cancellationToken);
        if (catalogEntry is null)
        {
            return [];
        }

        var allApps = await _catalog.GetAllAppsAsync(cancellationToken);
        return allApps
            .Where(a => string.Equals(a.Category, catalogEntry.Category, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(a.Id, galleryId, StringComparison.OrdinalIgnoreCase))
            .ToImmutableArray();
    }

    private static string NormalizePath(string path) =>
        path.Replace('/', '\\').TrimEnd('\\');
}
