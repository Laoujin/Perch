using Perch.Core.Packages;
using Perch.Core.Scanner;

namespace Perch.Core.Deploy;

public sealed class VscodeExtensionInstaller : IVscodeExtensionInstaller
{
    private readonly IProcessRunner _processRunner;
    private readonly IVsCodeService _vsCodeService;

    public VscodeExtensionInstaller(IProcessRunner processRunner, IVsCodeService vsCodeService)
    {
        _processRunner = processRunner;
        _vsCodeService = vsCodeService;
    }

    public async Task<DeployResult> InstallAsync(string moduleName, string extensionId, bool dryRun, CancellationToken cancellationToken = default)
    {
        string? codePath = _vsCodeService.GetCodePath();
        if (codePath == null)
        {
            return new DeployResult(moduleName, "", extensionId, ResultLevel.Error,
                "VS Code not found. Install VS Code or add 'code' to PATH.");
        }

        if (dryRun)
        {
            return new DeployResult(moduleName, "", extensionId, ResultLevel.Ok, $"Would run: code --install-extension {extensionId}");
        }

        ProcessRunResult result = await _processRunner.RunAsync(codePath, $"--install-extension {extensionId}", cancellationToken: cancellationToken).ConfigureAwait(false);

        if (result.ExitCode != 0)
        {
            string errorDetail = string.IsNullOrWhiteSpace(result.StandardError) ? result.StandardOutput : result.StandardError;
            return new DeployResult(moduleName, "", extensionId, ResultLevel.Error, $"code --install-extension {extensionId} failed (exit {result.ExitCode}): {errorDetail.Trim()}");
        }

        return new DeployResult(moduleName, "", extensionId, ResultLevel.Ok, $"Installed extension {extensionId}");
    }
}
