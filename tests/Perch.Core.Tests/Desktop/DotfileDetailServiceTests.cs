#if DESKTOP_TESTS
using System.Collections.Immutable;
using System.Runtime.Versioning;

using Perch.Core;
using Perch.Core.Catalog;
using Perch.Core.Config;
using Perch.Core.Modules;
using Perch.Desktop.Models;
using Perch.Desktop.Services;

namespace Perch.Core.Tests.Desktop;

[TestFixture]
[Platform("Win")]
[SupportedOSPlatform("windows")]
public sealed class DotfileDetailServiceTests
{
    private IModuleDiscoveryService _moduleDiscovery = null!;
    private ICatalogService _catalog = null!;
    private ISettingsProvider _settings = null!;
    private IPlatformDetector _platformDetector = null!;
    private DotfileDetailService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _moduleDiscovery = Substitute.For<IModuleDiscoveryService>();
        _catalog = Substitute.For<ICatalogService>();
        _settings = Substitute.For<ISettingsProvider>();
        _platformDetector = Substitute.For<IPlatformDetector>();
        _platformDetector.CurrentPlatform.Returns(Platform.Windows);
        _settings.LoadAsync(Arg.Any<CancellationToken>())
            .Returns(new PerchSettings { ConfigRepoPath = @"C:\config" });

        _service = new DotfileDetailService(_moduleDiscovery, _catalog, _settings, _platformDetector);
    }

    [Test]
    public void FindOwningModule_MatchingTarget_ReturnsModule()
    {
        var module = new AppModule(
            "git", "Git", true, @"C:\config\git",
            [Platform.Windows],
            [new LinkEntry(".gitconfig", @"%USERPROFILE%\.gitconfig", LinkType.Symlink)]);

        var result = DotfileDetailService.FindOwningModule(
            [module],
            Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.gitconfig"),
            Platform.Windows);

        Assert.That(result, Is.EqualTo(module));
    }

    [Test]
    public void FindOwningModule_NoMatch_ReturnsNull()
    {
        var module = new AppModule(
            "git", "Git", true, @"C:\config\git",
            [Platform.Windows],
            [new LinkEntry(".gitconfig", @"%USERPROFILE%\.gitconfig", LinkType.Symlink)]);

        var result = DotfileDetailService.FindOwningModule(
            [module],
            @"C:\Users\someone\.bashrc",
            Platform.Windows);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void FindOwningModule_PlatformTarget_MatchesCorrectPlatform()
    {
        var targets = new Dictionary<Platform, string>
        {
            [Platform.Windows] = @"%USERPROFILE%\.gitconfig",
            [Platform.Linux] = "~/.gitconfig",
        }.ToImmutableDictionary();

        var module = new AppModule(
            "git", "Git", true, @"C:\config\git",
            [Platform.Windows, Platform.Linux],
            [new LinkEntry(".gitconfig", null, targets, LinkType.Symlink)]);

        var result = DotfileDetailService.FindOwningModule(
            [module],
            Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.gitconfig"),
            Platform.Windows);

        Assert.That(result, Is.EqualTo(module));
    }

    [Test]
    public void FindOwningModule_EmptyModules_ReturnsNull()
    {
        var result = DotfileDetailService.FindOwningModule(
            [],
            @"C:\Users\someone\.gitconfig",
            Platform.Windows);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task LoadDetailAsync_NoConfigRepo_ReturnsEmptyDetail()
    {
        _settings.LoadAsync(Arg.Any<CancellationToken>())
            .Returns(new PerchSettings { ConfigRepoPath = null });

        var group = MakeGroup("git", "Git");
        var detail = await _service.LoadDetailAsync(group);

        Assert.Multiple(() =>
        {
            Assert.That(detail.Group, Is.EqualTo(group));
            Assert.That(detail.OwningModule, Is.Null);
            Assert.That(detail.Manifest, Is.Null);
            Assert.That(detail.Alternatives, Is.Empty);
        });
    }

    [Test]
    public async Task LoadDetailAsync_NoMatchingModule_ReturnsNullModule()
    {
        _moduleDiscovery.DiscoverAsync(@"C:\config", Arg.Any<CancellationToken>())
            .Returns(new DiscoveryResult([], []));

        var group = MakeGroup("git", "Git");
        var detail = await _service.LoadDetailAsync(group);

        Assert.Multiple(() =>
        {
            Assert.That(detail.OwningModule, Is.Null);
            Assert.That(detail.Manifest, Is.Null);
        });
    }

    [Test]
    public void FindOwningModule_ForwardSlashTarget_NormalizesAndMatches()
    {
        var module = new AppModule(
            "git", "Git", true, @"C:\config\git",
            [Platform.Windows],
            [new LinkEntry(".gitconfig", @"C:/Users/test/.gitconfig", LinkType.Symlink)]);

        var result = DotfileDetailService.FindOwningModule(
            [module],
            @"C:\Users\test\.gitconfig",
            Platform.Windows);

        Assert.That(result, Is.EqualTo(module));
    }

    [Test]
    public void FindOwningModuleByGalleryId_MatchesModuleName()
    {
        var module = new AppModule(
            "git", "Git", true, @"C:\config\git",
            [Platform.Windows],
            [new LinkEntry(".gitconfig", @"%USERPROFILE%\.gitconfig", LinkType.Symlink)]);

        var result = DotfileDetailService.FindOwningModuleByGalleryId([module], "git");

        Assert.That(result, Is.EqualTo(module));
    }

    [Test]
    public void FindOwningModuleByGalleryId_NoMatch_ReturnsNull()
    {
        var module = new AppModule(
            "git", "Git", true, @"C:\config\git",
            [Platform.Windows],
            [new LinkEntry(".gitconfig", @"%USERPROFILE%\.gitconfig", LinkType.Symlink)]);

        var result = DotfileDetailService.FindOwningModuleByGalleryId([module], "bash");

        Assert.That(result, Is.Null);
    }

    private static DotfileGroupCardModel MakeGroup(string id, string name)
    {
        var entry = new CatalogEntry(
            id, name, null, "Test", [], null, null, null, null, null, null, CatalogKind.Dotfile);
        var files = ImmutableArray.Create(
            new DotfileFileStatus(
                $".{id}config",
                $@"C:\Users\test\.{id}config",
                true,
                false,
                CardStatus.Detected));
        return new DotfileGroupCardModel(entry, files, CardStatus.Detected);
    }
}
#endif
