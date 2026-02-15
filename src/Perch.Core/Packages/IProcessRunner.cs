namespace Perch.Core.Packages;

public interface IProcessRunner
{
    Task<ProcessRunResult> RunAsync(string fileName, string arguments, CancellationToken cancellationToken = default);
}

public sealed record ProcessRunResult(int ExitCode, string StandardOutput, string StandardError);
