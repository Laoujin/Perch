using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Perch.Desktop.Models;
using Perch.Desktop.Services;

namespace Perch.Desktop.ViewModels;

public sealed partial class AppsViewModel : ViewModelBase
{
    private readonly IGalleryDetectionService _detectionService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _selectedCount;

    public ObservableCollection<AppCardModel> YourApps { get; } = [];
    public ObservableCollection<AppCardModel> SuggestedApps { get; } = [];
    public ObservableCollection<AppCardModel> OtherApps { get; } = [];

    private GalleryDetectionResult? _lastResult;

    public AppsViewModel(IGalleryDetectionService detectionService)
    {
        _detectionService = detectionService;
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    [RelayCommand]
    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;

        try
        {
            var profiles = new HashSet<UserProfile> { UserProfile.Developer, UserProfile.PowerUser };
            _lastResult = await _detectionService.DetectAppsAsync(profiles, cancellationToken);
            ApplyFilter();
        }
        catch (OperationCanceledException)
        {
            return;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadFromDetectionResultAsync(GalleryDetectionResult result)
    {
        _lastResult = result;
        ApplyFilter();
        await Task.CompletedTask;
    }

    private void ApplyFilter()
    {
        if (_lastResult is null) return;

        YourApps.Clear();
        SuggestedApps.Clear();
        OtherApps.Clear();

        foreach (var app in _lastResult.YourApps)
        {
            if (app.MatchesSearch(SearchText))
                YourApps.Add(app);
        }

        foreach (var app in _lastResult.Suggested)
        {
            if (app.MatchesSearch(SearchText))
                SuggestedApps.Add(app);
        }

        foreach (var app in _lastResult.OtherApps)
        {
            if (app.MatchesSearch(SearchText))
                OtherApps.Add(app);
        }

        UpdateSelectedCount();
    }

    public void UpdateSelectedCount()
    {
        SelectedCount = YourApps.Count(a => a.IsSelected)
            + SuggestedApps.Count(a => a.IsSelected)
            + OtherApps.Count(a => a.IsSelected);
    }

    public void ClearSelection()
    {
        foreach (var app in YourApps.Concat(SuggestedApps).Concat(OtherApps))
            app.IsSelected = false;
        SelectedCount = 0;
    }
}
