using System.Collections.Immutable;

using NSubstitute;

using Perch.Core.Catalog;
using Perch.Core.Deploy;
using Perch.Core.Modules;
using Perch.Core.Registry;
using Perch.Core.Tweaks;

namespace Perch.Core.Tests.Tweaks;

[TestFixture]
public sealed class TweakServiceRevertToCapturedTests
{
    private IRegistryProvider _registry = null!;
    private ICapturedRegistryStore _capturedStore = null!;
    private TweakService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _registry = Substitute.For<IRegistryProvider>();
        _capturedStore = Substitute.For<ICapturedRegistryStore>();
        _capturedStore.LoadAsync(Arg.Any<CancellationToken>()).Returns(new CapturedRegistryData());
        _service = new TweakService(_registry, _capturedStore);
    }

    [Test]
    public async Task RevertToCaptured_UsesCapturedValue()
    {
        var captured = new CapturedRegistryData();
        captured.Entries[@"HKCU\Software\Test\Value1"] = new CapturedRegistryEntry
        {
            Value = "99", Kind = RegistryValueType.DWord, CapturedAt = DateTime.UtcNow,
        };
        _capturedStore.LoadAsync(Arg.Any<CancellationToken>()).Returns(captured);

        var tweak = MakeTweak(
            new RegistryEntryDefinition(@"HKCU\Software\Test", "Value1", 1, RegistryValueType.DWord, 0));

        var result = await _service.RevertToCapturedAsync(tweak);

        Assert.That(result.Level, Is.EqualTo(ResultLevel.Ok));
        _registry.Received(1).SetValue(@"HKCU\Software\Test", "Value1", "99", RegistryValueType.DWord);
        Assert.That(result.Entries[0].Message, Does.Contain("Restored"));
    }

    [Test]
    public async Task RevertToCaptured_FallsBackToDefaultValue()
    {
        var tweak = MakeTweak(
            new RegistryEntryDefinition(@"HKCU\Software\Test", "Value1", 1, RegistryValueType.DWord, 42));

        var result = await _service.RevertToCapturedAsync(tweak);

        Assert.That(result.Level, Is.EqualTo(ResultLevel.Ok));
        _registry.Received(1).SetValue(@"HKCU\Software\Test", "Value1", 42, RegistryValueType.DWord);
    }

    [Test]
    public async Task RevertToCaptured_NoCapturedNoDefault_DeletesEntry()
    {
        var tweak = MakeTweak(
            new RegistryEntryDefinition(@"HKCU\Software\Test", "Value1", 1, RegistryValueType.DWord));

        var result = await _service.RevertToCapturedAsync(tweak);

        _registry.Received(1).DeleteValue(@"HKCU\Software\Test", "Value1");
        Assert.That(result.Entries[0].Message, Does.Contain("Deleted"));
    }

    [Test]
    public async Task RevertToCaptured_DryRun_DoesNotWrite()
    {
        var captured = new CapturedRegistryData();
        captured.Entries[@"HKCU\Software\Test\Value1"] = new CapturedRegistryEntry
        {
            Value = "99", Kind = RegistryValueType.DWord, CapturedAt = DateTime.UtcNow,
        };
        _capturedStore.LoadAsync(Arg.Any<CancellationToken>()).Returns(captured);

        var tweak = MakeTweak(
            new RegistryEntryDefinition(@"HKCU\Software\Test", "Value1", 1, RegistryValueType.DWord));

        var result = await _service.RevertToCapturedAsync(tweak, dryRun: true);

        Assert.That(result.Entries[0].Message, Does.Contain("Would restore"));
        _registry.DidNotReceive().SetValue(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<object>(), Arg.Any<RegistryValueType>());
    }

    [Test]
    public async Task RevertToCaptured_EmptyRegistry_ReturnsEmpty()
    {
        var tweak = new TweakCatalogEntry(
            "empty", "Empty", "Test", [], null, true, [], ImmutableArray<RegistryEntryDefinition>.Empty);

        var result = await _service.RevertToCapturedAsync(tweak);

        Assert.That(result.Entries, Is.Empty);
    }

    [Test]
    public async Task RevertToCaptured_DryRun_NoCapturedNoDefault_ReportsDelete()
    {
        var tweak = MakeTweak(
            new RegistryEntryDefinition(@"HKCU\Software\Test", "Value1", 1, RegistryValueType.DWord));

        var result = await _service.RevertToCapturedAsync(tweak, dryRun: true);

        Assert.That(result.Entries[0].Message, Does.Contain("Would delete"));
        _registry.DidNotReceive().DeleteValue(Arg.Any<string>(), Arg.Any<string>());
    }

    private static TweakCatalogEntry MakeTweak(params RegistryEntryDefinition[] entries) =>
        new("test-tweak", "Test Tweak", "Test", [], null, true, [],
            entries.ToImmutableArray());
}
