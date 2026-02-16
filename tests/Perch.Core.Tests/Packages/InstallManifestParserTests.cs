using Perch.Core.Packages;

namespace Perch.Core.Tests.Packages;

[TestFixture]
public sealed class InstallManifestParserTests
{
    private InstallManifestParser _parser = null!;

    [SetUp]
    public void SetUp()
    {
        _parser = new InstallManifestParser();
    }

    [Test]
    public void Parse_ValidYaml_ReturnsManifest()
    {
        string yaml = """
            apps:
              - git
              - vscode
              - nvm
            """;

        var result = _parser.Parse(yaml);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Manifest!.Apps, Has.Length.EqualTo(3));
    }

    [Test]
    public void Parse_WithMachineOverrides_ParsesAddAndExclude()
    {
        string yaml = """
            apps:
              - git
              - vscode
              - docker
            machines:
              HOME-PC:
                add: [heidisql]
                exclude: [docker]
              WORK-PC:
                add: [docker]
            """;

        var result = _parser.Parse(yaml);

        Assert.That(result.IsSuccess, Is.True);
        var manifest = result.Manifest!;
        Assert.Multiple(() =>
        {
            Assert.That(manifest.Apps, Has.Length.EqualTo(3));
            Assert.That(manifest.Machines, Has.Count.EqualTo(2));
            Assert.That(manifest.Machines["HOME-PC"].Add, Has.Length.EqualTo(1));
            Assert.That(manifest.Machines["HOME-PC"].Exclude, Has.Length.EqualTo(1));
        });
    }

    [Test]
    public void Parse_EmptyYaml_ReturnsFailure()
    {
        var result = _parser.Parse("");

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public void Parse_NoApps_ReturnsEmptyList()
    {
        string yaml = """
            machines:
              PC:
                add: [git]
            """;

        var result = _parser.Parse(yaml);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Manifest!.Apps, Is.Empty);
    }
}
