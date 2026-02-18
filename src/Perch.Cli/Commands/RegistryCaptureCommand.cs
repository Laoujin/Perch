using System.ComponentModel;
using Perch.Core.Config;
using Perch.Core.Modules;
using Perch.Core.Registry;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Perch.Cli.Commands;

public sealed class RegistryCaptureCommand : AsyncCommand<RegistryCaptureCommand.Settings>
{
    private readonly IModuleDiscoveryService _discoveryService;
    private readonly IRegistryCaptureService _captureService;
    private readonly ICapturedRegistryStore _capturedStore;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IAnsiConsole _console;

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<module>")]
        [Description("Module name to capture registry state for")]
        public string ModuleName { get; init; } = "";

        [CommandOption("--config-path")]
        [Description("Path to the config repository")]
        public string? ConfigPath { get; init; }
    }

    public RegistryCaptureCommand(IModuleDiscoveryService discoveryService, IRegistryCaptureService captureService, ICapturedRegistryStore capturedStore, ISettingsProvider settingsProvider, IAnsiConsole console)
    {
        _discoveryService = discoveryService;
        _captureService = captureService;
        _capturedStore = capturedStore;
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
            _console.MarkupLine("[red]Error: No config path specified. Use --config-path or set it in settings.[/]");
            return 2;
        }

        DiscoveryResult discovery = await _discoveryService.DiscoverAsync(configPath, cancellationToken);
        AppModule? module = discovery.Modules.FirstOrDefault(m =>
            string.Equals(m.Name, settings.ModuleName, StringComparison.OrdinalIgnoreCase));

        if (module == null)
        {
            _console.MarkupLine($"[red]Error: Module '{settings.ModuleName}' not found.[/]");
            return 1;
        }

        if (module.Registry.IsDefaultOrEmpty)
        {
            _console.MarkupLine($"[yellow]Module '{settings.ModuleName}' has no registry entries defined.[/]");
            return 0;
        }

        RegistryCaptureResult result = _captureService.Capture(module.Registry);

        foreach (string warning in result.Warnings)
        {
            _console.MarkupLine($"[yellow]Warning: {warning}[/]");
        }

        if (result.Entries.IsDefaultOrEmpty)
        {
            _console.MarkupLine("[yellow]No registry values could be captured.[/]");
            return 0;
        }

        var capturedData = await _capturedStore.LoadAsync(cancellationToken);
        foreach (var entry in result.Entries)
        {
            string storeKey = $@"{entry.Key}\{entry.Name}";
            capturedData.Entries[storeKey] = new CapturedRegistryEntry
            {
                Value = entry.Value?.ToString(),
                Kind = entry.Kind,
                CapturedAt = DateTime.UtcNow,
            };
        }
        await _capturedStore.SaveAsync(capturedData, cancellationToken);

        _console.MarkupLine($"[green]Captured {result.Entries.Length} registry value(s) for '{settings.ModuleName}':[/]");
        foreach (var entry in result.Entries)
        {
            _console.MarkupLine($"  {entry.Key}\\{entry.Name} = {entry.Value} ({entry.Kind})");
        }

        return 0;
    }
}
