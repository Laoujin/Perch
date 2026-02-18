using System.Collections.Immutable;

using Perch.Core.Modules;

namespace Perch.Core.Registry;

public sealed class ThreeValueService(IRegistryProvider registryProvider) : IThreeValueService
{
    public ImmutableArray<RegistryThreeValueResult> Evaluate(ImmutableArray<RegistryEntryDefinition> entries, IReadOnlyDictionary<string, string?>? capturedValues = null)
    {
        var results = new List<RegistryThreeValueResult>(entries.Length);

        foreach (var entry in entries)
        {
            string key = $@"{entry.Key}\{entry.Name}";
            string? captured = null;
            capturedValues?.TryGetValue(key, out captured);
            results.Add(EvaluateEntry(entry, captured));
        }

        return results.ToImmutableArray();
    }

    private RegistryThreeValueResult EvaluateEntry(RegistryEntryDefinition entry, string? capturedValue)
    {
        object? current;
        try
        {
            current = registryProvider.GetValue(entry.Key, entry.Name);
        }
        catch (Exception)
        {
            return new RegistryThreeValueResult(entry, ThreeValueStatus.Error, null, capturedValue);
        }

        var status = DetermineStatus(current, entry.Value, entry.DefaultValue, capturedValue);
        return new RegistryThreeValueResult(entry, status, current, capturedValue);
    }

    private static ThreeValueStatus DetermineStatus(object? current, object? desired, object? defaultValue, string? capturedValue)
    {
        if (ValuesEqual(current, desired))
        {
            return ThreeValueStatus.Applied;
        }

        if (defaultValue != null && ValuesEqual(current, defaultValue))
        {
            return ThreeValueStatus.NotApplied;
        }

        if (capturedValue != null && ValuesEqual(current, capturedValue))
        {
            return ThreeValueStatus.AtCaptured;
        }

        if (desired == null && current == null)
        {
            return ThreeValueStatus.Applied;
        }

        if (desired != null && current == null)
        {
            return ThreeValueStatus.NotApplied;
        }

        if (defaultValue == null && current == null)
        {
            return ThreeValueStatus.Reverted;
        }

        return ThreeValueStatus.Drifted;
    }

    private static bool ValuesEqual(object? a, object? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.Equals(b) || string.Equals(a.ToString(), b.ToString(), StringComparison.Ordinal);
    }
}
