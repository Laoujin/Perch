using System.Collections.Immutable;

namespace Perch.Core.Catalog;

public sealed record FontCatalogEntry(
    string Id,
    string Name,
    string Category,
    ImmutableArray<string> Tags,
    string? Description,
    string? Logo,
    string? PreviewText,
    InstallDefinition? Install,
    ImmutableArray<string> Profiles = default,
    string? License = null,
    int? Sort = null);
