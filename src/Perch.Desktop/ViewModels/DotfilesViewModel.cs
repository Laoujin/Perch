using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Perch.Desktop.Models;
using Perch.Desktop.Services;

namespace Perch.Desktop.ViewModels;

public sealed partial class DotfilesViewModel : ViewModelBase
{
    private readonly IGalleryDetectionService _detectionService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _linkedCount;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _selectedCount;

    public ObservableCollection<DotfileCardModel> Dotfiles { get; } = [];

    public DotfilesViewModel(IGalleryDetectionService detectionService)
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
            var all = await _detectionService.DetectDotfilesAsync(cancellationToken);
            Dotfiles.Clear();
            foreach (var df in all)
                Dotfiles.Add(df);

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
        LinkedCount = Dotfiles.Count(d => d.Status == CardStatus.Linked);
        TotalCount = Dotfiles.Count;
        UpdateSelectedCount();
    }

    public void UpdateSelectedCount()
    {
        SelectedCount = Dotfiles.Count(d => d.IsSelected);
    }

    public void ClearSelection()
    {
        foreach (var df in Dotfiles)
            df.IsSelected = false;
        SelectedCount = 0;
    }
}
