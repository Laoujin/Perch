using System.Collections.Immutable;
using Perch.Core.Machines;

namespace Perch.Core.Tests.Machines;

[TestFixture]
public sealed class MachineProfileServiceTests
{
    private MachineProfileService _service = null!;
    private string _tempDir = null!;
    private string _machinesDir = null!;
    private string _hostname = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new MachineProfileService();
        _tempDir = Path.Combine(Path.GetTempPath(), $"perch-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _machinesDir = Path.Combine(_tempDir, "machines");
        Directory.CreateDirectory(_machinesDir);
        _hostname = Environment.MachineName;
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
        string profilePath = Path.Combine(_machinesDir, $"{_hostname}.yaml");
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
            Assert.That(profile.Variables, Is.Empty);
        });
    }

    [Test]
    public async Task LoadAsync_NoProfile_ReturnsNull()
    {
        await File.WriteAllTextAsync(Path.Combine(_machinesDir, "OTHER-MACHINE.yaml"), """
            include-modules:
              - git
            """);

        MachineProfile? profile = await _service.LoadAsync(_tempDir);

        Assert.That(profile, Is.Null);
    }

    [Test]
    public async Task LoadAsync_CaseInsensitiveHostname_FindsProfile()
    {
        string hostname = Environment.MachineName.ToLowerInvariant();
        string profilePath = Path.Combine(_machinesDir, $"{hostname}.yaml");
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
        Directory.Delete(_machinesDir);

        MachineProfile? profile = await _service.LoadAsync(_tempDir);

        Assert.That(profile, Is.Null);
    }

    [Test]
    public async Task LoadAsync_OnlyBase_ReturnsBaseProfile()
    {
        await File.WriteAllTextAsync(Path.Combine(_machinesDir, "base.yaml"), """
            include-modules:
              - git
              - vscode
            variables:
              editor: code
              shell: bash
            """);

        MachineProfile? profile = await _service.LoadAsync(_tempDir);

        Assert.That(profile, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(profile!.IncludeModules, Is.EqualTo(new[] { "git", "vscode" }));
            Assert.That(profile.ExcludeModules, Is.Empty);
            Assert.That(profile.Variables, Is.EqualTo(new Dictionary<string, string>
            {
                ["editor"] = "code",
                ["shell"] = "bash",
            }));
        });
    }

    [Test]
    public async Task LoadAsync_BaseAndHostname_HostnameOverridesIncludes()
    {
        await File.WriteAllTextAsync(Path.Combine(_machinesDir, "base.yaml"), """
            include-modules:
              - git
              - vscode
            """);
        await File.WriteAllTextAsync(Path.Combine(_machinesDir, $"{_hostname}.yaml"), """
            include-modules:
              - git
              - docker
            """);

        MachineProfile? profile = await _service.LoadAsync(_tempDir);

        Assert.That(profile, Is.Not.Null);
        Assert.That(profile!.IncludeModules, Is.EqualTo(new[] { "git", "docker" }));
    }

    [Test]
    public async Task LoadAsync_BaseAndHostname_HostnameOverridesExcludes()
    {
        await File.WriteAllTextAsync(Path.Combine(_machinesDir, "base.yaml"), """
            exclude-modules:
              - steam
            """);
        await File.WriteAllTextAsync(Path.Combine(_machinesDir, $"{_hostname}.yaml"), """
            exclude-modules:
              - games
              - media
            """);

        MachineProfile? profile = await _service.LoadAsync(_tempDir);

        Assert.That(profile, Is.Not.Null);
        Assert.That(profile!.ExcludeModules, Is.EqualTo(new[] { "games", "media" }));
    }

    [Test]
    public async Task LoadAsync_BaseAndHostname_MergesVariables()
    {
        await File.WriteAllTextAsync(Path.Combine(_machinesDir, "base.yaml"), """
            variables:
              editor: vim
              shell: bash
            """);
        await File.WriteAllTextAsync(Path.Combine(_machinesDir, $"{_hostname}.yaml"), """
            variables:
              editor: code
            """);

        MachineProfile? profile = await _service.LoadAsync(_tempDir);

        Assert.That(profile, Is.Not.Null);
        Assert.That(profile!.Variables, Is.EqualTo(new Dictionary<string, string>
        {
            ["editor"] = "code",
            ["shell"] = "bash",
        }));
    }

    [Test]
    public async Task LoadAsync_BaseAndHostname_HostnameOmitsList_UsesBase()
    {
        await File.WriteAllTextAsync(Path.Combine(_machinesDir, "base.yaml"), """
            include-modules:
              - git
            exclude-modules:
              - steam
            """);
        await File.WriteAllTextAsync(Path.Combine(_machinesDir, $"{_hostname}.yaml"), """
            variables:
              editor: code
            """);

        MachineProfile? profile = await _service.LoadAsync(_tempDir);

        Assert.That(profile, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(profile!.IncludeModules, Is.EqualTo(new[] { "git" }));
            Assert.That(profile.ExcludeModules, Is.EqualTo(new[] { "steam" }));
            Assert.That(profile.Variables, Is.EqualTo(new Dictionary<string, string>
            {
                ["editor"] = "code",
            }));
        });
    }

    [Test]
    public async Task LoadAsync_OnlyHostname_ReturnsProfile()
    {
        await File.WriteAllTextAsync(Path.Combine(_machinesDir, $"{_hostname}.yaml"), """
            include-modules:
              - git
            """);

        MachineProfile? profile = await _service.LoadAsync(_tempDir);

        Assert.That(profile, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(profile!.IncludeModules, Is.EqualTo(new[] { "git" }));
            Assert.That(profile.Variables, Is.Empty);
        });
    }

    [Test]
    public async Task LoadAsync_NeitherBaseNorHostname_ReturnsNull()
    {
        await File.WriteAllTextAsync(Path.Combine(_machinesDir, "OTHER-MACHINE.yaml"), """
            include-modules:
              - git
            """);

        MachineProfile? profile = await _service.LoadAsync(_tempDir);

        Assert.That(profile, Is.Null);
    }

    [Test]
    public async Task LoadAsync_BaseOnlyVariables_ReturnsProfile()
    {
        await File.WriteAllTextAsync(Path.Combine(_machinesDir, "base.yaml"), """
            variables:
              editor: code
            """);

        MachineProfile? profile = await _service.LoadAsync(_tempDir);

        Assert.That(profile, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(profile!.IncludeModules, Is.Empty);
            Assert.That(profile.ExcludeModules, Is.Empty);
            Assert.That(profile.Variables, Is.EqualTo(new Dictionary<string, string>
            {
                ["editor"] = "code",
            }));
        });
    }
}
