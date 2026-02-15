using System.Collections.Immutable;

using Perch.Core.Packages;

namespace Perch.Core.Scanner;

public sealed record SystemScanResult(
    ImmutableArray<DetectedDotfile> Dotfiles,
    ImmutableArray<InstalledPackage> InstalledPackages,
    ImmutableArray<DetectedFont> InstalledFonts,
    ImmutableArray<DetectedVsCodeExtension> VsCodeExtensions,
    ImmutableArray<string> GlobalPackages,
    bool VsCodeDetected,
    ImmutableArray<string> Warnings);
