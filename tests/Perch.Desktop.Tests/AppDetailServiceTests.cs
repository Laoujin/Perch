using System.Collections.Immutable;
using System.Runtime.Versioning;

using Perch.Core;
using Perch.Core.Catalog;
using Perch.Core.Config;
using Perch.Core.Modules;
using Perch.Core.Symlinks;
using Perch.Desktop.Models;
using Perch.Desktop.Services;

namespace Perch.Desktop.Tests;

[TestFixture]
[Platform("Win")]
[SupportedOSPlatform("windows")]
public sealed class AppDetailServiceTests
{
    private IModuleDiscoveryService _moduleDiscovery = null!;
    private ICatalogService _catalog = null!;
    private ISettingsProvider _settings = null!;
    private IPlatformDetector _platformDetector = null!;
    private ISymlinkProvider _symlinkProvider = null!;
    private AppDetailService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _moduleDiscovery = Substitute.For<IModuleDiscoveryService>();
        _catalog = Substitute.For<ICatalogService>();
        _settings = Substitute.For<ISettingsProvider>();
        _platformDetector = Substitute.For<IPlatformDetector>();
        _symlinkProvider = Substitute.For<ISymlinkProvider>();
        _settings.LoadAsync(Arg.Any<CancellationToken>())
            .Returns(new PerchSettings { ConfigRepoPath = @"C:\config" });
        _catalog.GetAllAppsAsync(Arg.Any<CancellationToken>())
            .Returns(ImmutableArray<CatalogEntry>.Empty);
        _platformDetector.CurrentPlatform.Returns(Platform.Windows);

