using System.Collections.Immutable;

using NSubstitute;

using Perch.Core;
using Perch.Core.Catalog;
using Perch.Core.Packages;

namespace Perch.Core.Tests.Packages;

[TestFixture]
public sealed class InstallResolverTests
{
    private ICatalogService _catalogService = null!;
    private InstallResolver _resolver = null!;

    [SetUp]
    public void SetUp()
    {
        _catalogService = Substitute.For<ICatalogService>();
        _resolver = new InstallResolver(_catalogService);
    }

    private static CatalogEntry CreateApp(string id, string? winget = null, string? choco = null) =>
        new(id, id, null, "Test", ImmutableArray<string>.Empty, null, null, null,
            new InstallDefinition(winget, choco), null, null);

    private static InstallManifest CreateManifest(
        ImmutableArray<string> apps,
        ImmutableDictionary<string, MachineInstallOverrides>? machines = null) =>
        new(apps, machines ?? ImmutableDictionary<string, MachineInstallOverrides>.Empty);

    [Test]
    public async Task ResolveAsync_AppsWithWinget_ResolvesToPackages()
    {
        _catalogService.GetAppAsync("git", Arg.Any<CancellationToken>())
            .Returns(CreateApp("git", winget: "Git.Git"));
        _catalogService.GetAppAsync("vscode", Arg.Any<CancellationToken>())
            .Returns(CreateApp("vscode", winget: "Microsoft.VisualStudioCode"));

        var manifest = CreateManifest(ImmutableArray.Create("git", "vscode"));

        var resolution = await _resolver.ResolveAsync(manifest, "PC", Platform.Windows);

        Assert.That(resolution.Packages, Has.Length.EqualTo(2));
        Assert.That(resolution.Errors, Is.Empty);
    }

    [Test]
    public async Task ResolveAsync_MachineAdd_IncludesExtraApp()
    {
        _catalogService.GetAppAsync("git", Arg.Any<CancellationToken>())
            .Returns(CreateApp("git", winget: "Git.Git"));
        _catalogService.GetAppAsync("heidisql", Arg.Any<CancellationToken>())
            .Returns(CreateApp("heidisql", winget: "HeidiSQL.HeidiSQL"));

        var machines = ImmutableDictionary.CreateBuilder<string, MachineInstallOverrides>(StringComparer.OrdinalIgnoreCase);
        machines["HOME-PC"] = new MachineInstallOverrides(ImmutableArray.Create("heidisql"), ImmutableArray<string>.Empty);
        var manifest = CreateManifest(ImmutableArray.Create("git"), machines.ToImmutable());

        var resolution = await _resolver.ResolveAsync(manifest, "HOME-PC", Platform.Windows);

        Assert.That(resolution.Packages, Has.Length.EqualTo(2));
    }

    [Test]
    public async Task ResolveAsync_MachineExclude_RemovesApp()
    {
        _catalogService.GetAppAsync("git", Arg.Any<CancellationToken>())
            .Returns(CreateApp("git", winget: "Git.Git"));

        var machines = ImmutableDictionary.CreateBuilder<string, MachineInstallOverrides>(StringComparer.OrdinalIgnoreCase);
        machines["WORK-PC"] = new MachineInstallOverrides(ImmutableArray<string>.Empty, ImmutableArray.Create("docker"));
        var manifest = CreateManifest(ImmutableArray.Create("git", "docker"), machines.ToImmutable());

        var resolution = await _resolver.ResolveAsync(manifest, "WORK-PC", Platform.Windows);

        Assert.That(resolution.Packages, Has.Length.EqualTo(1));
        Assert.That(resolution.Packages[0].Name, Is.EqualTo("Git.Git"));
    }

    [Test]
    public async Task ResolveAsync_MissingGalleryEntry_ReportsError()
    {
        _catalogService.GetAppAsync("nonexistent", Arg.Any<CancellationToken>())
            .Returns((CatalogEntry?)null);

        var manifest = CreateManifest(ImmutableArray.Create("nonexistent"));

        var resolution = await _resolver.ResolveAsync(manifest, "PC", Platform.Windows);

        Assert.That(resolution.Packages, Is.Empty);
        Assert.That(resolution.Errors, Has.Length.EqualTo(1));
        Assert.That(resolution.Errors[0], Does.Contain("nonexistent"));
    }

    [Test]
    public async Task ResolveAsync_FallsBackToChoco_WhenNoWinget()
    {
        _catalogService.GetAppAsync("oldapp", Arg.Any<CancellationToken>())
            .Returns(CreateApp("oldapp", choco: "oldapp-choco"));

        var manifest = CreateManifest(ImmutableArray.Create("oldapp"));

        var resolution = await _resolver.ResolveAsync(manifest, "PC", Platform.Windows);

        Assert.That(resolution.Packages, Has.Length.EqualTo(1));
        Assert.That(resolution.Packages[0].Manager, Is.EqualTo(PackageManager.Chocolatey));
    }
}
