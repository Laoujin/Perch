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
    private int _linkedCount;

    [ObservableProperty]
    private int _attentionCount;

    [ObservableProperty]
    private int _brokenCount;

    [ObservableProperty]
    private int _healthPercent = 100;

    [ObservableProperty]
    private string _statusMessage = "Checking status...";

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
            StatusMessage = "No config repository configured";
            return;
        }

        HasConfigRepo = true;
        IsLoading = true;
        AttentionItems.Clear();
        LinkedCount = 0;
        AttentionCount = 0;
        BrokenCount = 0;

        var progress = new Progress<StatusResult>(result =>
        {
            switch (result.Level)
            {
                case DriftLevel.Ok:
                    LinkedCount++;
                    break;
                case DriftLevel.Missing:
                case DriftLevel.Drift:
                    AttentionCount++;
                    AttentionItems.Add(new StatusItemViewModel(result));
                    break;
                case DriftLevel.Error:
                    BrokenCount++;
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

        var total = LinkedCount + AttentionCount + BrokenCount;
        HealthPercent = total > 0 ? (int)(LinkedCount * 100.0 / total) : 100;

        var issues = AttentionCount + BrokenCount;
        StatusMessage = issues == 0
            ? $"Everything looks good"
            : $"{issues} item{(issues == 1 ? "" : "s")} need attention";

        IsLoading = false;
    }
}

public sealed class StatusItemViewModel
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
