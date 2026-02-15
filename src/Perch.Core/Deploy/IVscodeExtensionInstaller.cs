namespace Perch.Core.Deploy;

public interface IVscodeExtensionInstaller
{
    Task<DeployResult> InstallAsync(string moduleName, string extensionId, bool dryRun, CancellationToken cancellationToken = default);
}
