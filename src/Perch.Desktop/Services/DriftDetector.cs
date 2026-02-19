using System.Security;

using Microsoft.Extensions.Logging;

namespace Perch.Desktop.Services;

internal readonly record struct DriftCheckResult(bool IsDrift, string? Error = null);

internal static class DriftDetector
{
    public static DriftCheckResult Check(string resolvedPath, string configRepoPath, ILogger logger)
    {
        try
        {
            var linkTarget = new FileInfo(resolvedPath).LinkTarget;
            if (linkTarget is null)
                return new DriftCheckResult(false);

            var resolvedTarget = Path.GetFullPath(linkTarget, Path.GetDirectoryName(resolvedPath)!);
            var resolvedConfig = Path.GetFullPath(configRepoPath);
            var isDrift = !resolvedTarget.StartsWith(resolvedConfig, StringComparison.OrdinalIgnoreCase);
            return new DriftCheckResult(isDrift);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException)
        {
            logger.LogWarning(ex, "Drift check failed for {Path}", resolvedPath);
            return new DriftCheckResult(false, ex.Message);
        }
    }
}
