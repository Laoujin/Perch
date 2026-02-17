using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Perch.Core.Startup;
using Perch.Desktop.Models;

namespace Perch.Desktop.ViewModels;

public sealed partial class StartupViewModel : ViewModelBase
{
    private readonly IStartupService _startupService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public ObservableCollection<StartupCardModel> StartupItems { get; } = [];
    public ObservableCollection<StartupCardModel> FilteredItems { get; } = [];

    public StartupViewModel(IStartupService startupService)
    {
        _startupService = startupService;
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    [RelayCommand]
    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;

        try
        {
            var entries = await _startupService.GetAllAsync(cancellationToken);
            StartupItems.Clear();
            foreach (var entry in entries)
                StartupItems.Add(new StartupCardModel(entry));

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

    [RelayCommand]
    private async Task ToggleEnabledAsync(StartupCardModel card)
    {
        var newState = !card.IsEnabled;
        await _startupService.SetEnabledAsync(card.Entry, newState);
        card.IsEnabled = newState;
    }

    [RelayCommand]
    private async Task RemoveStartupItemAsync(StartupCardModel card)
    {
        await _startupService.RemoveAsync(card.Entry);
        StartupItems.Remove(card);
        FilteredItems.Remove(card);
    }

    [RelayCommand]
    private async Task AddToStartupAsync(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return;

        var name = Path.GetFileNameWithoutExtension(command);
        await _startupService.AddAsync(name, command, StartupSource.RegistryCurrentUser);
        await RefreshAsync(CancellationToken.None);
    }

    private void ApplyFilter()
    {
        FilteredItems.Clear();
        foreach (var item in StartupItems)
        {
            if (item.MatchesSearch(SearchText))
                FilteredItems.Add(item);
        }
    }
}
