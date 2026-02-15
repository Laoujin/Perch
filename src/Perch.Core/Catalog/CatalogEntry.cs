using System.Collections.Immutable;

namespace Perch.Core.Catalog;

public sealed record CatalogEntry(
    string Id,
    string Name,
    string? DisplayName,
    string Category,
    ImmutableArray<string> Tags,
    string? Description,
    string? Logo,
    CatalogLinks? Links,
    InstallDefinition? Install,
    CatalogConfigDefinition? Config,
    CatalogExtensions? Extensions);

public sealed record CatalogLinks(string? Website, string? Docs, string? GitHub);

public sealed record InstallDefinition(string? Winget, string? Choco);

public sealed record CatalogConfigDefinition(ImmutableArray<CatalogConfigLink> Links);

public sealed record CatalogConfigLink(
    string Source,
    ImmutableDictionary<Platform, string> Targets);

public sealed record CatalogExtensions(
    ImmutableArray<string> Bundled,
    ImmutableArray<string> Recommended);
