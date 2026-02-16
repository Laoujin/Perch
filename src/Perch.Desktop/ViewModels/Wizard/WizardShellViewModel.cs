using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Perch.Core.Config;
using Perch.Core.Deploy;
using Perch.Core.Modules;
using Perch.Core.Packages;

namespace Perch.Desktop.ViewModels.Wizard;

public sealed partial class WizardShellViewModel : ViewModelBase
{
    private readonly IDeployService _deployService;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IModuleDiscoveryService _discoveryService;
    private readonly IAppScanService _appScanService;

    [ObservableProperty]
    private int _currentStepIndex;

    [ObservableProperty]
    private bool _isDeploying;

    [ObservableProperty]
    private bool _isComplete;

    [ObservableProperty]
    private bool _hasErrors;

    [ObservableProperty]
    private string _deployStatusMessage = string.Empty;

    [ObservableProperty]
    private int _deployedCount;

    [ObservableProperty]
    private int _errorCount;

    [ObservableProperty]
    private bool _isLoadingModules;

    public ObservableCollection<string> StepNames { get; } = [];

    public ProfileSelectionViewModel ProfileSelection { get; }

    public ObservableCollection<WizardModuleViewModel> DotfileModules { get; } = [];
    public ObservableCollection<WizardAppViewModel> Apps { get; } = [];
    public ObservableCollection<WizardTweakModuleViewModel> TweakModules { get; } = [];
    public ObservableCollection<DeployResultItemViewModel> DeployResults { get; } = [];

    public WizardShellViewModel(
        IDeployService deployService,
        ISettingsProvider settingsProvider,
        IModuleDiscoveryService discoveryService,
        IAppScanService appScanService)
    {
        _deployService = deployService;
        _settingsProvider = settingsProvider;
        _discoveryService = discoveryService;
        _appScanService = appScanService;

        ProfileSelection = new ProfileSelectionViewModel();

        StepNames.Add("Welcome");
        StepNames.Add("Dotfiles");
        StepNames.Add("Apps");
        StepNames.Add("System Tweaks");
        StepNames.Add("Review");
        StepNames.Add("Deploy");
    }

    public bool CanGoBack => CurrentStepIndex > 0 && !IsDeploying && !IsComplete;
    public bool CanGoNext => CurrentStepIndex < StepNames.Count - 1 && !IsDeploying && !IsComplete;
    public bool ShowDeploy => CurrentStepIndex == StepNames.Count - 1 && !IsDeploying && !IsComplete;

    partial void OnCurrentStepIndexChanged(int value)
    {
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(ShowDeploy));
    }

    [RelayCommand]
    private void GoBack()
    {
        if (CanGoBack)
            CurrentStepIndex--;
    }

    [RelayCommand]
    private async Task GoNextAsync(CancellationToken cancellationToken)
    {
        if (!CanGoNext)
            return;

        // Load data when leaving Welcome step
        if (CurrentStepIndex == 0 && DotfileModules.Count == 0)
            await LoadModulesAndAppsAsync(cancellationToken);

        CurrentStepIndex++;
    }

    private async Task LoadModulesAndAppsAsync(CancellationToken cancellationToken)
    {
        var settings = await _settingsProvider.LoadAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(settings.ConfigRepoPath))
            return;

        IsLoadingModules = true;

        try
        {
            var discoveryTask = _discoveryService.DiscoverAsync(settings.ConfigRepoPath, cancellationToken);
            var appScanTask = _appScanService.ScanAsync(settings.ConfigRepoPath, cancellationToken);

            await Task.WhenAll(discoveryTask, appScanTask);

            var discovery = discoveryTask.Result;
            var appScan = appScanTask.Result;

            DotfileModules.Clear();
            TweakModules.Clear();
            Apps.Clear();

            foreach (var module in discovery.Modules.OrderBy(m => m.DisplayName))
            {
                if (module.Links.Length > 0)
                {
                    DotfileModules.Add(new WizardModuleViewModel(module.Name, module.DisplayName, module.Enabled)
                    {
                        LinkCount = module.Links.Length,
                    });
                }

                if (module.Registry.Length > 0)
                {
                    TweakModules.Add(new WizardTweakModuleViewModel(module.Name, module.DisplayName, module.Enabled)
                    {
                        EntryCount = module.Registry.Length,
                    });
                }
            }

            foreach (var entry in appScan.Entries.OrderBy(e => e.Name))
            {
                Apps.Add(new WizardAppViewModel(entry.Name, entry.Category, entry.Source?.ToString()));
            }
        }
        catch (OperationCanceledException)
        {
            // User cancelled
        }
        finally
        {
            IsLoadingModules = false;
        }
    }

    [RelayCommand]
    private async Task DeployAsync(CancellationToken cancellationToken)
    {
        var settings = await _settingsProvider.LoadAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(settings.ConfigRepoPath))
        {
            DeployStatusMessage = "No config repository configured.";
            HasErrors = true;
            IsComplete = true;
            return;
        }

        IsDeploying = true;
        DeployResults.Clear();
        DeployedCount = 0;
        ErrorCount = 0;
        DeployStatusMessage = "Deploying...";

        var selectedModules = DotfileModules
            .Where(m => m.IsSelected)
            .Select(m => m.Name)
            .Concat(TweakModules.Where(m => m.IsSelected).Select(m => m.Name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var progress = new Progress<DeployResult>(result =>
        {
            if (result.EventType == DeployEventType.Action)
            {
                DeployResults.Add(new DeployResultItemViewModel(result));
                if (result.Level == ResultLevel.Error)
                    ErrorCount++;
                else
                    DeployedCount++;

                DeployStatusMessage = $"Deployed {DeployedCount} items...";
            }
        });

        try
        {
            var exitCode = await _deployService.DeployAsync(
                settings.ConfigRepoPath,
                new DeployOptions
                {
                    Progress = progress,
                    BeforeModule = (module, _) =>
                    {
                        var action = selectedModules.Contains(module.Name)
                            ? ModuleAction.Proceed
                            : ModuleAction.Skip;
                        return Task.FromResult(action);
                    },
                },
                cancellationToken);

            HasErrors = exitCode != 0 || ErrorCount > 0;
        }
        catch (OperationCanceledException)
        {
            DeployStatusMessage = "Deploy cancelled.";
            HasErrors = true;
        }
        catch (Exception ex)
        {
            DeployStatusMessage = $"Deploy failed: {ex.Message}";
            HasErrors = true;
        }

        IsDeploying = false;
        IsComplete = true;

        DeployStatusMessage = HasErrors
            ? $"Completed with {ErrorCount} error{(ErrorCount == 1 ? "" : "s")}. {DeployedCount} items deployed."
            : $"All done! {DeployedCount} configs linked successfully.";

        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(ShowDeploy));
    }

    public int SelectedDotfileCount => DotfileModules.Count(m => m.IsSelected);
    public int SelectedTweakCount => TweakModules.Count(m => m.IsSelected);
    public int TotalSelectedCount => SelectedDotfileCount + SelectedTweakCount;

    public void NotifySelectionCounts()
    {
        OnPropertyChanged(nameof(SelectedDotfileCount));
        OnPropertyChanged(nameof(SelectedTweakCount));
        OnPropertyChanged(nameof(TotalSelectedCount));
    }
}

