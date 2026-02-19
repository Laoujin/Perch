using System.Collections.Immutable;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Perch.Core.Catalog;
using Perch.Desktop.Models;
using Perch.Desktop.Services;

namespace Perch.Desktop.ViewModels;

public sealed partial class LanguagesViewModel : GalleryViewModelBase
{
    private readonly IGalleryDetectionService _detectionService;
    private readonly IAppDetailService _detailService;
    private readonly IPendingChangesService _pendingChanges;

    private ImmutableArray<EcosystemCardModel> _allEcosystems = [];

    [ObservableProperty]
    private int _syncedCount;

    [ObservableProperty]
    private int _driftedCount;

    [ObservableProperty]
    private int _detectedCount;

    [ObservableProperty]
    private EcosystemCardModel? _selectedEcosystem;

    [ObservableProperty]
    private AppCardModel? _selectedItem;

    [ObservableProperty]
    private AppDetail? _itemDetail;

    [ObservableProperty]
    private bool _isLoadingDetail;

    public override bool ShowGrid => SelectedEcosystem is null;
    public override bool ShowDetail => SelectedEcosystem is not null;
    public bool ShowEcosystemDetail => SelectedEcosystem is not null && SelectedItem is null;
    public bool ShowItemDetail => SelectedItem is not null;
    public bool HasAlternatives => AlternativeApps.Count > 0;

    public BulkObservableCollection<EcosystemCardModel> Ecosystems { get; } = [];
    public BulkObservableCollection<AppCategoryGroup> SubCategories { get; } = [];
    public BulkObservableCollection<AppCardModel> AlternativeApps { get; } = [];

    public LanguagesViewModel(
        IGalleryDetectionService detectionService,
        IAppDetailService detailService,
        IPendingChangesService pendingChanges)
    {
        _detectionService = detectionService;
        _detailService = detailService;
        _pendingChanges = pendingChanges;
    }

    partial void OnSelectedEcosystemChanged(EcosystemCardModel? value)
    {
        OnPropertyChanged(nameof(ShowGrid));
        OnPropertyChanged(nameof(ShowDetail));
        OnPropertyChanged(nameof(ShowEcosystemDetail));
    }

    partial void OnSelectedItemChanged(AppCardModel? value)
    {
        OnPropertyChanged(nameof(ShowEcosystemDetail));
        OnPropertyChanged(nameof(ShowItemDetail));
    }

    protected override void OnSearchTextUpdated() => ApplyFilter();

    [RelayCommand]
    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            _detectionService.InvalidateCache();
            var allApps = await _detectionService.DetectAllAppsAsync(cancellationToken);
            BuildEcosystems(allApps);
            ApplyFilter();
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load languages: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void BuildEcosystems(ImmutableArray<AppCardModel> allApps)
    {
        var runtimes = allApps
            .Where(a => a.CatalogEntry.Kind == CatalogKind.Runtime
                && a.Category.Contains("Languages", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var ecosystems = runtimes.Select(runtime =>
        {
            var ecosystemName = runtime.DisplayLabel
                .Replace(" SDK", "", StringComparison.OrdinalIgnoreCase)
                .Replace(" Runtime", "", StringComparison.OrdinalIgnoreCase);

            var eco = new EcosystemCardModel(
                runtime.Id,
                ecosystemName,
                runtime.Description,
                runtime.LogoUrl);

            eco.Items = [runtime];
            eco.UpdateCounts();
            return eco;
        }).ToImmutableArray();

        _allEcosystems = ecosystems;
    }

    private void ApplyFilter()
    {
        var query = SearchText;
        Ecosystems.ReplaceAll(_allEcosystems.Where(e => e.MatchesSearch(query)));
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        SyncedCount = _allEcosystems.Sum(e => e.SyncedCount);
        DriftedCount = _allEcosystems.Sum(e => e.DriftedCount);
        DetectedCount = _allEcosystems.Sum(e => e.DetectedCount);
    }

    [RelayCommand]
    private void SelectEcosystem(EcosystemCardModel ecosystem)
    {
        SelectedEcosystem = ecosystem;
        SelectedItem = null;
        ItemDetail = null;
        AlternativeApps.ReplaceAll([]);
        OnPropertyChanged(nameof(HasAlternatives));
        RebuildSubCategories();
    }

    private void RebuildSubCategories()
    {
        if (SelectedEcosystem is null)
        {
            SubCategories.ReplaceAll([]);
            return;
        }

        var groups = SelectedEcosystem.Items
            .GroupBy(a => a.SubCategory, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
            .Select(g => new AppCategoryGroup(
                g.Key,
                new System.Collections.ObjectModel.ObservableCollection<AppCardModel>(
                    g.OrderBy(a => StatusSortOrder(a.Status))
                     .ThenByDescending(a => a.GitHubStars ?? 0)
                     .ThenBy(a => a.DisplayLabel, StringComparer.OrdinalIgnoreCase))));

        SubCategories.ReplaceAll(groups);
    }

    [RelayCommand]
    private async Task SelectItemAsync(AppCardModel card, CancellationToken cancellationToken)
    {
        SelectedItem = card;
        ItemDetail = null;
        AlternativeApps.ReplaceAll([]);
        OnPropertyChanged(nameof(HasAlternatives));
        IsLoadingDetail = true;

        try
        {
            ItemDetail = await _detailService.LoadDetailAsync(card, cancellationToken);
            OnPropertyChanged(nameof(HasAlternatives));
        }
        catch (OperationCanceledException)
        {
            return;
        }
        finally
        {
            IsLoadingDetail = false;
        }
    }

    [RelayCommand]
    private void BackToGrid()
    {
        SelectedEcosystem = null;
        SelectedItem = null;
        ItemDetail = null;
        SubCategories.ReplaceAll([]);
        AlternativeApps.ReplaceAll([]);
        OnPropertyChanged(nameof(HasAlternatives));
    }

    [RelayCommand]
    private void BackToEcosystem()
    {
        SelectedItem = null;
        ItemDetail = null;
        AlternativeApps.ReplaceAll([]);
        OnPropertyChanged(nameof(HasAlternatives));
    }

    [RelayCommand]
    private void ToggleApp(AppCardModel app)
    {
        if (!app.CanToggle)
            return;

        if (_pendingChanges.Contains(app.Id, PendingChangeKind.LinkApp))
            _pendingChanges.Remove(app.Id, PendingChangeKind.LinkApp);
        else if (_pendingChanges.Contains(app.Id, PendingChangeKind.UnlinkApp))
            _pendingChanges.Remove(app.Id, PendingChangeKind.UnlinkApp);
        else if (app.IsManaged)
            _pendingChanges.Add(new UnlinkAppChange(app));
        else
            _pendingChanges.Add(new LinkAppChange(app));
    }

    private static int StatusSortOrder(CardStatus status) => status switch
    {
        CardStatus.Drift or CardStatus.Broken => 0,
        CardStatus.Detected => 1,
        CardStatus.Linked => 2,
        _ => 3,
    };
}
