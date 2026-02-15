using System.ComponentModel;
using Perch.Core.Config;
using Perch.Core.Packages;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Perch.Cli.Commands;

public sealed class AppsCommand : AsyncCommand<AppsCommand.Settings>
{
    private readonly IAppScanService _appScanService;
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

        [CommandOption("--unmanaged")]
        [Description("Show only installed apps without a config module")]
        public bool Unmanaged { get; init; }
    }

    public AppsCommand(IAppScanService appScanService, ISettingsProvider settingsProvider, IAnsiConsole console)
    {
        _appScanService = appScanService;
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
            return await ExecuteJsonAsync(configPath, settings.Unmanaged, cancellationToken);
        }

        return await ExecutePrettyAsync(configPath, settings.Unmanaged, cancellationToken);
    }

    private async Task<int> ExecutePrettyAsync(string configPath, bool unmanagedOnly, CancellationToken cancellationToken)
    {
        var result = await _appScanService.ScanAsync(configPath, cancellationToken);
        var entries = unmanagedOnly
            ? result.Entries.Where(e => e.Category == AppCategory.InstalledNoModule).ToList()
            : result.Entries.ToList();

        foreach (string warning in result.Warnings)
        {
            _console.MarkupLine($"[yellow]Warning:[/] {warning.EscapeMarkup()}");
        }

        if (entries.Count == 0)
        {
            if (unmanagedOnly)
            {
                _console.MarkupLine("[green]All installed apps are managed.[/]");
            }
            else
            {
                _console.MarkupLine("[grey]No apps found.[/]");
            }
            return 0;
        }

        var table = new Table();
        table.AddColumn("Name");
        table.AddColumn("Category");
        table.AddColumn("Source");

        foreach (var entry in entries)
        {
            string categoryDisplay = entry.Category switch
            {
                AppCategory.Managed => "[green]Managed[/]",
                AppCategory.InstalledNoModule => "[yellow]No Module[/]",
                AppCategory.DefinedNotInstalled => "[red]Not Installed[/]",
                _ => "[grey]Unknown[/]",
            };

            table.AddRow(
                entry.Name.EscapeMarkup(),
                categoryDisplay,
                entry.Source?.ToString() ?? "-");
        }

        _console.Write(table);
        return 0;
    }

    private async Task<int> ExecuteJsonAsync(string configPath, bool unmanagedOnly, CancellationToken cancellationToken)
    {
        var result = await _appScanService.ScanAsync(configPath, cancellationToken);
        var entries = unmanagedOnly
            ? result.Entries.Where(e => e.Category == AppCategory.InstalledNoModule)
            : result.Entries.AsEnumerable();

        var output = new
        {
            entries = entries.Select(e => new
            {
                name = e.Name,
                category = e.Category.ToString(),
                source = e.Source?.ToString(),
            }),
            warnings = result.Warnings.AsEnumerable(),
        };

        string json = JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });
        _console.WriteLine(json);
        return 0;
    }
}
