using System.Collections.Immutable;

namespace Perch.Core.Modules;

public sealed record GlobalPackagesDefinition(GlobalPackageManager Manager, ImmutableArray<string> Packages);
