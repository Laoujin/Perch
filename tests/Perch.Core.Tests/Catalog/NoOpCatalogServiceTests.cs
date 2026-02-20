using Perch.Core.Catalog;

namespace Perch.Core.Tests.Catalog;

[TestFixture]
public sealed class NoOpCatalogServiceTests
{
    private NoOpCatalogService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new NoOpCatalogService();
    }

    [Test]
    public async Task GetIndexAsync_ReturnsEmptyIndex()
    {
        var index = await _service.GetIndexAsync();

        Assert.Multiple(() =>
        {
            Assert.That(index.Apps, Is.Empty);
            Assert.That(index.Fonts, Is.Empty);
            Assert.That(index.Tweaks, Is.Empty);
        });
    }

    [Test]
    public async Task GetAppAsync_ReturnsNull()
    {
        var result = await _service.GetAppAsync("any-id");
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetFontAsync_ReturnsNull()
    {
        var result = await _service.GetFontAsync("any-id");
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetTweakAsync_ReturnsNull()
    {
        var result = await _service.GetTweakAsync("any-id");
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAllAppsAsync_ReturnsEmpty()
    {
        var result = await _service.GetAllAppsAsync();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetAllFontsAsync_ReturnsEmpty()
    {
        var result = await _service.GetAllFontsAsync();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetAllTweaksAsync_ReturnsEmpty()
    {
        var result = await _service.GetAllTweaksAsync();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetAllDotfileAppsAsync_ReturnsEmpty()
    {
        var result = await _service.GetAllDotfileAppsAsync();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetAllAppOwnedTweaksAsync_ReturnsEmpty()
    {
        var result = await _service.GetAllAppOwnedTweaksAsync();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetGitHubStarsAsync_ReturnsEmptyDictionary()
    {
        var result = await _service.GetGitHubStarsAsync();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void InvalidateAll_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _service.InvalidateAll());
    }
}
