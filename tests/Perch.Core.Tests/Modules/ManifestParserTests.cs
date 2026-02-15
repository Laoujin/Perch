using Perch.Core.Modules;

namespace Perch.Core.Tests.Modules;

[TestFixture]
public sealed class ManifestParserTests
{
    private ManifestParser _parser = null!;

    [SetUp]
    public void SetUp()
    {
        _parser = new ManifestParser();
    }

    [Test]
    public void Parse_SingleLink_ReturnsManifestWithOneLink()
    {
        string yaml = """
            links:
              - source: settings.json
                target: "%APPDATA%\\Code\\User\\settings.json"
            """;

        var result = _parser.Parse(yaml, "vscode");

        Assert.That(result.IsSuccess, Is.True);
        var manifest = result.Manifest!;
        Assert.Multiple(() =>
        {
            Assert.That(manifest.ModuleName, Is.EqualTo("vscode"));
            Assert.That(manifest.DisplayName, Is.EqualTo("vscode"));
            Assert.That(manifest.Links, Has.Length.EqualTo(1));
            Assert.That(manifest.Links[0].Source, Is.EqualTo("settings.json"));
            Assert.That(manifest.Links[0].Target, Is.EqualTo("%APPDATA%\\Code\\User\\settings.json"));
            Assert.That(manifest.Links[0].LinkType, Is.EqualTo(LinkType.Symlink));
        });
    }

    [Test]
    public void Parse_MultipleLinks_ReturnsAllLinks()
    {
        string yaml = """
            links:
              - source: settings.json
                target: "%APPDATA%\\Code\\User\\settings.json"
              - source: keybindings.json
                target: "%APPDATA%\\Code\\User\\keybindings.json"
            """;

        var result = _parser.Parse(yaml, "vscode");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Manifest!.Links, Has.Length.EqualTo(2));
    }

    [Test]
    public void Parse_JunctionLinkType_ParsesCorrectly()
    {
        string yaml = """
            links:
              - source: data
                target: "C:\\ProgramData\\MyApp"
                link-type: junction
            """;

        var result = _parser.Parse(yaml, "myapp");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Manifest!.Links[0].LinkType, Is.EqualTo(LinkType.Junction));
    }

    [Test]
    public void Parse_WithDisplayName_UsesDisplayName()
    {
        string yaml = """
            display-name: Visual Studio Code
            links:
              - source: settings.json
                target: "%APPDATA%\\Code\\User\\settings.json"
            """;

        var result = _parser.Parse(yaml, "vscode");

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Manifest!.ModuleName, Is.EqualTo("vscode"));
            Assert.That(result.Manifest!.DisplayName, Is.EqualTo("Visual Studio Code"));
        });
    }

    [Test]
    public void Parse_DefaultLinkType_IsSymlink()
    {
        string yaml = """
            links:
              - source: config
                target: "C:\\Users\\test\\config"
            """;

        var result = _parser.Parse(yaml, "app");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Manifest!.Links[0].LinkType, Is.EqualTo(LinkType.Symlink));
    }

    [Test]
    public void Parse_InvalidYaml_ReturnsError()
    {
        string yaml = "{{invalid yaml::";

        var result = _parser.Parse(yaml, "bad");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void Parse_MissingLinks_ReturnsError()
    {
        string yaml = """
            display-name: No Links App
            """;

        var result = _parser.Parse(yaml, "nolinks");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Does.Contain("links"));
    }

    [Test]
    public void Parse_EmptyFile_ReturnsError()
    {
        string yaml = "";

        var result = _parser.Parse(yaml, "empty");

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public void Parse_LinkMissingSource_ReturnsError()
    {
        string yaml = """
            links:
              - target: "C:\\somewhere"
            """;

        var result = _parser.Parse(yaml, "badsource");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Does.Contain("source"));
    }

    [Test]
    public void Parse_LinkMissingTarget_ReturnsError()
    {
        string yaml = """
            links:
              - source: settings.json
            """;

        var result = _parser.Parse(yaml, "badtarget");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Does.Contain("target"));
    }
}
