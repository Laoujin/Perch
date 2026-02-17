using System.Collections.Immutable;

using Perch.Core.Packages;

namespace Perch.Core.Scanner;

public sealed class SystemScanner : ISystemScanner
{
    private readonly IFontScanner _fontScanner;
    private readonly IVsCodeService _vsCodeService;
    private readonly IEnumerable<IPackageManagerProvider> _packageProviders;

    public SystemScanner(
        IFontScanner fontScanner,
        IVsCodeService vsCodeService,
        IEnumerable<IPackageManagerProvider> packageProviders)
    {
        _fontScanner = fontScanner;
        _vsCodeService = vsCodeService;
        _packageProviders = packageProviders;
    }

    public async Task<SystemScanResult> ScanAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        var warnings = new List<string>();

        progress?.Report("Scanning installed applications...");
        var installedPackages = new List<InstalledPackage>();
        foreach (var provider in _packageProviders)
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

        progress?.Report("Detecting installed fonts...");
        var fonts = await _fontScanner.ScanAsync(cancellationToken).ConfigureAwait(false);

        progress?.Report("Reading VS Code extensions...");
        bool vsCodeDetected = _vsCodeService.IsInstalled;
        var extensions = vsCodeDetected
            ? await _vsCodeService.GetInstalledExtensionsAsync(cancellationToken).ConfigureAwait(false)
            : ImmutableArray<DetectedVsCodeExtension>.Empty;

        return new SystemScanResult(
            installedPackages.ToImmutableArray(),
            fonts,
            extensions,
            ImmutableArray<string>.Empty,
            vsCodeDetected,
            warnings.ToImmutableArray());
    }
}
