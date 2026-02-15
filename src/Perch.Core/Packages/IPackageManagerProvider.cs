namespace Perch.Core.Packages;

public interface IPackageManagerProvider
{
    PackageManager Manager { get; }
    Task<PackageManagerScanResult> ScanInstalledAsync(CancellationToken cancellationToken = default);
}
