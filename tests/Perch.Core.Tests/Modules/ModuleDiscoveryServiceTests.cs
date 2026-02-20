using System.Collections.Immutable;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using Perch.Core.Catalog;
using Perch.Core.Git;
using Perch.Core.Modules;

namespace Perch.Core.Tests.Modules;

[TestFixture]
public sealed class ModuleDiscoveryServiceTests
{
    private string _tempDir = null!;
    private ModuleDiscoveryService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"perch-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _service = new ModuleDiscoveryService(new ManifestParser());
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Test]
    public async Task DiscoverAsync_MultipleModules_ReturnsAll()
    {
        CreateModule("git", """
            links:
              - source: .gitconfig
                target: "C:\\Users\\test\\.gitconfig"
            """);
        CreateModule("vscode", """
            display-name: Visual Studio Code
            links:
              - source: settings.json
                target: "%APPDATA%\\Code\\User\\settings.json"
            """);

        DiscoveryResult result = await _service.DiscoverAsync(_tempDir);

        Assert.Multiple(() =>
        {
            Assert.That(result.Modules, Has.Length.EqualTo(2));
            Assert.That(result.Errors, Is.Empty);
            Assert.That(result.Modules[0].Name, Is.EqualTo("git"));
            Assert.That(result.Modules[1].Name, Is.EqualTo("vscode"));
            Assert.That(result.Modules[1].DisplayName, Is.EqualTo("Visual Studio Code"));
        });
    }

    [Test]
    public async Task DiscoverAsync_FolderWithoutManifest_IsIgnored()
    {
        CreateModule("git", """
            links:
              - source: .gitconfig
                target: "C:\\Users\\test\\.gitconfig"
            """);
        Directory.CreateDirectory(Path.Combine(_tempDir, "no-manifest"));

        DiscoveryResult result = await _service.DiscoverAsync(_tempDir);

        Assert.That(result.Modules, Has.Length.EqualTo(1));
        Assert.That(result.Modules[0].Name, Is.EqualTo("git"));
    }

    [Test]
    public async Task DiscoverAsync_InvalidManifest_ReportsErrorAndContinues()
    {
        CreateModule("good", """
            links:
              - source: config
                target: "C:\\test\\config"
            """);
        CreateModule("bad", "{{invalid yaml");

        DiscoveryResult result = await _service.DiscoverAsync(_tempDir);

        Assert.Multiple(() =>
        {
            Assert.That(result.Modules, Has.Length.EqualTo(1));
            Assert.That(result.Modules[0].Name, Is.EqualTo("good"));
            Assert.That(result.Errors, Has.Length.EqualTo(1));
            Assert.That(result.Errors[0], Does.Contain("bad"));
        });
    }

    [Test]
    public async Task DiscoverAsync_EmptyDirectory_ReturnsEmpty()
    {
        DiscoveryResult result = await _service.DiscoverAsync(_tempDir);

        Assert.Multiple(() =>
        {
            Assert.That(result.Modules, Is.Empty);
            Assert.That(result.Errors, Is.Empty);
        });
    }

    [Test]
    public void DiscoverAsync_Cancellation_Throws()
    {
        CreateModule("git", """
            links:
              - source: .gitconfig
                target: "C:\\Users\\test\\.gitconfig"
            """);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.DiscoverAsync(_tempDir, cts.Token));
    }

    [Test]
    public async Task DiscoverAsync_UnreadableManifest_ReportsErrorAndContinues()
    {
        CreateModule("good", """
            links:
              - source: config
                target: "C:\\test\\config"
            """);
        CreateModule("locked", """
            links:
              - source: data
                target: "C:\\test\\data"
            """);

        var lockedPath = Path.Combine(_tempDir, "locked", "manifest.yaml");
        using var lockHandle = File.Open(lockedPath, FileMode.Open, FileAccess.Read, FileShare.None);

        DiscoveryResult result = await _service.DiscoverAsync(_tempDir);

        Assert.Multiple(() =>
        {
            Assert.That(result.Modules, Has.Length.EqualTo(1));
            Assert.That(result.Modules[0].Name, Is.EqualTo("good"));
            Assert.That(result.Errors, Has.Length.EqualTo(1));
            Assert.That(result.Errors[0], Does.Contain("locked"));
            Assert.That(result.Errors[0], Does.Contain("Failed to read manifest"));
        });
    }

    [Test]
    public async Task DiscoverAsync_NonExistentPath_ReturnsError()
    {
        string fakePath = Path.Combine(Path.GetTempPath(), $"perch-no-exist-{Guid.NewGuid():N}");

        DiscoveryResult result = await _service.DiscoverAsync(fakePath);

        Assert.Multiple(() =>
        {
            Assert.That(result.Modules, Is.Empty);
            Assert.That(result.Errors, Has.Length.EqualTo(1));
            Assert.That(result.Errors[0], Does.Contain("does not exist"));
        });
    }

    [Test]
    public async Task DiscoverAsync_SubmoduleService_InitializesSubmodules()
    {
        var submoduleService = Substitute.For<ISubmoduleService>();
        var svc = new ModuleDiscoveryService(new ManifestParser(), submoduleService: submoduleService);
        CreateModule("git", """
            links:
              - source: .gitconfig
                target: "C:\\Users\\test\\.gitconfig"
            """);

        await svc.DiscoverAsync(_tempDir);

        await submoduleService.Received(1).InitializeIfNeededAsync(_tempDir, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task DiscoverAsync_GalleryOverlay_MergesSuccessfully()
    {
        var catalogService = Substitute.For<ICatalogService>();
        var overlayService = Substitute.For<IGalleryOverlayService>();
        var galleryEntry = new CatalogEntry("vscode", "vscode", "Visual Studio Code", "editors",
            ImmutableArray<string>.Empty, null, null, null, null, null, null);
        catalogService.GetAppAsync("vscode", Arg.Any<CancellationToken>()).Returns(galleryEntry);
        overlayService.Merge(Arg.Any<AppManifest>(), galleryEntry).Returns(call =>
        {
            var m = call.Arg<AppManifest>();
            return m with { DisplayName = "VS Code Merged" };
        });

        var svc = new ModuleDiscoveryService(new ManifestParser(), catalogService, overlayService);
        CreateModule("code", """
            gallery: vscode
            links:
              - source: settings.json
                target: "%APPDATA%\\Code\\User\\settings.json"
            """);

        DiscoveryResult result = await svc.DiscoverAsync(_tempDir);

        Assert.Multiple(() =>
        {
            Assert.That(result.Modules, Has.Length.EqualTo(1));
            Assert.That(result.Modules[0].DisplayName, Is.EqualTo("VS Code Merged"));
            Assert.That(result.Errors, Is.Empty);
        });
        overlayService.Received(1).Merge(Arg.Any<AppManifest>(), galleryEntry);
    }

    [Test]
    public async Task DiscoverAsync_GalleryOverlay_AppNotFound_ReportsError()
    {
        var catalogService = Substitute.For<ICatalogService>();
        var overlayService = Substitute.For<IGalleryOverlayService>();
        catalogService.GetAppAsync("nonexistent", Arg.Any<CancellationToken>()).Returns((CatalogEntry?)null);

        var svc = new ModuleDiscoveryService(new ManifestParser(), catalogService, overlayService);
        CreateModule("myapp", """
            gallery: nonexistent
            links:
              - source: config
                target: "C:\\test\\config"
            """);

        DiscoveryResult result = await svc.DiscoverAsync(_tempDir);

        Assert.Multiple(() =>
        {
            Assert.That(result.Modules, Has.Length.EqualTo(1));
            Assert.That(result.Errors, Has.Length.EqualTo(1));
            Assert.That(result.Errors[0], Does.Contain("nonexistent"));
            Assert.That(result.Errors[0], Does.Contain("not found"));
        });
    }

    [Test]
    public async Task DiscoverAsync_GalleryOverlay_Exception_ReportsError()
    {
        var catalogService = Substitute.For<ICatalogService>();
        var overlayService = Substitute.For<IGalleryOverlayService>();
        catalogService.GetAppAsync("broken", Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Network error"));

        var svc = new ModuleDiscoveryService(new ManifestParser(), catalogService, overlayService);
        CreateModule("myapp", """
            gallery: broken
            links:
              - source: config
                target: "C:\\test\\config"
            """);

        DiscoveryResult result = await svc.DiscoverAsync(_tempDir);

        Assert.Multiple(() =>
        {
            Assert.That(result.Modules, Has.Length.EqualTo(1));
            Assert.That(result.Errors, Has.Length.EqualTo(1));
            Assert.That(result.Errors[0], Does.Contain("broken"));
            Assert.That(result.Errors[0], Does.Contain("Network error"));
        });
    }

    [Test]
    public async Task DiscoverAsync_GalleryOnly_NoLinks_StillDiscoversModule()
    {
        var catalogService = Substitute.For<ICatalogService>();
        var overlayService = Substitute.For<IGalleryOverlayService>();
        var galleryEntry = new CatalogEntry("myapp", "myapp", null, "tools",
            ImmutableArray<string>.Empty, null, null, null, null, null, null);
        catalogService.GetAppAsync("myapp", Arg.Any<CancellationToken>()).Returns(galleryEntry);
        overlayService.Merge(Arg.Any<AppManifest>(), galleryEntry).Returns(call => call.Arg<AppManifest>());

        var svc = new ModuleDiscoveryService(new ManifestParser(), catalogService, overlayService);
        CreateModule("myapp", "gallery: myapp");

        DiscoveryResult result = await svc.DiscoverAsync(_tempDir);

        Assert.Multiple(() =>
        {
            Assert.That(result.Modules, Has.Length.EqualTo(1));
            Assert.That(result.Errors, Is.Empty);
        });
    }

    private void CreateModule(string name, string manifestYaml)
    {
        string moduleDir = Path.Combine(_tempDir, name);
        Directory.CreateDirectory(moduleDir);
        File.WriteAllText(Path.Combine(moduleDir, "manifest.yaml"), manifestYaml);
    }
}
