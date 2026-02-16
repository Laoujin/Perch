using System.Collections.Immutable;

using NSubstitute;

using Perch.Core.Modules;
using Perch.Core.Registry;

namespace Perch.Core.Tests.Registry;

[TestFixture]
public sealed class RegistryCaptureServiceTests
{
    private IRegistryProvider _registryProvider = null!;
    private RegistryCaptureService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _registryProvider = Substitute.For<IRegistryProvider>();
        _service = new RegistryCaptureService(_registryProvider);
    }

    [Test]
    public void Capture_ExistingKeys_ReturnsCurrentValues()
    {
        var entries = ImmutableArray.Create(
            new RegistryEntryDefinition("HKCU\\Software\\Test", "Setting1", 0, RegistryValueType.DWord));

        _registryProvider.GetValue("HKCU\\Software\\Test", "Setting1").Returns(42);

        var result = _service.Capture(entries);

        Assert.Multiple(() =>
        {
            Assert.That(result.Entries, Has.Length.EqualTo(1));
            Assert.That(result.Entries[0].Value, Is.EqualTo(42));
            Assert.That(result.Warnings, Is.Empty);
        });
    }

    [Test]
    public void Capture_MissingKey_ProducesWarning()
    {
        var entries = ImmutableArray.Create(
            new RegistryEntryDefinition("HKCU\\Software\\Test", "Missing", 0, RegistryValueType.DWord));

        _registryProvider.GetValue("HKCU\\Software\\Test", "Missing").Returns((object?)null);

        var result = _service.Capture(entries);

        Assert.Multiple(() =>
        {
            Assert.That(result.Entries, Is.Empty);
            Assert.That(result.Warnings, Has.Length.EqualTo(1));
            Assert.That(result.Warnings[0], Does.Contain("Missing"));
        });
    }

    [Test]
    public void Capture_MixedExistingAndMissing_CapturesFoundAndWarnsOnMissing()
    {
        var entries = ImmutableArray.Create(
            new RegistryEntryDefinition("HKCU\\Software\\Test", "Found", 0, RegistryValueType.DWord),
            new RegistryEntryDefinition("HKCU\\Software\\Test", "Gone", "", RegistryValueType.String));

        _registryProvider.GetValue("HKCU\\Software\\Test", "Found").Returns(100);
        _registryProvider.GetValue("HKCU\\Software\\Test", "Gone").Returns((object?)null);

        var result = _service.Capture(entries);

        Assert.Multiple(() =>
        {
            Assert.That(result.Entries, Has.Length.EqualTo(1));
            Assert.That(result.Warnings, Has.Length.EqualTo(1));
        });
    }
}
