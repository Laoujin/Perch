using System.ComponentModel;
using Perch.Core.Config;
using Perch.Core.Git;
using Perch.Core.Modules;
using Spectre.Console.Cli;

namespace Perch.Cli.Commands;

public sealed class FilterCleanCommand : AsyncCommand<FilterCleanCommand.Settings>
{
    private readonly IModuleDiscoveryService _discoveryService;
    private readonly IContentFilterProcessor _filterProcessor;
    private readonly ISettingsProvider _settingsProvider;

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<module>")]
        [Description("Module name to apply clean filter for")]
        public string ModuleName { get; init; } = "";

        [CommandOption("--config-path")]
        [Description("Path to the config repository")]
        public string? ConfigPath { get; init; }
    }

    public FilterCleanCommand(IModuleDiscoveryService discoveryService, IContentFilterProcessor filterProcessor, ISettingsProvider settingsProvider)
    {
        _discoveryService = discoveryService;
        _filterProcessor = filterProcessor;
        _settingsProvider = settingsProvider;
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
            await Console.Error.WriteLineAsync("Error: No config path specified. Use --config-path or set it in settings.");
            return 2;
        }

        DiscoveryResult discovery = await _discoveryService.DiscoverAsync(configPath, cancellationToken);
        AppModule? module = discovery.Modules.FirstOrDefault(m =>
            string.Equals(m.Name, settings.ModuleName, StringComparison.OrdinalIgnoreCase));

        if (module == null)
        {
            await Console.Error.WriteLineAsync($"Error: Module '{settings.ModuleName}' not found.");
            return 1;
        }

        string content = await Console.In.ReadToEndAsync(cancellationToken);

        if (module.CleanFilter?.Rules.IsDefaultOrEmpty != false)
        {
            await Console.Out.WriteAsync(content);
            return 0;
        }

        string filtered = _filterProcessor.Apply(content, module.CleanFilter.Rules);
        await Console.Out.WriteAsync(filtered);
        return 0;
    }
}
