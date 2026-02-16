using NSubstitute;

using Perch.Core.Catalog;
using Perch.Core.Config;

namespace Perch.Core.Tests.Catalog;

[TestFixture]
public sealed class SettingsAwareCatalogFetcherTests
{
    private string _tempDir = null!;
    private ISettingsProvider _settingsProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"perch-gallery-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _settingsProvider = Substitute.For<ISettingsProvider>();
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
    public async Task FetchAsync_WithLocalPath_ReadsFromDisk()
    {
        string appsDir = Path.Combine(_tempDir, "apps");
        Directory.CreateDirectory(appsDir);
        await File.WriteAllTextAsync(Path.Combine(appsDir, "test.yaml"), "name: TestApp");

        _settingsProvider.LoadAsync(Arg.Any<CancellationToken>())
            .Returns(new PerchSettings { GalleryLocalPath = _tempDir });

        var fetcher = new SettingsAwareCatalogFetcher(_settingsProvider, new System.Net.Http.HttpClient());

        string result = await fetcher.FetchAsync("apps/test.yaml");

        Assert.That(result, Is.EqualTo("name: TestApp"));
    }

    [Test]
    public async Task FetchAsync_LocalPathTakesPrecedenceOverUrl()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "index.yaml"), "apps: []");

        _settingsProvider.LoadAsync(Arg.Any<CancellationToken>())
            .Returns(new PerchSettings
            {
                GalleryUrl = "https://example.com/gallery/",
                GalleryLocalPath = _tempDir
            });

        var fetcher = new SettingsAwareCatalogFetcher(_settingsProvider, new System.Net.Http.HttpClient());

        string result = await fetcher.FetchAsync("index.yaml");

        Assert.That(result, Is.EqualTo("apps: []"));
    }
}
