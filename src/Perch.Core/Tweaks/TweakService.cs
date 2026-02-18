using System.Collections.Immutable;

using Perch.Core.Catalog;
using Perch.Core.Deploy;
using Perch.Core.Modules;
using Perch.Core.Registry;

namespace Perch.Core.Tweaks;

public sealed class TweakService : ITweakService
{
    private readonly IRegistryProvider _registryProvider;
    private readonly ICapturedRegistryStore _capturedStore;

    public TweakService(IRegistryProvider registryProvider, ICapturedRegistryStore capturedStore)
    {
        _registryProvider = registryProvider;
        _capturedStore = capturedStore;
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
            object? currentValue;
            try
            {
                currentValue = _registryProvider.GetValue(entry.Key, entry.Name);
            }
            catch (Exception)
            {
                entries.Add(new RegistryEntryStatus(entry, null, null, false));
                continue;
            }

            bool isApplied = Equals(currentValue, entry.Value);
            entries.Add(new RegistryEntryStatus(entry, currentValue, null, isApplied));
            if (isApplied) appliedCount++;
        }

        TweakStatus status = appliedCount == tweak.Registry.Length
            ? TweakStatus.Applied
            : appliedCount == 0
                ? TweakStatus.NotApplied
                : TweakStatus.Partial;

