using System.Collections.Immutable;

namespace Perch.Core.Packages;

public interface IInstallResolver
{
    Task<InstallResolution> ResolveAsync(
        InstallManifest manifest,
        string machineName,
        Platform currentPlatform,
        CancellationToken cancellationToken = default);

    Task<InstallResolution> ResolveFontsAsync(
        ImmutableArray<string> fontIds,
        Platform currentPlatform,
        CancellationToken cancellationToken = default);
}
