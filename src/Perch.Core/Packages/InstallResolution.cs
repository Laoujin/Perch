using System.Collections.Immutable;

namespace Perch.Core.Packages;

public sealed record InstallResolution(
    ImmutableArray<PackageDefinition> Packages,
    ImmutableArray<string> Errors);
