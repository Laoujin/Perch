using System.Collections.Immutable;
using Perch.Core.Fonts;

namespace Perch.Core.Tests.Fonts;

[TestFixture]
public sealed class FontManifestParserTests
{
    private FontManifestParser _parser = null!;

    [SetUp]
    public void SetUp()
    {
        _parser = new FontManifestParser();
    }

    [Test]
    public void Parse_EmptyYaml_ReturnsEmptyList()
    {
        var result = _parser.Parse("");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.FontIds, Is.Empty);
        });
    }

    [Test]
    public void Parse_WhitespaceOnly_ReturnsEmptyList()
    {
        var result = _parser.Parse("   \n  ");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.FontIds, Is.Empty);
        });
    }

    [Test]
    public void Parse_ValidList_ReturnsFontIds()
    {
        string yaml = """
            - cascadia-code-nf
            - jetbrains-mono-nf
            - meslo-nf
            """;

        var result = _parser.Parse(yaml);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.FontIds, Has.Length.EqualTo(3));
            Assert.That(result.FontIds[0], Is.EqualTo("cascadia-code-nf"));
            Assert.That(result.FontIds[1], Is.EqualTo("jetbrains-mono-nf"));
            Assert.That(result.FontIds[2], Is.EqualTo("meslo-nf"));
        });
    }

    [Test]
    public void Parse_InvalidYaml_ReturnsFailure()
    {
        string yaml = "{ invalid: [yaml";

        var result = _parser.Parse(yaml);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Does.Contain("Invalid fonts YAML"));
        });
    }

    [Test]
    public void Parse_WhitespaceEntries_AreTrimmedAndFiltered()
    {
        string yaml = """
            - cascadia-code-nf
            -
            - " jetbrains-mono-nf "
            """;

        var result = _parser.Parse(yaml);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.FontIds, Has.Length.EqualTo(2));
            Assert.That(result.FontIds[0], Is.EqualTo("cascadia-code-nf"));
        });
    }
}
