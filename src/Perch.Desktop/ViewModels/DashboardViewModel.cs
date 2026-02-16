using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Perch.Core.Config;
using Perch.Core.Status;

namespace Perch.Desktop.ViewModels;

public sealed partial class DashboardViewModel : ViewModelBase
{
    private readonly IStatusService _statusService;
    private readonly ISettingsProvider _settingsProvider;

    [ObservableProperty]
    private int _okCount;

    [ObservableProperty]
    private int _missingCount;

    [ObservableProperty]
    private int _driftCount;

    [ObservableProperty]
    private int _errorCount;

    [ObservableProperty]
    private string _healthMessage = "Checking status...";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasConfigRepo = true;

    public ObservableCollection<StatusItemViewModel> AttentionItems { get; } = [];

    public DashboardViewModel(IStatusService statusService, ISettingsProvider settingsProvider)
    {
        _statusService = statusService;
        _settingsProvider = settingsProvider;
    }

    [RelayCommand]
    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        var settings = await _settingsProvider.LoadAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(settings.ConfigRepoPath))
        {
            HasConfigRepo = false;
            HealthMessage = "No config repository configured. Go to Settings to set one up.";
            return;
        }

        HasConfigRepo = true;
        IsLoading = true;
        AttentionItems.Clear();
        OkCount = 0;
        MissingCount = 0;
        DriftCount = 0;
        ErrorCount = 0;

        var results = new List<StatusResult>();
        var progress = new Progress<StatusResult>(result =>
        {
            results.Add(result);
            switch (result.Level)
            {
                case DriftLevel.Ok:
                    OkCount++;
                    break;
                case DriftLevel.Missing:
                    MissingCount++;
                    AttentionItems.Add(new StatusItemViewModel(result));
                    break;
                case DriftLevel.Drift:
                    DriftCount++;
                    AttentionItems.Add(new StatusItemViewModel(result));
                    break;
                case DriftLevel.Error:
                    ErrorCount++;
                    AttentionItems.Add(new StatusItemViewModel(result));
                    break;
            }
        });

        try
        {
            await _statusService.CheckAsync(settings.ConfigRepoPath, progress, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        var issues = MissingCount + DriftCount + ErrorCount;
        HealthMessage = issues == 0
            ? $"Everything looks good. {OkCount} configs linked."
            : $"{issues} item{(issues == 1 ? "" : "s")} need attention.";

        IsLoading = false;
    }
}

public sealed partial class StatusItemViewModel : ObservableObject
{
    public string ModuleName { get; }
    public string SourcePath { get; }
    public string TargetPath { get; }
    public DriftLevel Level { get; }
    public string Message { get; }
    public StatusCategory Category { get; }

    public string LevelDisplay => Level switch
    {
        DriftLevel.Missing => "Missing",
        DriftLevel.Drift => "Drift",
        DriftLevel.Error => "Error",
        _ => "OK",
    };

    public StatusItemViewModel(StatusResult result)
    {
        ModuleName = result.ModuleName;
        SourcePath = result.SourcePath;
        TargetPath = result.TargetPath;
        Level = result.Level;
        Message = result.Message;
        Category = result.Category;
    }
}
