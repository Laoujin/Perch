using CommunityToolkit.Mvvm.ComponentModel;

using Perch.Core.Config;
using Perch.Desktop.Models;

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

    protected static async Task<HashSet<UserProfile>> LoadProfilesAsync(
        ISettingsProvider settingsProvider, CancellationToken cancellationToken)
    {
        var settings = await settingsProvider.LoadAsync(cancellationToken);
        var profiles = new HashSet<UserProfile>();
        if (settings.Profiles is { Count: > 0 })
        {
            foreach (var name in settings.Profiles)
            {
                if (Enum.TryParse<UserProfile>(name, ignoreCase: true, out var profile))
                    profiles.Add(profile);
            }
        }

        if (profiles.Count == 0)
            profiles = [UserProfile.Developer, UserProfile.PowerUser];

        return profiles;
    }
}
