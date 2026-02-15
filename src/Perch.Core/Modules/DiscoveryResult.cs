using System.Collections.Immutable;

namespace Perch.Core.Modules;

public sealed record DiscoveryResult(ImmutableArray<AppModule> Modules, ImmutableArray<string> Errors);