        return new TweakDetectionResult(status, entries.MoveToImmutable());
    }

    public async Task<TweakDetectionResult> DetectWithCaptureAsync(TweakCatalogEntry tweak, CancellationToken cancellationToken = default)
    {
        var syncResult = Detect(tweak);
        if (syncResult.Entries.IsEmpty)
            return syncResult;

        var capturedData = await _capturedStore.LoadAsync(cancellationToken).ConfigureAwait(false);
        bool dirty = false;

        var enriched = ImmutableArray.CreateBuilder<RegistryEntryStatus>(syncResult.Entries.Length);
        foreach (var entry in syncResult.Entries)
        {
            string key = $@"{entry.Definition.Key}\{entry.Definition.Name}";
            string? capturedValue = null;

            if (capturedData.Entries.TryGetValue(key, out var captured))
            {
                capturedValue = captured.Value;
            }
            else if (entry.CurrentValue != null)
            {
                capturedData.Entries[key] = new CapturedRegistryEntry
                {
                    Value = entry.CurrentValue.ToString(),
                    Kind = entry.Definition.Kind,
                    CapturedAt = DateTime.UtcNow,
                };
                capturedValue = entry.CurrentValue.ToString();
                dirty = true;
            }

            enriched.Add(entry with { CapturedValue = capturedValue });
        }

        if (dirty)
            await _capturedStore.SaveAsync(capturedData, cancellationToken).ConfigureAwait(false);

        return new TweakDetectionResult(syncResult.Status, enriched.MoveToImmutable());
    }

    public TweakOperationResult Apply(TweakCatalogEntry tweak, bool dryRun = false)
    {
        if (tweak.Registry.IsDefaultOrEmpty)
        {
            return new TweakOperationResult(ResultLevel.Ok, ImmutableArray<TweakEntryResult>.Empty);
        }

        var results = ImmutableArray.CreateBuilder<TweakEntryResult>(tweak.Registry.Length);
        ResultLevel overall = ResultLevel.Ok;

        foreach (RegistryEntryDefinition entry in tweak.Registry)
        {
            string location = $@"{entry.Key}\{entry.Name}";

            if (dryRun)
            {
                results.Add(new TweakEntryResult(entry.Key, entry.Name, ResultLevel.Ok,
                    $"Would set {location} to {entry.Value}"));
                continue;
            }

            object? currentValue = _registryProvider.GetValue(entry.Key, entry.Name);
            if (Equals(currentValue, entry.Value))
            {
                results.Add(new TweakEntryResult(entry.Key, entry.Name, ResultLevel.Ok,
                    $"Already set to {entry.Value}"));
                continue;
            }

            if (entry.Value == null)
            {
                _registryProvider.DeleteValue(entry.Key, entry.Name);
                results.Add(new TweakEntryResult(entry.Key, entry.Name, ResultLevel.Ok,
                    $"Deleted {entry.Name}"));
            }
            else
            {
                _registryProvider.SetValue(entry.Key, entry.Name, entry.Value, entry.Kind);
                results.Add(new TweakEntryResult(entry.Key, entry.Name, ResultLevel.Ok,
                    $"Set {entry.Name} to {entry.Value}"));
            }
        }

        return new TweakOperationResult(overall, results.MoveToImmutable());
    }

    public TweakOperationResult Revert(TweakCatalogEntry tweak, bool dryRun = false)
    {
        if (!tweak.Reversible)
        {
            return new TweakOperationResult(ResultLevel.Error,
                [new TweakEntryResult("", "", ResultLevel.Error, "Tweak is not reversible")]);
        }

        if (tweak.Registry.IsDefaultOrEmpty)
        {
            return new TweakOperationResult(ResultLevel.Ok, ImmutableArray<TweakEntryResult>.Empty);
        }

        var results = ImmutableArray.CreateBuilder<TweakEntryResult>(tweak.Registry.Length);

        foreach (RegistryEntryDefinition entry in tweak.Registry)
        {
            if (dryRun)
            {
                string action = entry.DefaultValue != null
                    ? $"Would restore {entry.Name} to {entry.DefaultValue}"
                    : $"Would delete {entry.Name}";
                results.Add(new TweakEntryResult(entry.Key, entry.Name, ResultLevel.Ok, action));
                continue;
            }

            if (entry.DefaultValue != null)
            {
                _registryProvider.SetValue(entry.Key, entry.Name, entry.DefaultValue, entry.Kind);
                results.Add(new TweakEntryResult(entry.Key, entry.Name, ResultLevel.Ok,
                    $"Restored {entry.Name} to {entry.DefaultValue}"));
            }
            else
            {
                _registryProvider.DeleteValue(entry.Key, entry.Name);
                results.Add(new TweakEntryResult(entry.Key, entry.Name, ResultLevel.Ok,
                    $"Deleted {entry.Name}"));
            }
        }

        return new TweakOperationResult(ResultLevel.Ok, results.MoveToImmutable());
    }

    public async Task<TweakOperationResult> RevertToCapturedAsync(TweakCatalogEntry tweak, bool dryRun = false, CancellationToken cancellationToken = default)
    {
        if (tweak.Registry.IsDefaultOrEmpty)
            return new TweakOperationResult(ResultLevel.Ok, ImmutableArray<TweakEntryResult>.Empty);

        var capturedData = await _capturedStore.LoadAsync(cancellationToken).ConfigureAwait(false);
        var results = ImmutableArray.CreateBuilder<TweakEntryResult>(tweak.Registry.Length);

        foreach (RegistryEntryDefinition entry in tweak.Registry)
        {
            string capturedKey = $@"{entry.Key}\{entry.Name}";
            object? targetValue = null;
            RegistryValueType targetKind = entry.Kind;

            if (capturedData.Entries.TryGetValue(capturedKey, out var captured))
            {
                targetValue = captured.Value;
                targetKind = captured.Kind;
            }
            else if (entry.DefaultValue != null)
            {
                targetValue = entry.DefaultValue;
            }

            if (dryRun)
            {
                string action = targetValue != null
                    ? $"Would restore {entry.Name} to {targetValue}"
                    : $"Would delete {entry.Name}";
                results.Add(new TweakEntryResult(entry.Key, entry.Name, ResultLevel.Ok, action));
                continue;
            }

            if (targetValue != null)
            {
                _registryProvider.SetValue(entry.Key, entry.Name, targetValue, targetKind);
                results.Add(new TweakEntryResult(entry.Key, entry.Name, ResultLevel.Ok,
                    $"Restored {entry.Name} to {targetValue}"));
            }
            else
            {
                _registryProvider.DeleteValue(entry.Key, entry.Name);
                results.Add(new TweakEntryResult(entry.Key, entry.Name, ResultLevel.Ok,
                    $"Deleted {entry.Name}"));
            }
        }

        return new TweakOperationResult(ResultLevel.Ok, results.MoveToImmutable());
    }
}
