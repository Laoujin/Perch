using Perch.Core.Modules;

namespace Perch.Core.Tests.Modules;

[TestFixture]
public sealed class EnvironmentExpanderTests
{
    [Test]
    public void Expand_WindowsPercentSyntax_ExpandsVariable()
    {
        Environment.SetEnvironmentVariable("PERCH_TEST_VAR", "resolved");
        try
        {
            var result = EnvironmentExpander.Expand("%PERCH_TEST_VAR%\\subfolder");

            Assert.That(result, Is.EqualTo("resolved\\subfolder"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("PERCH_TEST_VAR", null);
        }
    }

    [Test]
    public void Expand_UnixDollarSyntax_ExpandsVariable()
    {
        Environment.SetEnvironmentVariable("PERCH_TEST_VAR", "resolved");
        try
        {
            var result = EnvironmentExpander.Expand("$PERCH_TEST_VAR/subfolder");

            Assert.That(result, Is.EqualTo("resolved/subfolder"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("PERCH_TEST_VAR", null);
        }
    }

    [Test]
    public void Expand_MultipleVariables_ExpandsAll()
    {
        Environment.SetEnvironmentVariable("PERCH_A", "first");
        Environment.SetEnvironmentVariable("PERCH_B", "second");
        try
        {
            var result = EnvironmentExpander.Expand("%PERCH_A%\\%PERCH_B%\\file");

            Assert.That(result, Is.EqualTo("first\\second\\file"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("PERCH_A", null);
            Environment.SetEnvironmentVariable("PERCH_B", null);
        }
    }

    [Test]
    public void Expand_UnknownVariable_LeavesUnchanged()
    {
        var result = EnvironmentExpander.Expand("%PERCH_NONEXISTENT_12345%\\file");

        Assert.That(result, Is.EqualTo("%PERCH_NONEXISTENT_12345%\\file"));
    }

    [Test]
    public void Expand_NoVariables_ReturnsOriginal()
    {
        var result = EnvironmentExpander.Expand("C:\\plain\\path\\file.txt");

        Assert.That(result, Is.EqualTo("C:\\plain\\path\\file.txt"));
    }

    [Test]
    public void Expand_UnknownDollarVariable_LeavesUnchanged()
    {
        var result = EnvironmentExpander.Expand("$PERCH_NONEXISTENT_12345/file");

        Assert.That(result, Is.EqualTo("$PERCH_NONEXISTENT_12345/file"));
    }
}
