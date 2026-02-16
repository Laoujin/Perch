using System.Collections.Immutable;

namespace Perch.Core.Packages;

public sealed record InstallManifest(
    ImmutableArray<string> Apps,
    ImmutableDictionary<string, MachineInstallOverrides> Machines);

public sealed record MachineInstallOverrides(
    ImmutableArray<string> Add,
    ImmutableArray<string> Exclude);
