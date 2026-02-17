using CommunityToolkit.Mvvm.ComponentModel;

using Perch.Core.Scanner;

namespace Perch.Desktop.Models;

public partial class DotfileCardModel : ObservableObject
{
    public string Name { get; }
    public string FullPath { get; }
    public string Group { get; }
    public long SizeBytes { get; }
    public bool IsSymlink { get; }
    public bool Exists { get; }

    [ObservableProperty]
    private CardStatus _status;

    [ObservableProperty]
    private bool _isSelected;

    public DotfileCardModel(DetectedDotfile dotfile)
    {
        Name = dotfile.Name;
        FullPath = dotfile.FullPath;
        Group = dotfile.Group;
        SizeBytes = dotfile.SizeBytes;
        IsSymlink = dotfile.IsSymlink;
        Exists = dotfile.Exists;
        Status = !dotfile.Exists ? CardStatus.NotInstalled
               : dotfile.IsSymlink ? CardStatus.Linked
               : CardStatus.Detected;
    }

    public bool MatchesSearch(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return true;

        return Name.Contains(query, StringComparison.OrdinalIgnoreCase)
            || Group.Contains(query, StringComparison.OrdinalIgnoreCase)
            || FullPath.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}