        _service = new AppDetailService(_moduleDiscovery, _catalog, _settings, _platformDetector, _symlinkProvider,
            Substitute.For<ILogger<AppDetailService>>());
    }

    [Test]
    public async Task LoadDetailAsync_NoConfigRepo_ReturnsEmptyDetail()
    {
        _settings.LoadAsync(Arg.Any<CancellationToken>())
            .Returns(new PerchSettings { ConfigRepoPath = null });

        var card = MakeCard("vscode");
        var detail = await _service.LoadDetailAsync(card);

        Assert.Multiple(() =>
        {
            Assert.That(detail.Card, Is.EqualTo(card));
            Assert.That(detail.OwningModule, Is.Null);
            Assert.That(detail.Manifest, Is.Null);
            Assert.That(detail.ManifestYaml, Is.Null);
            Assert.That(detail.ManifestPath, Is.Null);
            Assert.That(detail.Alternatives, Is.Empty);
        });
    }

    [Test]
    public async Task LoadDetailAsync_NoModules_ReturnsNullModule()
    {
        _moduleDiscovery.DiscoverAsync(@"C:\config", Arg.Any<CancellationToken>())
            .Returns(new DiscoveryResult([], []));

        var card = MakeCard("vscode");
        var detail = await _service.LoadDetailAsync(card);

        Assert.Multiple(() =>
        {
            Assert.That(detail.OwningModule, Is.Null);
            Assert.That(detail.Manifest, Is.Null);
            Assert.That(detail.ManifestYaml, Is.Null);
        });
    }

    [Test]
    public async Task LoadDetailAsync_ReturnsAlternativesFromSameCategory()
    {
        _moduleDiscovery.DiscoverAsync(@"C:\config", Arg.Any<CancellationToken>())
            .Returns(new DiscoveryResult([], []));

        var alt = MakeCatalogEntry("sublime-text", "Editors");
        var other = MakeCatalogEntry("firefox", "Browsers");
        _catalog.GetAllAppsAsync(Arg.Any<CancellationToken>())
            .Returns(ImmutableArray.Create(alt, other));

        var card = MakeCard("vscode", category: "Editors");
        var detail = await _service.LoadDetailAsync(card);

        Assert.Multiple(() =>
        {
            Assert.That(detail.Alternatives, Has.Length.EqualTo(1));
            Assert.That(detail.Alternatives[0].Id, Is.EqualTo("sublime-text"));
        });
    }

    [Test]
    public async Task LoadDetailAsync_ExcludesSelfFromAlternatives()
    {
        _moduleDiscovery.DiscoverAsync(@"C:\config", Arg.Any<CancellationToken>())
            .Returns(new DiscoveryResult([], []));

        var self = MakeCatalogEntry("vscode", "Editors");
        var alt = MakeCatalogEntry("sublime-text", "Editors");
        _catalog.GetAllAppsAsync(Arg.Any<CancellationToken>())
            .Returns(ImmutableArray.Create(self, alt));

        var card = MakeCard("vscode", category: "Editors");
        var detail = await _service.LoadDetailAsync(card);

        Assert.Multiple(() =>
        {
            Assert.That(detail.Alternatives, Has.Length.EqualTo(1));
            Assert.That(detail.Alternatives[0].Id, Is.EqualTo("sublime-text"));
        });
    }

    [Test]
    public async Task LoadDetailAsync_NoAlternativesInCategory_ReturnsEmpty()
    {
        _moduleDiscovery.DiscoverAsync(@"C:\config", Arg.Any<CancellationToken>())
            .Returns(new DiscoveryResult([], []));

        _catalog.GetAllAppsAsync(Arg.Any<CancellationToken>())
            .Returns(ImmutableArray.Create(MakeCatalogEntry("firefox", "Browsers")));

        var card = MakeCard("vscode", category: "Editors");
        var detail = await _service.LoadDetailAsync(card);

        Assert.That(detail.Alternatives, Is.Empty);
    }

    [Test]
    public async Task LoadDetailAsync_WithConfigLinks_DetectsFileStatuses()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"perch-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var existingFile = Path.Combine(tempDir, "settings.json");
            File.WriteAllText(existingFile, "{}");

            var symlinkFile = Path.Combine(tempDir, "keybindings.json");
            File.WriteAllText(symlinkFile, "[]");
            _symlinkProvider.IsSymlink(symlinkFile).Returns(true);

            var missingFile = Path.Combine(tempDir, "missing.json");

            var links = ImmutableArray.Create(
                new CatalogConfigLink("settings.json",
                    new Dictionary<Platform, string> { [Platform.Windows] = existingFile }.ToImmutableDictionary()),
                new CatalogConfigLink("keybindings.json",
                    new Dictionary<Platform, string> { [Platform.Windows] = symlinkFile }.ToImmutableDictionary()),
                new CatalogConfigLink("missing.json",
                    new Dictionary<Platform, string> { [Platform.Windows] = missingFile }.ToImmutableDictionary()));
            var config = new CatalogConfigDefinition(links);
            var entry = new CatalogEntry("vscode", "vscode", null, "Editors", [], null, null, null, null, config, null);

            _moduleDiscovery.DiscoverAsync(@"C:\config", Arg.Any<CancellationToken>())
                .Returns(new DiscoveryResult([], []));

            var card = new AppCardModel(entry, CardTier.YourApps, CardStatus.Detected);
            var detail = await _service.LoadDetailAsync(card);

            Assert.Multiple(() =>
            {
                Assert.That(detail.FileStatuses, Has.Length.EqualTo(3));
                Assert.That(detail.FileStatuses[0].Status, Is.EqualTo(CardStatus.Detected));
                Assert.That(detail.FileStatuses[0].Exists, Is.True);
                Assert.That(detail.FileStatuses[1].Status, Is.EqualTo(CardStatus.Synced));
                Assert.That(detail.FileStatuses[1].IsSymlink, Is.True);
                Assert.That(detail.FileStatuses[2].Status, Is.EqualTo(CardStatus.Unmanaged));
                Assert.That(detail.FileStatuses[2].Exists, Is.False);
            });
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task LoadDetailAsync_FindsModuleByGalleryId()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"perch-test-{Guid.NewGuid():N}");
        var moduleDir = Path.Combine(tempDir, "vscode");
        Directory.CreateDirectory(moduleDir);
        try
        {
            File.WriteAllText(Path.Combine(moduleDir, "manifest.yaml"), """
                gallery: vscode
                links:
                  - source: settings.json
                    target: "C:\\Users\\test\\settings.json"
                """);

            var module = new AppModule("vscode", "vscode", true, moduleDir,
                [], [], null, null, [], null, [], []);
            _moduleDiscovery.DiscoverAsync(@"C:\config", Arg.Any<CancellationToken>())
                .Returns(new DiscoveryResult([module], []));

            var card = MakeCard("vscode");
            var detail = await _service.LoadDetailAsync(card);

            Assert.Multiple(() =>
            {
                Assert.That(detail.OwningModule, Is.Not.Null);
                Assert.That(detail.OwningModule!.Name, Is.EqualTo("vscode"));
                Assert.That(detail.Manifest, Is.Not.Null);
                Assert.That(detail.ManifestYaml, Is.Not.Null.And.Not.Empty);
                Assert.That(detail.ManifestPath, Does.EndWith("manifest.yaml"));
            });
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task LoadDetailAsync_ModuleWithDifferentGalleryId_NotMatched()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"perch-test-{Guid.NewGuid():N}");
        var moduleDir = Path.Combine(tempDir, "rider");
        Directory.CreateDirectory(moduleDir);
        try
        {
            File.WriteAllText(Path.Combine(moduleDir, "manifest.yaml"), """
                gallery: rider
                links:
                  - source: config
                    target: "C:\\test\\config"
                """);

            var module = new AppModule("rider", "rider", true, moduleDir,
                [], [], null, null, [], null, [], []);
            _moduleDiscovery.DiscoverAsync(@"C:\config", Arg.Any<CancellationToken>())
                .Returns(new DiscoveryResult([module], []));

            var card = MakeCard("vscode");
            var detail = await _service.LoadDetailAsync(card);

            Assert.That(detail.OwningModule, Is.Null);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static AppCardModel MakeCard(string id, string category = "Editors") =>
        new(MakeCatalogEntry(id, category), CardTier.YourApps, CardStatus.Detected);

    private static CatalogEntry MakeCatalogEntry(string id, string category) =>
        new(id, id, null, category, [], null, null, null, null, null, null);
}
