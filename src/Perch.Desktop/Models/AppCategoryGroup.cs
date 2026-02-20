using System.Collections.ObjectModel;

namespace Perch.Desktop.Models;

public sealed class AppCategoryGroup
{
    public string SubCategory { get; }
    public ObservableCollection<AppCardModel> Apps { get; }
    public int SyncedCount { get; }
    public int DriftedCount { get; }
    public int DetectedCount { get; }
    public bool IsExpanded { get; set; }

    public AppCategoryGroup(string subCategory, ObservableCollection<AppCardModel> apps)
    {
        SubCategory = subCategory;
        Apps = apps;

        SyncedCount = apps.Count(a => a.Status == CardStatus.Synced);
        DriftedCount = apps.Count(a => a.Status == CardStatus.Drifted);
        DetectedCount = apps.Count(a => a.Status == CardStatus.Detected);

        var hasAttentionItems = DriftedCount > 0 || DetectedCount > 0;
        IsExpanded = hasAttentionItems || apps.Count <= 5;
    }
}
