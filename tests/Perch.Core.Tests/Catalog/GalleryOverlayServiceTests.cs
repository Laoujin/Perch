using System.Collections.Immutable;

using Perch.Core;
using Perch.Core.Catalog;
using Perch.Core.Git;
using Perch.Core.Modules;

namespace Perch.Core.Tests.Catalog;

[TestFixture]
public sealed class GalleryOverlayServiceTests
{
    private GalleryOverlayService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new GalleryOverlayService();
    }

    private static AppManifest CreateManifest(
        string name = "testmod",
        ImmutableArray<LinkEntry> links = default,
        CleanFilterDefinition? cleanFilter = null,
        ImmutableArray<string> vscodeExtensions = default,
        string? galleryId = "testapp") =>
        new(name, name, true, ImmutableArray<Platform>.Empty, links,
            GalleryId: galleryId, CleanFilter: cleanFilter, VscodeExtensions: vscodeExtensions);

    private static CatalogEntry CreateGallery(
        string id = "testapp",
        CatalogConfigDefinition? config = null,
        CatalogExtensions? extensions = null) =>
        new(id, "Test App", null, "Test", ImmutableArray<string>.Empty, null, null, null, null, config, extensions);

    [Test]
    public void Merge_GalleryCleanFilter_AppliedWhenManifestHasNone()
    {
        var manifest = CreateManifest();
        var filter = new CatalogCleanFilter(
            ImmutableArray.Create("config.xml"),
            ImmutableArray.Create(new FilterRule("strip-xml-elements", ImmutableArray.Create("FindHistory"))));
        var gallery = CreateGallery(config: new CatalogConfigDefinition(ImmutableArray<CatalogConfigLink>.Empty, filter));

        var result = _service.Merge(manifest, gallery);

        Assert.That(result.CleanFilter, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.CleanFilter!.Name, Is.EqualTo("testapp-clean"));
            Assert.That(result.CleanFilter.Files, Has.Length.EqualTo(1));
            Assert.That(result.CleanFilter.Rules, Has.Length.EqualTo(1));
        });
    }

    [Test]
    public void Merge_ManifestCleanFilter_TakesPrecedence()
    {
        var manifestFilter = new CleanFilterDefinition("custom-clean", null,
            ImmutableArray.Create("other.xml"),
            ImmutableArray.Create(new FilterRule("strip-ini-keys", ImmutableArray.Create("key1"))));
        var manifest = CreateManifest(cleanFilter: manifestFilter);
        var galleryFilter = new CatalogCleanFilter(
            ImmutableArray.Create("config.xml"),
            ImmutableArray.Create(new FilterRule("strip-xml-elements", ImmutableArray.Create("History"))));
        var gallery = CreateGallery(config: new CatalogConfigDefinition(ImmutableArray<CatalogConfigLink>.Empty, galleryFilter));

        var result = _service.Merge(manifest, gallery);

        Assert.That(result.CleanFilter!.Name, Is.EqualTo("custom-clean"));
    }

    [Test]
    public void Merge_Extensions_Combined()
    {
        var manifest = CreateManifest(vscodeExtensions: ImmutableArray.Create("ext.a", "ext.b"));
        var gallery = CreateGallery(extensions: new CatalogExtensions(
            ImmutableArray.Create("ext.b", "ext.c"),
            ImmutableArray.Create("ext.d")));

        var result = _service.Merge(manifest, gallery);

        Assert.That(result.VscodeExtensions, Has.Length.EqualTo(4));
        Assert.That(result.VscodeExtensions, Does.Contain("ext.a"));
        Assert.That(result.VscodeExtensions, Does.Contain("ext.b"));
        Assert.That(result.VscodeExtensions, Does.Contain("ext.c"));
        Assert.That(result.VscodeExtensions, Does.Contain("ext.d"));
    }

    [Test]
    public void Merge_GalleryLinks_AddedWhenManifestDoesNotDefine()
    {
        var manifest = CreateManifest();
        var galleryLinks = ImmutableArray.Create(
            new CatalogConfigLink("settings.json",
                new Dictionary<Platform, string> { [Platform.Windows] = "%APPDATA%/Test/settings.json" }.ToImmutableDictionary()));
        var gallery = CreateGallery(config: new CatalogConfigDefinition(galleryLinks));

        var result = _service.Merge(manifest, gallery);

        Assert.That(result.Links, Has.Length.EqualTo(1));
        Assert.That(result.Links[0].Source, Is.EqualTo("settings.json"));
    }

    [Test]
    public void Merge_ManifestLinkOverridesGalleryBySource()
    {
        var manifestLinks = ImmutableArray.Create(
            new LinkEntry("settings.json", "%CUSTOM%/settings.json", LinkType.Symlink));
        var manifest = CreateManifest(links: manifestLinks);
        var galleryLinks = ImmutableArray.Create(
            new CatalogConfigLink("settings.json",
                new Dictionary<Platform, string> { [Platform.Windows] = "%APPDATA%/Test/settings.json" }.ToImmutableDictionary()),
            new CatalogConfigLink("keybindings.json",
                new Dictionary<Platform, string> { [Platform.Windows] = "%APPDATA%/Test/keybindings.json" }.ToImmutableDictionary()));
        var gallery = CreateGallery(config: new CatalogConfigDefinition(galleryLinks));

        var result = _service.Merge(manifest, gallery);

        Assert.That(result.Links, Has.Length.EqualTo(2));
        Assert.That(result.Links[0].Target, Is.EqualTo("%CUSTOM%/settings.json"));
        Assert.That(result.Links[1].Source, Is.EqualTo("keybindings.json"));
    }

    [Test]
    public void Merge_NoGalleryConfig_ManifestUnchanged()
    {
        var manifestLinks = ImmutableArray.Create(
            new LinkEntry("test.conf", "/home/test", LinkType.Symlink));
        var manifest = CreateManifest(links: manifestLinks);
        var gallery = CreateGallery();

        var result = _service.Merge(manifest, gallery);

        Assert.That(result.Links, Has.Length.EqualTo(1));
        Assert.That(result.CleanFilter, Is.Null);
    }

    [Test]
    public void Merge_GalleryDisplayName_UsedWhenManifestIsDefault()
    {
        var manifest = CreateManifest(name: "vscode");
        var gallery = CreateGallery(id: "vscode") with { DisplayName = "VS Code" };

        var result = _service.Merge(manifest, gallery);

        Assert.That(result.DisplayName, Is.EqualTo("VS Code"));
    }

    [Test]
    public void Merge_ManifestDisplayName_TakesPrecedence()
    {
        var manifest = CreateManifest(name: "vscode") with { DisplayName = "My VS Code" };
        var gallery = CreateGallery(id: "vscode") with { DisplayName = "VS Code" };

        var result = _service.Merge(manifest, gallery);

        Assert.That(result.DisplayName, Is.EqualTo("My VS Code"));
    }
}
