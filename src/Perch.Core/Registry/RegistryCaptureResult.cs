using System.Collections.Immutable;
using Perch.Core.Modules;

namespace Perch.Core.Registry;

public sealed record RegistryCaptureResult(
    ImmutableArray<RegistryEntryDefinition> Entries,
    ImmutableArray<string> Warnings);
