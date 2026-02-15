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
            result = await _processRunner.RunAsync("winget", "list --source winget", cancellationToken).ConfigureAwait(false);
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

        // Find the header separator line (dashes)
        int dataStartIndex = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].TrimStart().StartsWith('-') && lines[i].Contains("--"))
            {
                dataStartIndex = i + 1;
                break;
            }
        }

        if (dataStartIndex < 0 || dataStartIndex >= lines.Length)
        {
            return ImmutableArray<InstalledPackage>.Empty;
        }

        // The header line is right before the separator
        string headerLine = lines[dataStartIndex - 2];
        string separatorLine = lines[dataStartIndex - 1];

        // Find column boundaries from separator line
        int idColumnStart = FindColumnStart(separatorLine, 1);
        if (idColumnStart < 0)
        {
            return ImmutableArray<InstalledPackage>.Empty;
        }

        for (int i = dataStartIndex; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Extract the Name column (from start to idColumnStart)
            if (line.Length > idColumnStart)
            {
                string name = line[..idColumnStart].Trim();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    packages.Add(new InstalledPackage(name, PackageManager.Winget));
                }
            }
        }

        return packages.ToImmutableArray();
    }

    private static int FindColumnStart(string separatorLine, int columnIndex)
    {
        int current = 0;
        int found = 0;
        bool inDashes = separatorLine.Length > 0 && separatorLine[0] == '-';

        for (int i = 0; i < separatorLine.Length; i++)
        {
            bool isDash = separatorLine[i] == '-';

            if (inDashes && !isDash)
            {
                inDashes = false;
            }
            else if (!inDashes && isDash)
            {
                inDashes = true;
                found++;
                if (found > columnIndex)
                {
                    return -1;
                }
                current = i;
                if (found == columnIndex)
                {
                    return current;
                }
            }
        }

        return -1;
    }
}
