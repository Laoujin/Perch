using Perch.Core.Backup;

namespace Perch.Core.Tests.Backup;

[TestFixture]
public sealed class FileBackupProviderTests
{
    private string _tempDir = null!;
    private FileBackupProvider _provider = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"perch-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _provider = new FileBackupProvider();
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
    public void BackupFile_ExistingFile_RenamesToBackup()
    {
        string file = Path.Combine(_tempDir, "settings.json");
        File.WriteAllText(file, "original");

        string backupPath = _provider.BackupFile(file);

        Assert.Multiple(() =>
        {
            Assert.That(backupPath, Is.EqualTo(file + ".backup"));
            Assert.That(File.Exists(file), Is.False);
            Assert.That(File.Exists(backupPath), Is.True);
            Assert.That(File.ReadAllText(backupPath), Is.EqualTo("original"));
        });
    }

    [Test]
    public void BackupFile_BackupAlreadyExists_IncrementsCounter()
    {
        string file = Path.Combine(_tempDir, "settings.json");
        File.WriteAllText(file, "original");
        File.WriteAllText(file + ".backup", "previous backup");

        string backupPath = _provider.BackupFile(file);

        Assert.Multiple(() =>
        {
            Assert.That(backupPath, Is.EqualTo(file + ".backup.1"));
            Assert.That(File.Exists(backupPath), Is.True);
            Assert.That(File.ReadAllText(backupPath), Is.EqualTo("original"));
            Assert.That(File.Exists(file + ".backup"), Is.True);
        });
    }

    [Test]
    public void BackupFile_MultipleBackupsExist_FindsNextAvailable()
    {
        string file = Path.Combine(_tempDir, "settings.json");
        File.WriteAllText(file, "original");
        File.WriteAllText(file + ".backup", "v1");
        File.WriteAllText(file + ".backup.1", "v2");

        string backupPath = _provider.BackupFile(file);

        Assert.That(backupPath, Is.EqualTo(file + ".backup.2"));
    }

    [Test]
    public void BackupFile_Directory_MovesDirectory()
    {
        string dir = Path.Combine(_tempDir, "mydir");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "file.txt"), "content");

        string backupPath = _provider.BackupFile(dir);

        Assert.Multiple(() =>
        {
            Assert.That(backupPath, Is.EqualTo(dir + ".backup"));
            Assert.That(Directory.Exists(dir), Is.False);
            Assert.That(Directory.Exists(backupPath), Is.True);
            Assert.That(File.Exists(Path.Combine(backupPath, "file.txt")), Is.True);
        });
    }

    [Test]
    public void BackupFile_Directory_BackupDirExists_IncrementsCounter()
    {
        string dir = Path.Combine(_tempDir, "mydir");
        Directory.CreateDirectory(dir);
        Directory.CreateDirectory(dir + ".backup");

        string backupPath = _provider.BackupFile(dir);

        Assert.That(backupPath, Is.EqualTo(dir + ".backup.1"));
    }
}
