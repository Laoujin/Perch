using Perch.Core.Modules;

namespace Perch.Core.Deploy;

public interface IGlobalPackageInstaller
{
    Task<DeployResult> InstallAsync(string moduleName, GlobalPackageManager manager, string packageName, bool dryRun, CancellationToken cancellationToken = default);
}
