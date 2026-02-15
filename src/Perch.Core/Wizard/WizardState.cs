using System.Collections.Immutable;

using Perch.Core.Scanner;

namespace Perch.Core.Wizard;

public sealed class WizardState
{
    public RepoSetupMode RepoMode { get; set; }
    public string? RepoPath { get; set; }
    public string? CloneUrl { get; set; }
    public UserProfile SelectedProfiles { get; set; }
    public string MachineName { get; set; } = Environment.MachineName;

    public SystemScanResult? ScanResult { get; set; }

    public ImmutableHashSet<string> SelectedDotfiles { get; set; } = ImmutableHashSet<string>.Empty;
    public ImmutableHashSet<string> AppsToInstall { get; set; } = ImmutableHashSet<string>.Empty;
    public ImmutableHashSet<string> ConfigsToAdopt { get; set; } = ImmutableHashSet<string>.Empty;
    public ImmutableHashSet<string> FontsToInstall { get; set; } = ImmutableHashSet<string>.Empty;
    public ImmutableHashSet<string> ExtensionsToSync { get; set; } = ImmutableHashSet<string>.Empty;
    public ImmutableHashSet<string> TweaksToApply { get; set; } = ImmutableHashSet<string>.Empty;
}
