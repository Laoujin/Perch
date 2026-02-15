using System.ComponentModel;
using Perch.Core.Config;
using Perch.Core.Status;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Perch.Cli.Commands;

public sealed class StatusCommand : AsyncCommand<StatusCommand.Settings>
{
    private readonly IStatusService _statusService;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IAnsiConsole _console;

    public sealed class Settings : CommandSettings
    {
        [CommandOption("--config-path")]
        [Description("Path to the config repository")]
        public string? ConfigPath { get; init; }

        [CommandOption("--output")]
        [Description("Output format (Pretty or Json)")]
        public OutputFormat Output { get; init; } = OutputFormat.Pretty;

        [CommandOption("--drift-only")]
        [Description("Only show items with drift, missing, or errors")]
        public bool DriftOnly { get; init; }
    }

    public StatusCommand(IStatusService statusService, ISettingsProvider settingsProvider, IAnsiConsole console)
    {
        _statusService = statusService;
        _settingsProvider = settingsProvider;
        _console = console;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        string? configPath = settings.ConfigPath;

        if (string.IsNullOrWhiteSpace(configPath))
        {
            PerchSettings perchSettings = await _settingsProvider.LoadAsync(cancellationToken);
            configPath = perchSettings.ConfigRepoPath;
        }

        if (string.IsNullOrWhiteSpace(configPath))
        {
            _console.MarkupLine("[red]Error:[/] No config path specified. Use --config-path or set it in settings.");
            return 2;
        }

        if (settings.Output == OutputFormat.Json)
        {
            return await ExecuteJsonAsync(configPath, settings.DriftOnly, cancellationToken);
        }

        return await ExecutePrettyAsync(configPath, settings.DriftOnly, cancellationToken);
    }

    private async Task<int> ExecutePrettyAsync(string configPath, bool driftOnly, CancellationToken cancellationToken)
    {
        _console.MarkupLine($"[blue]Checking status for:[/] {configPath.EscapeMarkup()}");
        _console.WriteLine();

        var progress = new SynchronousProgress<StatusResult>(result =>
        {
            if (driftOnly && result.Level == DriftLevel.Ok)
            {
                return;
            }

            string icon = result.Level switch
            {
                DriftLevel.Ok => "[green]OK[/]",
                DriftLevel.Missing => "[red]MISS[/]",
                DriftLevel.Drift => "[yellow]DRIFT[/]",
                DriftLevel.Error => "[red]FAIL[/]",
                _ => "[grey]??[/]",
            };

            _console.MarkupLine($"  {icon} [{GetColor(result.Level)}]{result.ModuleName.EscapeMarkup()}[/] {result.Message.EscapeMarkup()}");
            _console.MarkupLine($"       [grey]{result.TargetPath.EscapeMarkup()}[/]");
        });

        int exitCode = await _statusService.CheckAsync(configPath, progress, cancellationToken);

        _console.WriteLine();
        if (exitCode == 0)
        {
            _console.MarkupLine("[green]All checks passed.[/]");
        }
        else
        {
            _console.MarkupLine("[yellow]Drift detected.[/]");
        }

        return exitCode;
    }

    private async Task<int> ExecuteJsonAsync(string configPath, bool driftOnly, CancellationToken cancellationToken)
    {
        var results = new List<StatusResult>();
        var progress = new SynchronousProgress<StatusResult>(r =>
        {
            if (!driftOnly || r.Level != DriftLevel.Ok)
            {
                results.Add(r);
            }
        });

        int exitCode = await _statusService.CheckAsync(configPath, progress, cancellationToken);

        var output = new
        {
            exitCode,
            results = results.Select(r => new
            {
                moduleName = r.ModuleName,
                sourcePath = r.SourcePath,
                targetPath = r.TargetPath,
                level = r.Level.ToString(),
                message = r.Message,
            }),
        };

        string json = JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });
        _console.WriteLine(json);
        return exitCode;
    }

    private static string GetColor(DriftLevel level) => level switch
    {
        DriftLevel.Ok => "green",
        DriftLevel.Missing => "red",
        DriftLevel.Drift => "yellow",
        DriftLevel.Error => "red",
        _ => "grey",
    };

    private sealed class SynchronousProgress<T>(Action<T> handler) : IProgress<T>
    {
        public void Report(T value) => handler(value);
    }
}
