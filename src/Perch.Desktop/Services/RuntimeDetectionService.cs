using System.Collections.Immutable;
using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using Perch.Core.Catalog;
using Perch.Core.Packages;

namespace Perch.Desktop.Services;

public sealed partial class RuntimeDetectionService : IRuntimeDetectionService
{
    private static readonly TimeSpan s_timeout = TimeSpan.FromSeconds(2);

    private static readonly Dictionary<string, RuntimeCommand> s_commands = new(StringComparer.OrdinalIgnoreCase)
    {
        ["dotnet"] = new("dotnet", "--version", VersionParser.Plain),
        ["dotnet-sdk"] = new("dotnet", "--version", VersionParser.Plain),
        ["node"] = new("node", "--version", VersionParser.StripV),
        ["python"] = new("python", "--version", VersionParser.PrefixedWord),
        ["go"] = new("go", "version", VersionParser.GoVersion),
        ["ruby"] = new("ruby", "--version", VersionParser.PrefixedWord),
        ["rustup"] = new("rustup", "--version", VersionParser.PrefixedWord),
        ["rust"] = new("rustup", "--version", VersionParser.PrefixedWord),
        ["java"] = new("java", "--version", VersionParser.PrefixedWord),
    };

    private readonly IProcessRunner _processRunner;
    private readonly ILogger<RuntimeDetectionService> _logger;

    public RuntimeDetectionService(
        IProcessRunner processRunner,
        ILogger<RuntimeDetectionService> logger)
    {
        _processRunner = processRunner;
        _logger = logger;
    }

    public async Task<RuntimeDetectionResult> DetectRuntimeAsync(
        CatalogEntry entry,
        CancellationToken cancellationToken = default)
    {
        if (entry.Kind != CatalogKind.Runtime)
            return new RuntimeDetectionResult(false, null);

        var cliName = entry.Install?.Detect ?? entry.Id;

        if (!s_commands.TryGetValue(cliName, out var cmd) && !s_commands.TryGetValue(entry.Id, out cmd))
            return new RuntimeDetectionResult(false, null);

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(s_timeout);

            var result = await _processRunner.RunAsync(cmd.FileName, cmd.Arguments, null, cts.Token);

            if (result.ExitCode != 0)
                return new RuntimeDetectionResult(false, null);

            var output = !string.IsNullOrWhiteSpace(result.StandardOutput)
                ? result.StandardOutput
                : result.StandardError;

            var version = ParseVersion(output, cmd.Parser);
            return new RuntimeDetectionResult(version is not null, version);
        }
        catch (Exception ex) when (ex is Win32Exception or OperationCanceledException or InvalidOperationException)
        {
            _logger.LogDebug(ex, "Runtime detection failed for {RuntimeId}", entry.Id);
            return new RuntimeDetectionResult(false, null);
        }
    }

    public async Task<ImmutableArray<GlobalToolMatch>> DetectGlobalToolsAsync(
        string runtimeId,
        IReadOnlyList<CatalogEntry> candidates,
        CancellationToken cancellationToken = default)
    {
        if (runtimeId.Equals("dotnet-sdk", StringComparison.OrdinalIgnoreCase)
            || runtimeId.Equals("dotnet", StringComparison.OrdinalIgnoreCase))
        {
            return await DetectDotnetToolsAsync(candidates, cancellationToken);
        }

        if (runtimeId.Equals("node", StringComparison.OrdinalIgnoreCase))
        {
            return await DetectNpmPackagesAsync(candidates, cancellationToken);
        }

        return [];
    }

    private async Task<ImmutableArray<GlobalToolMatch>> DetectDotnetToolsAsync(
        IReadOnlyList<CatalogEntry> candidates,
        CancellationToken cancellationToken)
    {
        string output;
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(s_timeout);
            var result = await _processRunner.RunAsync("dotnet", "tool list -g", null, cts.Token);
            if (result.ExitCode != 0)
                return [];
            output = result.StandardOutput;
        }
        catch (Exception ex) when (ex is Win32Exception or OperationCanceledException or InvalidOperationException)
        {
            _logger.LogDebug(ex, "dotnet tool list failed");
            return [];
        }

        var installedTools = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("Package", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("---", StringComparison.Ordinal))
                continue;

            var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 1)
                installedTools.Add(parts[0]);
        }

        var builder = ImmutableArray.CreateBuilder<GlobalToolMatch>();
        foreach (var candidate in candidates)
        {
            if (candidate.Install?.DotnetTool is not null
                && installedTools.Contains(candidate.Install.DotnetTool))
            {
                builder.Add(new GlobalToolMatch(candidate.Id, candidate.Install.DotnetTool));
            }
        }

        return builder.ToImmutable();
    }

    private async Task<ImmutableArray<GlobalToolMatch>> DetectNpmPackagesAsync(
        IReadOnlyList<CatalogEntry> candidates,
        CancellationToken cancellationToken)
    {
        string output;
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(s_timeout);
            var result = await _processRunner.RunAsync("npm", "list -g --json", null, cts.Token);
            if (result.ExitCode != 0)
                return [];
            output = result.StandardOutput;
        }
        catch (Exception ex) when (ex is Win32Exception or OperationCanceledException or InvalidOperationException)
        {
            _logger.LogDebug(ex, "npm list failed");
            return [];
        }

        HashSet<string> installedPackages;
        try
        {
            using var doc = JsonDocument.Parse(output);
            if (!doc.RootElement.TryGetProperty("dependencies", out var deps))
                return [];

            installedPackages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in deps.EnumerateObject())
                installedPackages.Add(prop.Name);
        }
        catch (JsonException)
        {
            _logger.LogDebug("Failed to parse npm list output as JSON");
            return [];
        }

        var builder = ImmutableArray.CreateBuilder<GlobalToolMatch>();
        foreach (var candidate in candidates)
        {
            if (candidate.Install?.NodePackage is not null
                && installedPackages.Contains(candidate.Install.NodePackage))
            {
                builder.Add(new GlobalToolMatch(candidate.Id, candidate.Install.NodePackage));
            }
        }

        return builder.ToImmutable();
    }

    private static string? ParseVersion(string output, VersionParser parser)
    {
        var line = output.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
        if (string.IsNullOrEmpty(line))
            return null;

        return parser switch
        {
            VersionParser.Plain => line,
            VersionParser.StripV => line.StartsWith('v') ? line[1..] : line,
            VersionParser.PrefixedWord => ExtractVersion(line),
            VersionParser.GoVersion => ExtractGoVersion(line),
            _ => null,
        };
    }

    private static string? ExtractVersion(string line)
    {
        var match = VersionRegex().Match(line);
        return match.Success ? match.Value : null;
    }

    private static string? ExtractGoVersion(string line)
    {
        var match = GoVersionRegex().Match(line);
        return match.Success ? match.Groups[1].Value : null;
    }

    [GeneratedRegex(@"\d+\.\d+[\.\d]*")]
    private static partial Regex VersionRegex();

    [GeneratedRegex(@"go(\d+\.\d+[\.\d]*)")]
    private static partial Regex GoVersionRegex();

    private sealed record RuntimeCommand(string FileName, string Arguments, VersionParser Parser);

    private enum VersionParser
    {
        Plain,
        StripV,
        PrefixedWord,
        GoVersion,
    }
}
