using Perch.Cli.Commands;

namespace Perch.Core.Tests.Cli;

[TestFixture]
public sealed class CompletionScriptTests
{
    [Test]
    public void BashScript_ContainsCompletionFunction()
    {
        Assert.Multiple(() =>
        {
            Assert.That(CompletionCommand.BashScript, Does.Contain("complete -F"));
            Assert.That(CompletionCommand.BashScript, Does.Contain("_perch_completions"));
            Assert.That(CompletionCommand.BashScript, Does.Contain("deploy"));
            Assert.That(CompletionCommand.BashScript, Does.Contain("--interactive"));
        });
    }

    [Test]
    public void ZshScript_ContainsCompdefDirective()
    {
        Assert.Multiple(() =>
        {
            Assert.That(CompletionCommand.ZshScript, Does.Contain("#compdef perch"));
            Assert.That(CompletionCommand.ZshScript, Does.Contain("deploy"));
            Assert.That(CompletionCommand.ZshScript, Does.Contain("--interactive"));
        });
    }

    [Test]
    public void PowerShellScript_ContainsArgumentCompleter()
    {
        Assert.Multiple(() =>
        {
            Assert.That(CompletionCommand.PowerShellScript, Does.Contain("Register-ArgumentCompleter"));
            Assert.That(CompletionCommand.PowerShellScript, Does.Contain("deploy"));
            Assert.That(CompletionCommand.PowerShellScript, Does.Contain("--interactive"));
        });
    }

    [Test]
    public void AllScripts_ContainAllCommands()
    {
        string[] expectedCommands = ["deploy", "status", "apps", "git", "diff", "completion"];

        foreach (string command in expectedCommands)
        {
            Assert.Multiple(() =>
            {
                Assert.That(CompletionCommand.BashScript, Does.Contain(command), $"bash missing '{command}'");
                Assert.That(CompletionCommand.ZshScript, Does.Contain(command), $"zsh missing '{command}'");
                Assert.That(CompletionCommand.PowerShellScript, Does.Contain(command), $"powershell missing '{command}'");
            });
        }
    }
}
