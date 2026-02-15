using System.Collections.Immutable;

namespace Perch.Core.Modules;

public sealed record AppModule(string Name, string DisplayName, string ModulePath, ImmutableArray<LinkEntry> Links);
