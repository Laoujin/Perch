using Perch.Core.Registry;

namespace Perch.Core.Tests.Registry;

[TestFixture]
public sealed class YamlCapturedRegistryStoreTests
{
    private string _tempDir = null!;
    private string _filePath = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"perch-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _filePath = Path.Combine(_tempDir, "captured-registry.yaml");
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Test]
    public async Task Load_FileDoesNotExist_ReturnsEmptyData()
    {
        var store = new YamlCapturedRegistryStore(_filePath);

        var data = await store.LoadAsync();

        Assert.That(data.Entries, Is.Empty);
    }

    [Test]
    public async Task SaveAndLoad_RoundTrips()
    {
        var store = new YamlCapturedRegistryStore(_filePath);
        var capturedAt = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        var data = new CapturedRegistryData();
        data.Entries[@"HKCU\Software\Test\Value1"] = new CapturedRegistryEntry
        {
            Value = "1", Kind = RegistryValueType.DWord, CapturedAt = capturedAt,
        };
        data.Entries[@"HKCU\Software\Test\Value2"] = new CapturedRegistryEntry
        {
            Value = "hello", Kind = RegistryValueType.String, CapturedAt = capturedAt,
        };

        await store.SaveAsync(data);
        var loaded = await store.LoadAsync();

        Assert.Multiple(() =>
        {
            Assert.That(loaded.Entries, Has.Count.EqualTo(2));
            Assert.That(loaded.Entries.ContainsKey(@"HKCU\Software\Test\Value1"), Is.True);
            Assert.That(loaded.Entries.ContainsKey(@"HKCU\Software\Test\Value2"), Is.True);
            Assert.That(loaded.Entries[@"HKCU\Software\Test\Value1"].Value, Is.EqualTo("1"));
            Assert.That(loaded.Entries[@"HKCU\Software\Test\Value2"].Kind, Is.EqualTo(RegistryValueType.String));
        });
    }

    [Test]
    public async Task Save_CreatesDirectoryIfMissing()
    {
        var nestedPath = Path.Combine(_tempDir, "nested", "dir", "captured.yaml");
        var store = new YamlCapturedRegistryStore(nestedPath);
        var data = new CapturedRegistryData();

        await store.SaveAsync(data);

        Assert.That(File.Exists(nestedPath), Is.True);
    }

    [Test]
    public async Task Load_CorruptFile_ReturnsEmptyData()
    {
        await File.WriteAllTextAsync(_filePath, "{{{{not valid yaml");
        var store = new YamlCapturedRegistryStore(_filePath);

        var data = await store.LoadAsync();

        Assert.That(data.Entries, Is.Empty);
    }

    [Test]
    public async Task Save_OverwritesExistingFile()
    {
        var store = new YamlCapturedRegistryStore(_filePath);
        var capturedAt = DateTime.UtcNow;

        var first = new CapturedRegistryData();
        first.Entries["key1"] = new CapturedRegistryEntry
        {
            Value = "1", Kind = RegistryValueType.DWord, CapturedAt = capturedAt,
        };
        await store.SaveAsync(first);

        var second = new CapturedRegistryData();
        second.Entries["key2"] = new CapturedRegistryEntry
        {
            Value = "val", Kind = RegistryValueType.String, CapturedAt = capturedAt,
        };
        await store.SaveAsync(second);

        var loaded = await store.LoadAsync();
        Assert.Multiple(() =>
        {
            Assert.That(loaded.Entries, Has.Count.EqualTo(1));
            Assert.That(loaded.Entries.ContainsKey("key2"), Is.True);
        });
    }
}
