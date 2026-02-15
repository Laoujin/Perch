using Perch.Core.Packages;

namespace Perch.Core.Deploy;

public sealed class PsModuleInstaller : IPsModuleInstaller
{
    private readonly IProcessRunner _processRunner;

    public PsModuleInstaller(IProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    public async Task<DeployResult> InstallAsync(string moduleName, string psModuleName, bool dryRun, CancellationToken cancellationToken = default)
    {
        if (dryRun)
        {
            return new DeployResult(moduleName, "", psModuleName, ResultLevel.Ok, $"Would run: pwsh Install-Module {psModuleName}");
        }

        ProcessRunResult result = await _processRunner.RunAsync(
            "pwsh",
            $"-NoProfile -NonInteractive -Command \"Install-Module -Name '{psModuleName}' -Force -Scope CurrentUser\"",
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (result.ExitCode != 0)
        {
            string errorDetail = string.IsNullOrWhiteSpace(result.StandardError) ? result.StandardOutput : result.StandardError;
            return new DeployResult(moduleName, "", psModuleName, ResultLevel.Error, $"Install-Module {psModuleName} failed (exit {result.ExitCode}): {errorDetail.Trim()}");
        }

        return new DeployResult(moduleName, "", psModuleName, ResultLevel.Ok, $"Installed PS module {psModuleName}");
    }
}
