using Perch.Core.Packages;

namespace Perch.Core.Deploy;

public sealed class VscodeExtensionInstaller : IVscodeExtensionInstaller
{
    private readonly IProcessRunner _processRunner;

    public VscodeExtensionInstaller(IProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    public async Task<DeployResult> InstallAsync(string moduleName, string extensionId, bool dryRun, CancellationToken cancellationToken = default)
    {
        if (dryRun)
        {
            return new DeployResult(moduleName, "", extensionId, ResultLevel.Ok, $"Would run: code --install-extension {extensionId}");
        }

        ProcessRunResult result = await _processRunner.RunAsync("code", $"--install-extension {extensionId}", cancellationToken: cancellationToken).ConfigureAwait(false);

        if (result.ExitCode != 0)
        {
            string errorDetail = string.IsNullOrWhiteSpace(result.StandardError) ? result.StandardOutput : result.StandardError;
            return new DeployResult(moduleName, "", extensionId, ResultLevel.Error, $"code --install-extension {extensionId} failed (exit {result.ExitCode}): {errorDetail.Trim()}");
        }

        return new DeployResult(moduleName, "", extensionId, ResultLevel.Ok, $"Installed extension {extensionId}");
    }
}
