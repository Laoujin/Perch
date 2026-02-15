namespace Perch.Core.Scanner;

public interface ISystemScanner
{
    Task<SystemScanResult> ScanAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default);
}
