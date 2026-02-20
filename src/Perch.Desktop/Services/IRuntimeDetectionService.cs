using System.Collections.Immutable;

using Perch.Core.Catalog;

namespace Perch.Desktop.Services;

public interface IRuntimeDetectionService
{
    Task<RuntimeDetectionResult> DetectRuntimeAsync(CatalogEntry entry, CancellationToken cancellationToken = default);
    Task<ImmutableArray<GlobalToolMatch>> DetectGlobalToolsAsync(string runtimeId, IReadOnlyList<CatalogEntry> candidates, CancellationToken cancellationToken = default);
}

public sealed record RuntimeDetectionResult(bool IsInstalled, string? Version);

public sealed record GlobalToolMatch(string CatalogEntryId, string InstalledName);
