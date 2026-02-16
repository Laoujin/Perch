using System.Collections.Immutable;
using Perch.Core.Modules;

namespace Perch.Core.Registry;

public sealed class RegistryCaptureService : IRegistryCaptureService
{
    private readonly IRegistryProvider _registryProvider;

    public RegistryCaptureService(IRegistryProvider registryProvider)
    {
        _registryProvider = registryProvider;
    }

    public RegistryCaptureResult Capture(ImmutableArray<RegistryEntryDefinition> entries)
    {
        var captured = new List<RegistryEntryDefinition>();
        var warnings = new List<string>();

        foreach (var entry in entries)
        {
            object? currentValue = _registryProvider.GetValue(entry.Key, entry.Name);
            if (currentValue == null)
            {
                warnings.Add($"Registry value not found: {entry.Key}\\{entry.Name}");
                continue;
            }

            captured.Add(entry with { Value = currentValue });
        }

        return new RegistryCaptureResult(captured.ToImmutableArray(), warnings.ToImmutableArray());
    }
}
