namespace Perch.Core.Packages;

public interface IAppScanService
{
    Task<AppScanResult> ScanAsync(string configRepoPath, CancellationToken cancellationToken = default);
}
