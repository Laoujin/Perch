using Perch.Core.Catalog;

namespace Perch.Core.Tests.Catalog;

[TestFixture]
public sealed class LocalCatalogFetcherTests
{
    private string _tempDir = null!;
    private LocalCatalogFetcher _fetcher = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"perch-gallery-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _fetcher = new LocalCatalogFetcher(_tempDir);
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
    public async Task FetchAsync_ExistingFile_ReturnsContent()
    {
        string appsDir = Path.Combine(_tempDir, "apps");
        Directory.CreateDirectory(appsDir);
        await File.WriteAllTextAsync(Path.Combine(appsDir, "vscode.yaml"), "name: VS Code");

        string result = await _fetcher.FetchAsync("apps/vscode.yaml");

        Assert.That(result, Is.EqualTo("name: VS Code"));
    }

    [Test]
    public void FetchAsync_MissingFile_Throws()
    {
        Assert.That(async () => await _fetcher.FetchAsync("apps/nonexistent.yaml"),
            Throws.InstanceOf<IOException>());
    }

    [Test]
    public async Task FetchAsync_IndexFile_ReturnsContent()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "index.yaml"), "apps: []");

        string result = await _fetcher.FetchAsync("index.yaml");

        Assert.That(result, Is.EqualTo("apps: []"));
    }
}
