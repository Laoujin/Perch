using System.Collections.Immutable;

namespace Perch.Core.Catalog;

public sealed record CatalogIndex(
    ImmutableArray<CatalogIndexEntry> Apps,
    ImmutableArray<CatalogIndexEntry> Fonts,
    ImmutableArray<CatalogIndexEntry> Tweaks);

public sealed record CatalogIndexEntry(
    string Id,
    string Name,
    string Category,
    ImmutableArray<string> Tags,
    CatalogKind Kind = CatalogKind.App,
    ImmutableArray<string> Profiles = default,
    bool Hidden = false);
