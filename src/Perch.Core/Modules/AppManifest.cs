using System.Collections.Immutable;

namespace Perch.Core.Modules;

public sealed record AppManifest(string ModuleName, string DisplayName, ImmutableArray<LinkEntry> Links);
