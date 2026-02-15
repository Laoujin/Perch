using System.Collections.Immutable;
using Perch.Core.Modules;
using Perch.Core.Packages;

namespace Perch.Core.Tests.Packages;

[TestFixture]
public sealed class AppScanServiceTests
{
    private IModuleDiscoveryService _discoveryService = null!;
    private PackageManifestParser _manifestParser = null!;
    private IPackageManagerProvider _chocoProvider = null!;
    private IPackageManagerProvider _wingetProvider = null!;
    private AppScanService _service = null!;
    private string _tempDir = null!;

    [SetUp]
    public void SetUp()
    {
        _discoveryService = Substitute.For<IModuleDiscoveryService>();
        _manifestParser = new PackageManifestParser();
        _chocoProvider = Substitute.For<IPackageManagerProvider>();
        _chocoProvider.Manager.Returns(PackageManager.Chocolatey);
        _wingetProvider = Substitute.For<IPackageManagerProvider>();
        _wingetProvider.Manager.Returns(PackageManager.Winget);

        _service = new AppScanService(_discoveryService, _manifestParser, new[] { _chocoProvider, _wingetProvider });

        _tempDir = Path.Combine(Path.GetTempPath(), $"perch-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Test]
    public async Task ScanAsync_AllManaged_AllCategorizedAsManaged()
    {
        SetupModules("git", "7zip");
        SetupInstalledChoco("git", "7zip");
        SetupInstalledWinget();

        var result = await _service.ScanAsync(_tempDir);

        Assert.Multiple(() =>
        {
            Assert.That(result.Entries, Has.Length.EqualTo(2));
            Assert.That(result.Entries.All(e => e.Category == AppCategory.Managed), Is.True);
        });
    }

    [Test]
    public async Task ScanAsync_InstalledNoModule_CategorizedCorrectly()
    {
        SetupModules("git");
        SetupInstalledChoco("git", "notepadplusplus");
        SetupInstalledWinget();

        var result = await _service.ScanAsync(_tempDir);

        var unmanaged = result.Entries.Single(e => e.Name == "notepadplusplus");
        Assert.That(unmanaged.Category, Is.EqualTo(AppCategory.InstalledNoModule));
    }

    [Test]
    public async Task ScanAsync_DefinedNotInstalled_CategorizedCorrectly()
    {
        SetupModules();
        SetupInstalledChoco();
        SetupInstalledWinget();
        WritePackagesYaml("""
            packages:
              - name: ripgrep
                manager: winget
            """);

        var result = await _service.ScanAsync(_tempDir);

        var entry = result.Entries.Single(e => e.Name == "ripgrep");
        Assert.Multiple(() =>
        {
            Assert.That(entry.Category, Is.EqualTo(AppCategory.DefinedNotInstalled));
            Assert.That(entry.Source, Is.EqualTo(PackageManager.Winget));
        });
    }

    [Test]
    public async Task ScanAsync_MixedCategories_AllPresent()
    {
        SetupModules("git");
        SetupInstalledChoco("git", "notepadplusplus");
        SetupInstalledWinget();
        WritePackagesYaml("""
            packages:
              - name: git
                manager: chocolatey
              - name: ripgrep
                manager: winget
            """);

        var result = await _service.ScanAsync(_tempDir);

        Assert.Multiple(() =>
        {
            Assert.That(result.Entries.Single(e => e.Name == "git").Category, Is.EqualTo(AppCategory.Managed));
            Assert.That(result.Entries.Single(e => e.Name == "notepadplusplus").Category, Is.EqualTo(AppCategory.InstalledNoModule));
            Assert.That(result.Entries.Single(e => e.Name == "ripgrep").Category, Is.EqualTo(AppCategory.DefinedNotInstalled));
        });
    }

    [Test]
    public async Task ScanAsync_NoPackageManifest_ScansInstalledOnly()
    {
        SetupModules("git");
        SetupInstalledChoco("git", "7zip");
        SetupInstalledWinget();

        var result = await _service.ScanAsync(_tempDir);

        Assert.Multiple(() =>
        {
            Assert.That(result.Entries, Has.Length.EqualTo(2));
            Assert.That(result.Warnings, Is.Empty);
        });
    }

    [Test]
    public async Task ScanAsync_NoManagersAvailable_ReturnsWarnings()
    {
        SetupModules();
        _chocoProvider.ScanInstalledAsync(Arg.Any<CancellationToken>())
            .Returns(PackageManagerScanResult.Unavailable("chocolatey is not installed."));
        _wingetProvider.ScanInstalledAsync(Arg.Any<CancellationToken>())
            .Returns(PackageManagerScanResult.Unavailable("winget is not installed."));

        var result = await _service.ScanAsync(_tempDir);

        Assert.Multiple(() =>
        {
            Assert.That(result.Entries, Is.Empty);
            Assert.That(result.Warnings, Has.Length.EqualTo(2));
        });
    }

    [Test]
    public async Task ScanAsync_Cancellation_Throws()
    {
        SetupModules();
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _chocoProvider.ScanInstalledAsync(Arg.Any<CancellationToken>())
            .Returns<PackageManagerScanResult>(_ => throw new OperationCanceledException());

        Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.ScanAsync(_tempDir, cts.Token));
    }

    private void SetupModules(params string[] names)
    {
        var modules = names.Select(n =>
            new AppModule(n, n, Path.Combine(_tempDir, n), ImmutableArray<Platform>.Empty, ImmutableArray<LinkEntry>.Empty))
            .ToImmutableArray();

        _discoveryService.DiscoverAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new DiscoveryResult(modules, ImmutableArray<string>.Empty));
    }

    private void SetupInstalledChoco(params string[] names)
    {
        var packages = names.Select(n => new InstalledPackage(n, PackageManager.Chocolatey)).ToImmutableArray();
        _chocoProvider.ScanInstalledAsync(Arg.Any<CancellationToken>())
            .Returns(new PackageManagerScanResult(true, packages, null));
    }

    private void SetupInstalledWinget(params string[] names)
    {
        var packages = names.Select(n => new InstalledPackage(n, PackageManager.Winget)).ToImmutableArray();
        _wingetProvider.ScanInstalledAsync(Arg.Any<CancellationToken>())
            .Returns(new PackageManagerScanResult(true, packages, null));
    }

    private void WritePackagesYaml(string yaml)
    {
        File.WriteAllText(Path.Combine(_tempDir, "packages.yaml"), yaml);
    }
}
