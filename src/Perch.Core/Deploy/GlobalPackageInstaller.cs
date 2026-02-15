using Perch.Core.Modules;
using Perch.Core.Packages;

namespace Perch.Core.Deploy;

public sealed class GlobalPackageInstaller : IGlobalPackageInstaller
{
    private readonly IProcessRunner _processRunner;

    public GlobalPackageInstaller(IProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    public async Task<DeployResult> InstallAsync(string moduleName, GlobalPackageManager manager, string packageName, bool dryRun, CancellationToken cancellationToken = default)
    {
        string command = manager switch
        {
            GlobalPackageManager.Bun => "bun",
            _ => "npm",
        };

        string arguments = manager switch
        {
            GlobalPackageManager.Bun => $"add -g {packageName}",
            _ => $"install -g {packageName}",
        };

        if (dryRun)
        {
            return new DeployResult(moduleName, "", packageName, ResultLevel.Ok, $"Would run: {command} {arguments}");
        }

        ProcessRunResult result = await _processRunner.RunAsync(command, arguments, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (result.ExitCode != 0)
        {
            string errorDetail = string.IsNullOrWhiteSpace(result.StandardError) ? result.StandardOutput : result.StandardError;
            return new DeployResult(moduleName, "", packageName, ResultLevel.Error, $"{command} {arguments} failed (exit {result.ExitCode}): {errorDetail.Trim()}");
        }

        return new DeployResult(moduleName, "", packageName, ResultLevel.Ok, $"Installed {packageName} via {command}");
    }
}
