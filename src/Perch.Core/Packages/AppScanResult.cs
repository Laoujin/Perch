using System.Collections.Immutable;

namespace Perch.Core.Packages;

public sealed record AppScanResult(
    ImmutableArray<AppEntry> Entries,
    ImmutableArray<string> Warnings);
