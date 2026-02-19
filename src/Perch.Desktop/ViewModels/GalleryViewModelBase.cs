using CommunityToolkit.Mvvm.ComponentModel;

namespace Perch.Desktop.ViewModels;

public abstract partial class GalleryViewModelBase : ViewModelBase
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public abstract bool ShowGrid { get; }
    public abstract bool ShowDetail { get; }

    partial void OnSearchTextChanged(string value) => OnSearchTextUpdated();

    protected virtual void OnSearchTextUpdated() { }
}
