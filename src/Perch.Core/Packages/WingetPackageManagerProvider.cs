using System.Collections.Immutable;
using System.ComponentModel;

namespace Perch.Core.Packages;

public sealed class WingetPackageManagerProvider : IPackageManagerProvider
{
    private readonly IProcessRunner _processRunner;

    public WingetPackageManagerProvider(IProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    public PackageManager Manager => PackageManager.Winget;

    public async Task<PackageManagerScanResult> ScanInstalledAsync(CancellationToken cancellationToken = default)
    {
        ProcessRunResult result;
        try
        {
            result = await _processRunner.RunAsync("winget", "list --accept-source-agreements", cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Win32Exception)
        {
            return PackageManagerScanResult.Unavailable("winget is not installed.");
        }

        if (result.ExitCode != 0)
        {
            return PackageManagerScanResult.Unavailable($"winget list failed: {result.StandardError}");
        }

        var packages = ParseOutput(result.StandardOutput);
        return new PackageManagerScanResult(true, packages, null);
    }

    internal static ImmutableArray<InstalledPackage> ParseOutput(string output)
    {
        var packages = new List<InstalledPackage>();
        string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        int headerIndex = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].TrimStart();
            if (trimmed.Contains("Name") && trimmed.Contains("Id") && trimmed.Contains("Version"))
            {
                headerIndex = i;
                break;
            }
        }

        if (headerIndex < 0 || headerIndex + 2 >= lines.Length)
        {
            return ImmutableArray<InstalledPackage>.Empty;
        }

        string header = lines[headerIndex];
        int idStart = header.IndexOf("Id", StringComparison.Ordinal);
        int versionStart = header.IndexOf("Version", StringComparison.Ordinal);

        if (idStart < 0 || versionStart < 0)
        {
            return ImmutableArray<InstalledPackage>.Empty;
        }

        for (int i = headerIndex + 2; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string name = (line.Length > idStart ? line[..idStart] : line).Trim();
            string id = (line.Length > versionStart ? line[idStart..versionStart] : line.Length > idStart ? line[idStart..] : "").Trim();

            if (!string.IsNullOrWhiteSpace(name))
            {
                packages.Add(new InstalledPackage(name, PackageManager.Winget));
            }

            if (!string.IsNullOrWhiteSpace(id) && !string.Equals(name, id, StringComparison.OrdinalIgnoreCase))
            {
                packages.Add(new InstalledPackage(id, PackageManager.Winget));
            }
        }

        return packages.ToImmutableArray();
    }
}
