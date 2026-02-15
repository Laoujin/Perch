using Perch.Core;
using Perch.Core.Catalog;
using Perch.Core.Registry;

namespace Perch.Core.Tests.Catalog;

[TestFixture]
public sealed class CatalogParserTests
{
    private CatalogParser _parser = null!;

    [SetUp]
    public void SetUp()
    {
        _parser = new CatalogParser();
    }

    [Test]
    public void ParseApp_ValidYaml_ReturnsEntry()
    {
        string yaml = """
            name: Visual Studio Code
            display-name: VS Code
            category: Development/IDEs
            tags: [editor, ide, microsoft]
            description: Lightweight but powerful source code editor
            install:
              winget: Microsoft.VisualStudio.Code
              choco: vscode
            config:
              links:
                - source: settings.json
                  target:
                    windows: "%APPDATA%/Code/User/settings.json"
                    linux: "$HOME/.config/Code/User/settings.json"
            extensions:
              bundled: []
              recommended:
                - dbaeumer.vscode-eslint
            """;

        var result = _parser.ParseApp(yaml, "vscode");

        Assert.That(result.IsSuccess, Is.True);
        var entry = result.Value!;
        Assert.Multiple(() =>
        {
            Assert.That(entry.Id, Is.EqualTo("vscode"));
            Assert.That(entry.Name, Is.EqualTo("Visual Studio Code"));
            Assert.That(entry.DisplayName, Is.EqualTo("VS Code"));
            Assert.That(entry.Category, Is.EqualTo("Development/IDEs"));
            Assert.That(entry.Tags, Has.Length.EqualTo(3));
            Assert.That(entry.Install!.Winget, Is.EqualTo("Microsoft.VisualStudio.Code"));
            Assert.That(entry.Install.Choco, Is.EqualTo("vscode"));
            Assert.That(entry.Config!.Links, Has.Length.EqualTo(1));
            Assert.That(entry.Config.Links[0].Targets[Platform.Windows], Is.EqualTo("%APPDATA%/Code/User/settings.json"));
            Assert.That(entry.Extensions!.Recommended, Has.Length.EqualTo(1));
        });
    }

    [Test]
    public void ParseApp_EmptyYaml_ReturnsFailure()
    {
        var result = _parser.ParseApp("", "test");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo("YAML content is empty."));
    }

    [Test]
    public void ParseApp_MissingName_ReturnsFailure()
    {
        string yaml = """
            category: Test
            """;

        var result = _parser.ParseApp(yaml, "test");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Does.Contain("missing 'name'"));
    }

    [Test]
    public void ParseFont_ValidYaml_ReturnsEntry()
    {
        string yaml = """
            name: JetBrains Mono Nerd Font
            category: Fonts/Programming
            tags: [monospace, nerd-font, ligatures]
            description: JetBrains Mono with Nerd Font patches
            preview-text: "fn main() { let x = 42; }"
            install:
              choco: nerd-fonts-jetbrainsmono
              winget: DEVCOM.JetBrainsMonoNerdFont
            """;

        var result = _parser.ParseFont(yaml, "jetbrains-mono-nf");

        Assert.That(result.IsSuccess, Is.True);
        var entry = result.Value!;
        Assert.Multiple(() =>
        {
            Assert.That(entry.Id, Is.EqualTo("jetbrains-mono-nf"));
            Assert.That(entry.Name, Is.EqualTo("JetBrains Mono Nerd Font"));
            Assert.That(entry.PreviewText, Is.EqualTo("fn main() { let x = 42; }"));
            Assert.That(entry.Install!.Choco, Is.EqualTo("nerd-fonts-jetbrainsmono"));
        });
    }

    [Test]
    public void ParseFont_EmptyYaml_ReturnsFailure()
    {
        var result = _parser.ParseFont("", "test");

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public void ParseTweak_ValidYaml_ReturnsEntry()
    {
        string yaml = """
            name: Show File Extensions
            category: Developer Settings
            tags: [explorer, files]
            description: Always show file name extensions in Explorer
            reversible: true
            profiles: [developer, power-user]
            priority: 1
            registry:
              - key: HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced
                name: HideFileExt
                value: 0
                type: dword
            """;

        var result = _parser.ParseTweak(yaml, "show-file-extensions");

        Assert.That(result.IsSuccess, Is.True);
        var entry = result.Value!;
        Assert.Multiple(() =>
        {
            Assert.That(entry.Id, Is.EqualTo("show-file-extensions"));
            Assert.That(entry.Name, Is.EqualTo("Show File Extensions"));
            Assert.That(entry.Reversible, Is.True);
            Assert.That(entry.Profiles, Has.Length.EqualTo(2));
            Assert.That(entry.Priority, Is.EqualTo(1));
            Assert.That(entry.Registry, Has.Length.EqualTo(1));
            Assert.That(entry.Registry[0].Name, Is.EqualTo("HideFileExt"));
            Assert.That(entry.Registry[0].Kind, Is.EqualTo(RegistryValueType.DWord));
        });
    }

    [Test]
    public void ParseIndex_ValidYaml_ReturnsIndex()
    {
        string yaml = """
            apps:
              - id: vscode
                name: Visual Studio Code
                category: Development
                tags: [editor]
              - id: firefox
                name: Firefox
                category: Browsers
                tags: [browser]
            fonts:
              - id: jetbrains-mono
                name: JetBrains Mono
                category: Fonts
                tags: [monospace]
            tweaks:
              - id: show-extensions
                name: Show File Extensions
                category: Developer
                tags: [explorer]
            """;

        var result = _parser.ParseIndex(yaml);

        Assert.That(result.IsSuccess, Is.True);
        var index = result.Value!;
        Assert.Multiple(() =>
        {
            Assert.That(index.Apps, Has.Length.EqualTo(2));
            Assert.That(index.Fonts, Has.Length.EqualTo(1));
            Assert.That(index.Tweaks, Has.Length.EqualTo(1));
            Assert.That(index.Apps[0].Id, Is.EqualTo("vscode"));
            Assert.That(index.Apps[0].Name, Is.EqualTo("Visual Studio Code"));
        });
    }

    [Test]
    public void ParseIndex_EmptyYaml_ReturnsFailure()
    {
        var result = _parser.ParseIndex("");

        Assert.That(result.IsSuccess, Is.False);
    }
}
