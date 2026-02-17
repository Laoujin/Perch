using Perch.Core.Scanner;

namespace Perch.Core.Tests.Scanner;

[TestFixture]
public sealed class DefaultFontFamiliesTests
{
    [TestCase("arial")]
    [TestCase("calibri")]
    [TestCase("consola")]
    [TestCase("segoeui")]
    [TestCase("times")]
    [TestCase("verdana")]
    [TestCase("tahoma")]
    [TestCase("comic")]
    [TestCase("impact")]
    [TestCase("georgia")]
    public void IsDefault_KnownDefaults_ReturnsTrue(string stem)
    {
        Assert.That(DefaultFontFamilies.IsDefault(stem), Is.True);
    }

    [TestCase("Arial")]
    [TestCase("CONSOLA")]
    [TestCase("Segoeui")]
    public void IsDefault_CaseInsensitive(string stem)
    {
        Assert.That(DefaultFontFamilies.IsDefault(stem), Is.True);
    }

    [TestCase("CascadiaCode")]
    [TestCase("FiraCode-Regular")]
    [TestCase("JetBrainsMono-Regular")]
    [TestCase("Hack-Regular")]
    [TestCase("NerdFont")]
    public void IsDefault_NerdAndThirdPartyFonts_ReturnsFalse(string stem)
    {
        Assert.That(DefaultFontFamilies.IsDefault(stem), Is.False);
    }

    [TestCase("")]
    [TestCase(" ")]
    public void IsDefault_EmptyOrWhitespace_ReturnsFalse(string stem)
    {
        Assert.That(DefaultFontFamilies.IsDefault(stem), Is.False);
    }
}
