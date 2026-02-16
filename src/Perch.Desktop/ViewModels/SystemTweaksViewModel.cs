using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Perch.Desktop.Models;
using Perch.Desktop.Services;

namespace Perch.Desktop.ViewModels;

public sealed partial class SystemTweaksViewModel : ViewModelBase
{
    private readonly IGalleryDetectionService _detectionService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _selectedCount;

    public ObservableCollection<TweakCardModel> Tweaks { get; } = [];

    public SystemTweaksViewModel(IGalleryDetectionService detectionService)
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
            var all = await _detectionService.DetectTweaksAsync(profiles, cancellationToken);
            Tweaks.Clear();
            foreach (var tweak in all)
                Tweaks.Add(tweak);

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

    private void ApplyFilter()
    {
        UpdateSelectedCount();
    }

    public void UpdateSelectedCount()
    {
        SelectedCount = Tweaks.Count(t => t.IsSelected);
    }

    public void ClearSelection()
    {
        foreach (var tweak in Tweaks)
            tweak.IsSelected = false;
        SelectedCount = 0;
    }
}
