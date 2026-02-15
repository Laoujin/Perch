using Perch.Core.Modules;
using Perch.Core.Symlinks;

namespace Perch.Core.Deploy;

public sealed class DeployService : IDeployService
{
    private readonly IModuleDiscoveryService _discoveryService;
    private readonly SymlinkOrchestrator _orchestrator;

    public DeployService(IModuleDiscoveryService discoveryService, SymlinkOrchestrator orchestrator)
    {
        _discoveryService = discoveryService;
        _orchestrator = orchestrator;
    }

    public async Task<int> DeployAsync(string configRepoPath, IProgress<DeployResult>? progress = null, CancellationToken cancellationToken = default)
    {
        DiscoveryResult discovery = await _discoveryService.DiscoverAsync(configRepoPath, cancellationToken).ConfigureAwait(false);

        foreach (string error in discovery.Errors)
        {
            progress?.Report(new DeployResult("discovery", "", "", ResultLevel.Error, error));
        }

        bool hasErrors = discovery.Errors.Length > 0;

        foreach (AppModule module in discovery.Modules)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (LinkEntry link in module.Links)
            {
                string expandedTarget = EnvironmentExpander.Expand(link.Target);
                string sourcePath = Path.GetFullPath(Path.Combine(module.ModulePath, link.Source));

                DeployResult result = _orchestrator.ProcessLink(module.DisplayName, sourcePath, expandedTarget, link.LinkType);
                progress?.Report(result);

                if (result.Level == ResultLevel.Error)
                {
                    hasErrors = true;
                }
            }
        }

        return hasErrors ? 1 : 0;
    }
}
