namespace Perch.Core.Deploy;

public interface IDeployService
{
    Task<int> DeployAsync(string configRepoPath, IProgress<DeployResult>? progress = null, CancellationToken cancellationToken = default);
}
