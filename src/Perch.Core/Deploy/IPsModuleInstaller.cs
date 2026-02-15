namespace Perch.Core.Deploy;

public interface IPsModuleInstaller
{
    Task<DeployResult> InstallAsync(string moduleName, string psModuleName, bool dryRun, CancellationToken cancellationToken = default);
}
