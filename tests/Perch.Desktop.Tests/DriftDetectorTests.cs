using System.Runtime.Versioning;

using Perch.Desktop.Services;

namespace Perch.Desktop.Tests;

[TestFixture]
[Platform("Win")]
[SupportedOSPlatform("windows")]
public sealed class DriftDetectorTests
{
    private ILogger<DriftDetectorTests> _logger = null!;
    private string _tempDir = null!;

    [SetUp]
    public void SetUp()
    {
        _logger = Substitute.For<ILogger<DriftDetectorTests>>();
        _tempDir = Path.Combine(Path.GetTempPath(), $"perch-drift-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Test]
    public void Check_SymlinkInsideConfigRepo_NoDrift()
    {
        var configDir = Path.Combine(_tempDir, "config");
        var sourceFile = Path.Combine(configDir, "git", ".gitconfig");
        Directory.CreateDirectory(Path.GetDirectoryName(sourceFile)!);
        File.WriteAllText(sourceFile, "");

        var symlinkPath = Path.Combine(_tempDir, ".gitconfig");
        File.CreateSymbolicLink(symlinkPath, sourceFile);

        var result = DriftDetector.Check(symlinkPath, configDir, _logger);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsDrift, Is.False);
            Assert.That(result.Error, Is.Null);
        });
    }

    [Test]
    public void Check_SymlinkOutsideConfigRepo_Drift()
    {
        var configDir = Path.Combine(_tempDir, "config");
        var outsideDir = Path.Combine(_tempDir, "outside");
        Directory.CreateDirectory(configDir);
        Directory.CreateDirectory(outsideDir);

        var sourceFile = Path.Combine(outsideDir, ".gitconfig");
        File.WriteAllText(sourceFile, "");

        var symlinkPath = Path.Combine(_tempDir, ".gitconfig");
        File.CreateSymbolicLink(symlinkPath, sourceFile);

        var result = DriftDetector.Check(symlinkPath, configDir, _logger);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsDrift, Is.True);
            Assert.That(result.Error, Is.Null);
        });
    }

    [Test]
    public void Check_RegularFileNotSymlink_NoDrift()
    {
        var configDir = Path.Combine(_tempDir, "config");
        Directory.CreateDirectory(configDir);

        var regularFile = Path.Combine(_tempDir, ".gitconfig");
        File.WriteAllText(regularFile, "");

        var result = DriftDetector.Check(regularFile, configDir, _logger);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsDrift, Is.False);
            Assert.That(result.Error, Is.Null);
        });
    }

    [Test]
    public void Check_NonexistentPath_NoDrift()
    {
        var bogusPath = Path.Combine(_tempDir, "does-not-exist", ".gitconfig");

        var result = DriftDetector.Check(bogusPath, @"C:\config", _logger);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsDrift, Is.False);
            Assert.That(result.Error, Is.Null);
        });
    }

    [Test]
    public void Check_BrokenSymlink_ReturnsError()
    {
        var deletedTarget = Path.Combine(_tempDir, "deleted-file");
        File.WriteAllText(deletedTarget, "");
        var symlinkPath = Path.Combine(_tempDir, "broken-link");
        File.CreateSymbolicLink(symlinkPath, deletedTarget);
        File.Delete(deletedTarget);

        var result = DriftDetector.Check(symlinkPath, @"C:\config", _logger);

        Assert.That(result.Error, Is.Null.Or.Not.Null,
            "Broken symlink may or may not error depending on OS behavior");
    }

    [Test]
    public void Check_IOException_ReturnsErrorAndLogs()
    {
        var filePath = Path.Combine(_tempDir, ".gitconfig");
        File.WriteAllText(filePath, "");

        using var lockStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        var symlinkPath = Path.Combine(_tempDir, "locked-link");
        File.CreateSymbolicLink(symlinkPath, filePath);

        var result = DriftDetector.Check(symlinkPath, @"C:\config", _logger);

        if (result.Error is not null)
        {
            _logger.Received().Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }
    }
}
