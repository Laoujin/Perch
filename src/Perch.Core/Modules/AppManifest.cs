using System.Collections.Immutable;
using Perch.Core;

namespace Perch.Core.Modules;

public sealed record AppManifest(string ModuleName, string DisplayName, bool Enabled, ImmutableArray<Platform> Platforms, ImmutableArray<LinkEntry> Links, DeployHooks? Hooks = null, CleanFilterDefinition? CleanFilter = null, ImmutableArray<RegistryEntryDefinition> Registry = default, GlobalPackagesDefinition? GlobalPackages = null, ImmutableArray<string> VscodeExtensions = default, ImmutableArray<string> PsModules = default);
