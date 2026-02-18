using System.Collections.Immutable;

using NSubstitute;

using Perch.Core.Catalog;
using Perch.Core.Modules;
using Perch.Core.Registry;
using Perch.Core.Tweaks;

namespace Perch.Core.Tests.Tweaks;

[TestFixture]
public sealed class TweakServiceDetectWithCaptureTests
{
    private IRegistryProvider _registry = null!;
    private ICapturedRegistryStore _capturedStore = null!;
    private TweakService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _registry = Substitute.For<IRegistryProvider>();
        _capturedStore = Substitute.For<ICapturedRegistryStore>();
        _capturedStore.LoadAsync(Arg.Any<CancellationToken>())
            .Returns(new CapturedRegistryData());
        _service = new TweakService(_registry, _capturedStore);
    }

    [Test]
    public async Task DetectWithCaptureAsync_AutoCapturesNewEntries()
    {
        _registry.GetValue(@"HKCU\Software\Test", "Value1").Returns(42);
        var tweak = MakeTweak(
            new RegistryEntryDefinition(@"HKCU\Software\Test", "Value1", 1, RegistryValueType.DWord));

        var result = await _service.DetectWithCaptureAsync(tweak);

        Assert.Multiple(() =>
        {
            Assert.That(result.Entries[0].CapturedValue, Is.EqualTo("42"));
            Assert.That(result.Entries[0].IsApplied, Is.False);
        });
        await _capturedStore.Received(1).SaveAsync(
            Arg.Is<CapturedRegistryData>(d => d.Entries.ContainsKey(@"HKCU\Software\Test\Value1")),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task DetectWithCaptureAsync_UsesExistingCapturedValue()
    {
        _registry.GetValue(@"HKCU\Software\Test", "Value1").Returns(1);
        var capturedData = new CapturedRegistryData();
        capturedData.Entries[@"HKCU\Software\Test\Value1"] = new CapturedRegistryEntry
        {
            Value = "99", Kind = RegistryValueType.DWord, CapturedAt = DateTime.UtcNow,
        };
        _capturedStore.LoadAsync(Arg.Any<CancellationToken>()).Returns(capturedData);

        var tweak = MakeTweak(
            new RegistryEntryDefinition(@"HKCU\Software\Test", "Value1", 1, RegistryValueType.DWord));

        var result = await _service.DetectWithCaptureAsync(tweak);

        Assert.That(result.Entries[0].CapturedValue, Is.EqualTo("99"));
        await _capturedStore.DidNotReceive().SaveAsync(Arg.Any<CapturedRegistryData>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task DetectWithCaptureAsync_EmptyRegistry_ReturnsEmpty()
    {
        var tweak = new TweakCatalogEntry(
            "empty", "Empty", "Test", [], null, true, [], ImmutableArray<RegistryEntryDefinition>.Empty);

        var result = await _service.DetectWithCaptureAsync(tweak);

        Assert.That(result.Entries, Is.Empty);
        await _capturedStore.DidNotReceive().LoadAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task DetectWithCaptureAsync_NullCurrentValue_DoesNotCapture()
    {
        _registry.GetValue(@"HKCU\Software\Test", "Value1").Returns((object?)null);
        var tweak = MakeTweak(
            new RegistryEntryDefinition(@"HKCU\Software\Test", "Value1", 1, RegistryValueType.DWord));

        var result = await _service.DetectWithCaptureAsync(tweak);

        Assert.That(result.Entries[0].CapturedValue, Is.Null);
        await _capturedStore.DidNotReceive().SaveAsync(Arg.Any<CapturedRegistryData>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task DetectWithCaptureAsync_PreservesDetectionStatus()
    {
        _registry.GetValue(@"HKCU\Software\Test", "Value1").Returns(1);
        var tweak = MakeTweak(
            new RegistryEntryDefinition(@"HKCU\Software\Test", "Value1", 1, RegistryValueType.DWord));

        var result = await _service.DetectWithCaptureAsync(tweak);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(TweakStatus.Applied));
            Assert.That(result.Entries[0].IsApplied, Is.True);
        });
    }

    private static TweakCatalogEntry MakeTweak(params RegistryEntryDefinition[] entries) =>
        new("test-tweak", "Test Tweak", "Test", [], null, true, [],
            entries.ToImmutableArray());
}
