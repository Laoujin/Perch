using System.Collections.Immutable;

using Perch.Core.Catalog;
using Perch.Core.Deploy;
using Perch.Core.Modules;
using Perch.Core.Registry;

namespace Perch.Core.Tweaks;

public sealed class TweakService : ITweakService
{
    private readonly IRegistryProvider _registryProvider;

    public TweakService(IRegistryProvider registryProvider)
    {
        _registryProvider = registryProvider;
    }

    public TweakDetectionResult Detect(TweakCatalogEntry tweak)
    {
        if (tweak.Registry.IsDefaultOrEmpty)
        {
            return new TweakDetectionResult(TweakStatus.Applied, ImmutableArray<RegistryEntryStatus>.Empty);
        }

        var entries = ImmutableArray.CreateBuilder<RegistryEntryStatus>(tweak.Registry.Length);
        int appliedCount = 0;

        foreach (RegistryEntryDefinition entry in tweak.Registry)
        {
            object? currentValue = _registryProvider.GetValue(entry.Key, entry.Name);
            bool isApplied = Equals(currentValue, entry.Value);
            entries.Add(new RegistryEntryStatus(entry, currentValue, isApplied));
            if (isApplied) appliedCount++;
        }

        TweakStatus status = appliedCount == tweak.Registry.Length
            ? TweakStatus.Applied
            : appliedCount == 0
                ? TweakStatus.NotApplied
                : TweakStatus.Partial;

        return new TweakDetectionResult(status, entries.MoveToImmutable());
    }

    public TweakOperationResult Apply(TweakCatalogEntry tweak, bool dryRun = false)
    {
        throw new NotImplementedException();
    }

    public TweakOperationResult Revert(TweakCatalogEntry tweak, bool dryRun = false)
    {
        throw new NotImplementedException();
    }
}
