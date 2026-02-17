using System.Collections.Immutable;

using Perch.Core.Catalog;
using Perch.Core.Modules;

namespace Perch.Desktop.Models;

public sealed record DotfileDetail(
    DotfileCardModel Card,
    AppModule? OwningModule,
    AppManifest? Manifest,
    string? ManifestYaml,
    string? ManifestPath,
    ImmutableArray<CatalogEntry> Alternatives);
