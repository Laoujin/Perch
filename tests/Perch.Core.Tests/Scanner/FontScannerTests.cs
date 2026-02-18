using Perch.Core.Scanner;

namespace Perch.Core.Tests.Scanner;

[TestFixture]
public sealed class FontScannerTests
{
    [TestCase("Arial", "Arial")]
    [TestCase("Arial Bold", "Arial")]
    [TestCase("Arial Bold Italic", "Arial")]
    [TestCase("Rockwell Condensed", "Rockwell")]
    [TestCase("Rockwell Condensed Bold", "Rockwell")]
    [TestCase("Cascadia Code SemiBold", "Cascadia Code")]
    [TestCase("JetBrains Mono ExtraBold Italic", "JetBrains Mono")]
    [TestCase("Segoe UI", "Segoe UI")]
    [TestCase("Segoe UI Light", "Segoe UI")]
    [TestCase("Bold", "Bold")]
    [TestCase("Consolas", "Consolas")]
    [TestCase("FiraCode-Regular", "FiraCode")]
    [TestCase("FiraCode-Bold", "FiraCode")]
    [TestCase("FiraCode-SemiBold", "FiraCode")]
    [TestCase("JetBrainsMono-ExtraBold", "JetBrainsMono")]
    [TestCase("CascadiaCode-SemiLight", "CascadiaCode")]
    public void ExtractFamilyName_StripsStyleSuffixes(string displayName, string expectedFamily)
    {
        var result = FontScanner.ExtractFamilyName(displayName);
        Assert.That(result, Is.EqualTo(expectedFamily));
    }
}
