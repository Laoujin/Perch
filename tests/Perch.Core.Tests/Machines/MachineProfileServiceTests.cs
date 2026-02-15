using System.Collections.Immutable;
using Perch.Core.Machines;

namespace Perch.Core.Tests.Machines;

[TestFixture]
public sealed class MachineProfileServiceTests
{
    private MachineProfileService _service = null!;
    private string _tempDir = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new MachineProfileService();
        _tempDir = Path.Combine(Path.GetTempPath(), $"perch-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [Test]
    public async Task LoadAsync_MatchingProfile_ReturnsProfile()
    {
        string machinesDir = Path.Combine(_tempDir, "machines");
        Directory.CreateDirectory(machinesDir);
        string hostname = Environment.MachineName;
        string profilePath = Path.Combine(machinesDir, $"{hostname}.yaml");
        await File.WriteAllTextAsync(profilePath, """
            include-modules:
              - git
              - vscode
            """);

        MachineProfile? profile = await _service.LoadAsync(_tempDir);

        Assert.That(profile, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(profile!.IncludeModules, Is.EqualTo(new[] { "git", "vscode" }));
            Assert.That(profile.ExcludeModules, Is.Empty);
        });
    }

    [Test]
    public async Task LoadAsync_NoProfile_ReturnsNull()
    {
        string machinesDir = Path.Combine(_tempDir, "machines");
        Directory.CreateDirectory(machinesDir);
        await File.WriteAllTextAsync(Path.Combine(machinesDir, "OTHER-MACHINE.yaml"), """
            include-modules:
              - git
            """);

        MachineProfile? profile = await _service.LoadAsync(_tempDir);

        Assert.That(profile, Is.Null);
    }

    [Test]
    public async Task LoadAsync_CaseInsensitiveHostname_FindsProfile()
    {
        string machinesDir = Path.Combine(_tempDir, "machines");
        Directory.CreateDirectory(machinesDir);
        string hostname = Environment.MachineName.ToLowerInvariant();
        string profilePath = Path.Combine(machinesDir, $"{hostname}.yaml");
        await File.WriteAllTextAsync(profilePath, """
            exclude-modules:
              - steam
            """);

        MachineProfile? profile = await _service.LoadAsync(_tempDir);

        Assert.That(profile, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(profile!.ExcludeModules, Is.EqualTo(new[] { "steam" }));
            Assert.That(profile.IncludeModules, Is.Empty);
        });
    }

    [Test]
    public async Task LoadAsync_NoMachinesDir_ReturnsNull()
    {
        MachineProfile? profile = await _service.LoadAsync(_tempDir);

        Assert.That(profile, Is.Null);
    }
}
