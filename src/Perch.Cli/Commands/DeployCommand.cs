using System.ComponentModel;
using Perch.Core.Config;
using Perch.Core.Deploy;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Perch.Cli.Commands;

public sealed class DeployCommand : AsyncCommand<DeployCommand.Settings>
{
    private readonly IDeployService _deployService;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IAnsiConsole _console;

    public sealed class Settings : CommandSettings
    {
        [CommandOption("--config-path")]
        [Description("Path to the config repository")]
        public string? ConfigPath { get; init; }
    }

    public DeployCommand(IDeployService deployService, ISettingsProvider settingsProvider, IAnsiConsole console)
    {
        _deployService = deployService;
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

        _console.MarkupLine($"[blue]Deploying from:[/] {configPath.EscapeMarkup()}");
        _console.WriteLine();

        var progress = new Progress<DeployResult>(result =>
        {
            string icon = result.Level switch
            {
                ResultLevel.Ok => "[green]OK[/]",
                ResultLevel.Warning => "[yellow]WARN[/]",
                ResultLevel.Error => "[red]FAIL[/]",
                _ => "[grey]??[/]",
            };

            _console.MarkupLine($"  {icon} [{GetColor(result.Level)}]{result.ModuleName.EscapeMarkup()}[/] {result.Message.EscapeMarkup()}");

            if (result.Level != ResultLevel.Error)
            {
                _console.MarkupLine($"       [grey]{result.TargetPath.EscapeMarkup()}[/]");
            }
        });

        int exitCode = await _deployService.DeployAsync(configPath, progress, cancellationToken);

        _console.WriteLine();
        if (exitCode == 0)
        {
            _console.MarkupLine("[green]Deploy complete.[/]");
        }
        else
        {
            _console.MarkupLine("[red]Deploy finished with errors.[/]");
        }

        return exitCode;
    }

    private static string GetColor(ResultLevel level) => level switch
    {
        ResultLevel.Ok => "green",
        ResultLevel.Warning => "yellow",
        ResultLevel.Error => "red",
        _ => "grey",
    };
}
