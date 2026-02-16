namespace Perch.Core.Packages;

public interface IInstallResolver
{
    Task<InstallResolution> ResolveAsync(
        InstallManifest manifest,
        string machineName,
        Platform currentPlatform,
        CancellationToken cancellationToken = default);
}
