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
    InstallDefinition? Install);
