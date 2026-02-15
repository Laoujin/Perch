using System.Collections.Immutable;

namespace Perch.Core.Machines;

public sealed record MachineProfile(
    ImmutableArray<string> IncludeModules,
    ImmutableArray<string> ExcludeModules,
    ImmutableDictionary<string, string> Variables);
