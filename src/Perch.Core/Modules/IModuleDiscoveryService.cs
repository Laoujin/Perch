namespace Perch.Core.Modules;

public interface IModuleDiscoveryService
{
    Task<DiscoveryResult> DiscoverAsync(string configRepoPath, CancellationToken cancellationToken = default);
}
