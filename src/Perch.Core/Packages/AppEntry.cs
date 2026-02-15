namespace Perch.Core.Packages;

public sealed record AppEntry(string Name, AppCategory Category, PackageManager? Source);
