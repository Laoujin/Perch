using Perch.Core.Registry;
using Perch.Core.Modules;

namespace Perch.Core.Tests.Registry;

[TestFixture]
public sealed class RegistryEntryParsingTests
{
    private ManifestParser _parser = null!;

    [SetUp]
    public void SetUp()
    {
        _parser = new ManifestParser();
    }

    [Test]
    public void Parse_WithRegistry_ReturnsEntries()
    {
        string yaml = """
            links:
              - source: settings.json
                target: "C:\\target\\settings.json"
            registry:
              - key: "HKCU\\Software\\Test"
                name: DarkMode
                value: 1
                type: dword
            """;

        var result = _parser.Parse(yaml, "test");

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Manifest!.Registry, Has.Length.EqualTo(1));
            Assert.That(result.Manifest.Registry[0].Key, Is.EqualTo(@"HKCU\Software\Test"));
            Assert.That(result.Manifest.Registry[0].Name, Is.EqualTo("DarkMode"));
            Assert.That(result.Manifest.Registry[0].Value, Is.EqualTo(1));
            Assert.That(result.Manifest.Registry[0].Kind, Is.EqualTo(RegistryValueType.DWord));
        });
    }

    [Test]
    public void Parse_NoRegistry_ReturnsEmpty()
    {
        string yaml = """
            links:
              - source: settings.json
                target: "C:\\target\\settings.json"
            """;

        var result = _parser.Parse(yaml, "test");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Manifest!.Registry, Is.Empty);
    }

    [Test]
    public void Parse_RegistryTypes_ParsesCorrectly()
    {
        string yaml = """
            links:
              - source: f.txt
                target: "C:\\t\\f.txt"
            registry:
              - key: "HKCU\\Software\\Test"
                name: StringVal
                value: hello
                type: string
              - key: "HKCU\\Software\\Test"
                name: DwordVal
                value: 42
                type: dword
              - key: "HKCU\\Software\\Test"
                name: QwordVal
                value: 9999999999
                type: qword
              - key: "HKCU\\Software\\Test"
                name: ExpandVal
                value: "%USERPROFILE%\\test"
                type: expandstring
            """;

        var result = _parser.Parse(yaml, "test");

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Manifest!.Registry, Has.Length.EqualTo(4));
            Assert.That(result.Manifest.Registry[0].Kind, Is.EqualTo(RegistryValueType.String));
            Assert.That(result.Manifest.Registry[0].Value, Is.EqualTo("hello"));
            Assert.That(result.Manifest.Registry[1].Kind, Is.EqualTo(RegistryValueType.DWord));
            Assert.That(result.Manifest.Registry[1].Value, Is.EqualTo(42));
            Assert.That(result.Manifest.Registry[2].Kind, Is.EqualTo(RegistryValueType.QWord));
            Assert.That(result.Manifest.Registry[2].Value, Is.EqualTo(9999999999L));
            Assert.That(result.Manifest.Registry[3].Kind, Is.EqualTo(RegistryValueType.ExpandString));
        });
    }

    [Test]
    public void Parse_RegistryOnly_NoLinks_Succeeds()
    {
        string yaml = """
            registry:
              - key: "HKCU\\Software\\Test"
                name: Theme
                value: 0
                type: dword
            """;

        var result = _parser.Parse(yaml, "test");

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Manifest!.Links, Is.Empty);
            Assert.That(result.Manifest.Registry, Has.Length.EqualTo(1));
        });
    }
}
