using System.Collections.Immutable;

using CommunityToolkit.Mvvm.ComponentModel;

using Perch.Core.Catalog;

namespace Perch.Desktop.Models;

public sealed record DotfileFileStatus(
    string FileName,
    string FullPath,
    bool Exists,
    bool IsSymlink,
    CardStatus Status);

public partial class DotfileGroupCardModel : ObservableObject
{
    public string Id { get; }
    public string Name { get; }
    public string DisplayLabel { get; }
    public string Category { get; }
    public CatalogEntry CatalogEntry { get; }
    public ImmutableArray<DotfileFileStatus> Files { get; }
    public int FileCount => Files.Length;
    public string FileCountLabel => FileCount == 1 ? "1 file" : $"{FileCount} files";

    [ObservableProperty]
    private CardStatus _status;

    [ObservableProperty]
    private bool _isSelected;

    public DotfileGroupCardModel(
        CatalogEntry catalogEntry,
        ImmutableArray<DotfileFileStatus> files,
        CardStatus status)
    {
        CatalogEntry = catalogEntry;
        Id = catalogEntry.Id;
        Name = catalogEntry.Name;
        DisplayLabel = catalogEntry.DisplayName ?? catalogEntry.Name;
        Category = catalogEntry.Category;
        Files = files;
        Status = status;
    }

    public bool MatchesSearch(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return true;

        if (Name.Contains(query, StringComparison.OrdinalIgnoreCase)
            || Category.Contains(query, StringComparison.OrdinalIgnoreCase)
            || DisplayLabel.Contains(query, StringComparison.OrdinalIgnoreCase))
            return true;

        foreach (var file in Files)
        {
            if (file.FileName.Contains(query, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
