using System.Collections.Immutable;

using Perch.Core.Fonts;

namespace Perch.Core.Tests.Fonts;

[TestFixture]
public sealed class FontOnboardingServiceTests
{
    private string _tempDir = null!;
    private string _configDir = null!;
    private FontOnboardingService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "perch-font-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);

        _configDir = Path.Combine(_tempDir, "config");
        Directory.CreateDirectory(_configDir);

        _service = new FontOnboardingService();
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Test]
    public async Task OnboardAsync_CopiesFilesToFontsDir()
    {
        var src = CreateTempFont("MyFont.ttf", "font-data");

        var result = await _service.OnboardAsync([src], _configDir);

        Assert.That(result.CopiedFiles, Has.Length.EqualTo(1));
        Assert.That(result.CopiedFiles[0], Is.EqualTo("MyFont.ttf"));
        Assert.That(File.Exists(Path.Combine(_configDir, "fonts", "MyFont.ttf")), Is.True);
    }

    [Test]
    public async Task OnboardAsync_CreatesFontsDirIfMissing()
    {
        var fontsDir = Path.Combine(_configDir, "fonts");
        Assert.That(Directory.Exists(fontsDir), Is.False);

        var src = CreateTempFont("Test.otf", "data");
        await _service.OnboardAsync([src], _configDir);

        Assert.That(Directory.Exists(fontsDir), Is.True);
    }

    [Test]
    public async Task OnboardAsync_SkipsIdenticalFiles()
    {
        var src = CreateTempFont("Same.ttf", "identical-bytes");

        var fontsDir = Path.Combine(_configDir, "fonts");
        Directory.CreateDirectory(fontsDir);
        File.WriteAllText(Path.Combine(fontsDir, "Same.ttf"), "identical-bytes");

        var result = await _service.OnboardAsync([src], _configDir);

        Assert.That(result.CopiedFiles, Is.Empty);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public async Task OnboardAsync_OverwritesDifferentFile()
    {
        var src = CreateTempFont("Update.ttf", "new-data");

        var fontsDir = Path.Combine(_configDir, "fonts");
        Directory.CreateDirectory(fontsDir);
        File.WriteAllText(Path.Combine(fontsDir, "Update.ttf"), "old-data");

        var result = await _service.OnboardAsync([src], _configDir);

        Assert.That(result.CopiedFiles, Has.Length.EqualTo(1));
        Assert.That(File.ReadAllText(Path.Combine(fontsDir, "Update.ttf")), Is.EqualTo("new-data"));
    }

    [Test]
    public async Task OnboardAsync_RecordsErrorsForMissingFiles()
    {
        var missing = Path.Combine(_tempDir, "nonexistent.ttf");

        var result = await _service.OnboardAsync([missing], _configDir);

        Assert.That(result.Errors, Has.Length.EqualTo(1));
        Assert.That(result.CopiedFiles, Is.Empty);
    }

    private string CreateTempFont(string name, string content)
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content);
        return path;
    }
}
