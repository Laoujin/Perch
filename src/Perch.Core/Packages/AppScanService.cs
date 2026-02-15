using System.Collections.Immutable;
using Perch.Core.Modules;

namespace Perch.Core.Packages;

public sealed class AppScanService : IAppScanService
{
    private readonly IModuleDiscoveryService _discoveryService;
    private readonly PackageManifestParser _manifestParser;
    private readonly IEnumerable<IPackageManagerProvider> _providers;

    public AppScanService(
        IModuleDiscoveryService discoveryService,
        PackageManifestParser manifestParser,
        IEnumerable<IPackageManagerProvider> providers)
    {
        _discoveryService = discoveryService;
        _manifestParser = manifestParser;
        _providers = providers;
    }

    public async Task<AppScanResult> ScanAsync(string configRepoPath, CancellationToken cancellationToken = default)
    {
        var warnings = new List<string>();

        // 1. Discover modules
        var discovery = await _discoveryService.DiscoverAsync(configRepoPath, cancellationToken).ConfigureAwait(false);
        var moduleNames = discovery.Modules
            .Select(m => m.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // 2. Parse packages.yaml (optional)
        var declaredPackages = ImmutableArray<PackageDefinition>.Empty;
        string manifestPath = Path.Combine(configRepoPath, "packages.yaml");
        if (File.Exists(manifestPath))
        {
            string yaml = await File.ReadAllTextAsync(manifestPath, cancellationToken).ConfigureAwait(false);
            var parseResult = _manifestParser.Parse(yaml);
            if (parseResult.IsSuccess)
            {
                declaredPackages = parseResult.Packages;
            }
            foreach (string error in parseResult.Errors)
            {
                warnings.Add(error);
            }
        }

        // 3. Scan installed packages from all providers
        var installedPackages = new List<InstalledPackage>();
        foreach (var provider in _providers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var scanResult = await provider.ScanInstalledAsync(cancellationToken).ConfigureAwait(false);
            if (scanResult.IsAvailable)
            {
                installedPackages.AddRange(scanResult.Packages);
            }
            else if (scanResult.ErrorMessage != null)
            {
                warnings.Add(scanResult.ErrorMessage);
            }
        }

        // 4. Cross-reference
        var entries = new List<AppEntry>();
        var installedByName = installedPackages
            .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        // Check installed packages
        foreach (var installed in installedByName.Values)
        {
            bool hasModule = moduleNames.Contains(installed.Name);
            var category = hasModule ? AppCategory.Managed : AppCategory.InstalledNoModule;
            entries.Add(new AppEntry(installed.Name, category, installed.Source));
        }

        // Check declared packages not found as installed
        foreach (var declared in declaredPackages)
        {
            if (!installedByName.ContainsKey(declared.Name))
            {
                entries.Add(new AppEntry(declared.Name, AppCategory.DefinedNotInstalled, declared.Manager));
            }
        }

        return new AppScanResult(
            entries.OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase).ToImmutableArray(),
            warnings.ToImmutableArray());
    }
}
