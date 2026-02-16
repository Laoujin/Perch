using Perch.Core.Config;

namespace Perch.Core.Tests.Config;

[TestFixture]
public sealed class YamlSettingsProviderTests
{
    private string _tempDir = null!;
    private string _settingsPath = null!;
    private YamlSettingsProvider _provider = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"perch-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _settingsPath = Path.Combine(_tempDir, "settings.yaml");
        _provider = new YamlSettingsProvider(_settingsPath);
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
    public async Task LoadAsync_FileMissing_ReturnsDefaults()
    {
        PerchSettings result = await _provider.LoadAsync();

        Assert.That(result.ConfigRepoPath, Is.Null);
    }

    [Test]
    public async Task SaveAsync_CreatesDirectoryAndFile()
    {
        string nestedDir = Path.Combine(_tempDir, "sub", "dir");
        string nestedPath = Path.Combine(nestedDir, "settings.yaml");
        var provider = new YamlSettingsProvider(nestedPath);
        var settings = new PerchSettings { ConfigRepoPath = "C:\\config" };

        await provider.SaveAsync(settings);

        Assert.That(File.Exists(nestedPath), Is.True);
    }

    [Test]
    public async Task Roundtrip_SaveAndLoad_PreservesValues()
    {
        var settings = new PerchSettings { ConfigRepoPath = "C:\\my\\config" };

        await _provider.SaveAsync(settings);
        PerchSettings loaded = await _provider.LoadAsync();

        Assert.That(loaded.ConfigRepoPath, Is.EqualTo("C:\\my\\config"));
    }

    [Test]
    public async Task LoadAsync_InvalidYaml_ReturnsDefaults()
    {
        await File.WriteAllTextAsync(_settingsPath, "{{invalid yaml::");

        PerchSettings result = await _provider.LoadAsync();

        Assert.That(result.ConfigRepoPath, Is.Null);
    }

    [Test]
    public async Task LoadAsync_NoGallerySettings_ReturnsDefaults()
    {
        PerchSettings result = await _provider.LoadAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.GalleryUrl, Is.EqualTo("https://laoujin.github.io/perch-gallery/"));
            Assert.That(result.GalleryLocalPath, Is.Null);
        });
    }

    [Test]
    public async Task Roundtrip_GallerySettings_PreservesValues()
    {
        var settings = new PerchSettings
        {
            ConfigRepoPath = "C:\\config",
            GalleryUrl = "https://custom.gallery/",
            GalleryLocalPath = "C:\\gallery-local"
        };

        await _provider.SaveAsync(settings);
        PerchSettings loaded = await _provider.LoadAsync();

        Assert.Multiple(() =>
        {
            Assert.That(loaded.GalleryUrl, Is.EqualTo("https://custom.gallery/"));
            Assert.That(loaded.GalleryLocalPath, Is.EqualTo("C:\\gallery-local"));
        });
    }

    [Test]
    public async Task LoadAsync_GalleryLocalPathOnly_UrlKeepsDefault()
    {
        await File.WriteAllTextAsync(_settingsPath, "gallery-local-path: C:\\local-gallery\n");

        PerchSettings result = await _provider.LoadAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.GalleryUrl, Is.EqualTo("https://laoujin.github.io/perch-gallery/"));
            Assert.That(result.GalleryLocalPath, Is.EqualTo("C:\\local-gallery"));
        });
    }
}
