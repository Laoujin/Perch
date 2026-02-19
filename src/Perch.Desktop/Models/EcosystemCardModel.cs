using System.Collections.Immutable;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Perch.Desktop.Models;

public partial class EcosystemCardModel : ObservableObject
{
    public string Id { get; }
    public string Name { get; }
    public string? Description { get; }
    public string? LogoUrl { get; }

    [ObservableProperty]
    private int _syncedCount;

    [ObservableProperty]
    private int _driftedCount;

    [ObservableProperty]
    private int _detectedCount;

    public ImmutableArray<AppCardModel> Items { get; set; } = [];

    public bool HasBadges => SyncedCount > 0 || DriftedCount > 0 || DetectedCount > 0;

    public EcosystemCardModel(string id, string name, string? description, string? logoUrl)
    {
        Id = id;
        Name = name;
        Description = description;
        LogoUrl = logoUrl;
    }

    public void UpdateCounts()
    {
        SyncedCount = Items.Count(i => i.Status == CardStatus.Linked);
        DriftedCount = Items.Count(i => i.Status is CardStatus.Drift or CardStatus.Broken);
        DetectedCount = Items.Count(i => i.Status == CardStatus.Detected);
        OnPropertyChanged(nameof(HasBadges));
    }

    public bool MatchesSearch(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return true;

        return Name.Contains(query, StringComparison.OrdinalIgnoreCase)
            || (Description?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
            || Items.Any(i => i.MatchesSearch(query));
    }
}
