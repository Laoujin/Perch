using System.Runtime.Versioning;
using Perch.Core.Symlinks;

namespace Perch.Core.Tests.Symlinks;

[TestFixture]
[Platform("Win")]
[SupportedOSPlatform("windows")]
public sealed class WindowsSymlinkProviderTests
{
    private string _tempDir = null!;
    private WindowsSymlinkProvider _provider = null!;

    private static bool IsElevated => CanCreateSymlinks();

    private static bool CanCreateSymlinks()
    {
        string testDir = Path.Combine(Path.GetTempPath(), $"perch-symtest-{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDir);
        try
        {
            string target = Path.Combine(testDir, "target.txt");
            File.WriteAllText(target, "test");
            string link = Path.Combine(testDir, "link.txt");
            File.CreateSymbolicLink(link, target);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"perch-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _provider = new WindowsSymlinkProvider();
    }

    [TearDown]
    public void TearDown()
    {
        if (!Directory.Exists(_tempDir))
        {
            return;
        }

        foreach (string dir in Directory.GetDirectories(_tempDir))
        {
            var info = new DirectoryInfo(dir);
            if (info.LinkTarget != null)
            {
                info.Delete();
            }
        }

        Directory.Delete(_tempDir, recursive: true);
    }

    [Test]
    public void CreateSymlink_File_CreatesSymbolicLink()
    {
        if (!IsElevated)
        {
            Assert.Ignore("Requires elevated privileges to create symlinks.");
        }

        string targetFile = Path.Combine(_tempDir, "target.txt");
        File.WriteAllText(targetFile, "content");
        string linkPath = Path.Combine(_tempDir, "link.txt");

        _provider.CreateSymlink(linkPath, targetFile);

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(linkPath), Is.True);
            Assert.That(_provider.IsSymlink(linkPath), Is.True);
            Assert.That(File.ReadAllText(linkPath), Is.EqualTo("content"));
        });
    }

    [Test]
    public void CreateJunction_Directory_CreatesDirectoryLink()
    {
        string targetDir = Path.Combine(_tempDir, "targetdir");
        Directory.CreateDirectory(targetDir);
        File.WriteAllText(Path.Combine(targetDir, "file.txt"), "content");
        string linkPath = Path.Combine(_tempDir, "linkdir");

        _provider.CreateJunction(linkPath, targetDir);

        Assert.Multiple(() =>
        {
            Assert.That(Directory.Exists(linkPath), Is.True);
            Assert.That(_provider.IsSymlink(linkPath), Is.True);
            Assert.That(File.Exists(Path.Combine(linkPath, "file.txt")), Is.True);
        });
    }

    [Test]
    public void IsSymlink_RegularFile_ReturnsFalse()
    {
        string file = Path.Combine(_tempDir, "regular.txt");
        File.WriteAllText(file, "content");

        Assert.That(_provider.IsSymlink(file), Is.False);
    }

    [Test]
    public void GetSymlinkTarget_Symlink_ReturnsTarget()
    {
        if (!IsElevated)
        {
            Assert.Ignore("Requires elevated privileges to create symlinks.");
        }

        string targetFile = Path.Combine(_tempDir, "target.txt");
        File.WriteAllText(targetFile, "content");
        string linkPath = Path.Combine(_tempDir, "link.txt");
        _provider.CreateSymlink(linkPath, targetFile);

        string? result = _provider.GetSymlinkTarget(linkPath);

        Assert.That(result, Is.EqualTo(targetFile));
    }
}
