using System.Collections.Immutable;

namespace Perch.Core.Packages;

public sealed record PackageManagerScanResult(
    bool IsAvailable,
    ImmutableArray<InstalledPackage> Packages,
    string? ErrorMessage)
{
    public static PackageManagerScanResult Unavailable(string errorMessage) =>
        new(false, ImmutableArray<InstalledPackage>.Empty, errorMessage);
}
