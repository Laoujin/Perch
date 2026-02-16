using Perch.Core.Modules;

namespace Perch.Core.Deploy;

public interface IDeployService
{
    Task<int> DeployAsync(string configRepoPath, bool dryRun = false, IProgress<DeployResult>? progress = null, Func<AppModule, ModuleApproval>? approvalCallback = null, CancellationToken cancellationToken = default);
}