public sealed partial class ProfileSelectionViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isDeveloper = true;

    [ObservableProperty]
    private bool _isPowerUser;

    [ObservableProperty]
    private bool _isGamer;

    [ObservableProperty]
    private bool _isCasual;

    [ObservableProperty]
    private bool _isCreative;
}

public sealed partial class WizardModuleViewModel : ObservableObject
{
    public string Name { get; }
    public string DisplayName { get; }

    [ObservableProperty]
    private bool _isSelected;

    public int LinkCount { get; init; }

    public string Description => $"{LinkCount} config file{(LinkCount == 1 ? "" : "s")}";

    public WizardModuleViewModel(string name, string displayName, bool defaultEnabled)
    {
        Name = name;
        DisplayName = displayName;
        IsSelected = defaultEnabled;
    }
}

public sealed partial class WizardAppViewModel : ObservableObject
{
    public string Name { get; }
    public AppCategory Category { get; }
    public string? Source { get; }

    [ObservableProperty]
    private bool _isSelected;

    public string CategoryDisplay => Category switch
    {
        AppCategory.Managed => "Managed",
        AppCategory.InstalledNoModule => "Installed",
        AppCategory.DefinedNotInstalled => "Not installed",
        _ => "",
    };

    public WizardAppViewModel(string name, AppCategory category, string? source)
    {
        Name = name;
        Category = category;
        Source = source;
        IsSelected = category == AppCategory.Managed;
    }
}

public sealed partial class WizardTweakModuleViewModel : ObservableObject
{
    public string Name { get; }
    public string DisplayName { get; }

    [ObservableProperty]
    private bool _isSelected;

    public int EntryCount { get; init; }

    public string Description => $"{EntryCount} registry entr{(EntryCount == 1 ? "y" : "ies")}";

    public WizardTweakModuleViewModel(string name, string displayName, bool defaultEnabled)
    {
        Name = name;
        DisplayName = displayName;
        IsSelected = defaultEnabled;
    }
}

public sealed class DeployResultItemViewModel
{
    public string ModuleName { get; }
    public string Message { get; }
    public ResultLevel Level { get; }
    public string SourcePath { get; }
    public string TargetPath { get; }

    public string LevelDisplay => Level switch
    {
        ResultLevel.Ok => "OK",
        ResultLevel.Warning => "Warning",
        ResultLevel.Error => "Error",
        _ => "",
    };

    public DeployResultItemViewModel(DeployResult result)
    {
        ModuleName = result.ModuleName;
        Message = result.Message;
        Level = result.Level;
        SourcePath = result.SourcePath;
        TargetPath = result.TargetPath;
    }
}
