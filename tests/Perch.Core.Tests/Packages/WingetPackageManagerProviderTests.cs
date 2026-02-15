using System.ComponentModel;
using Perch.Core.Packages;

namespace Perch.Core.Tests.Packages;

[TestFixture]
public sealed class WingetPackageManagerProviderTests
{
    private IProcessRunner _processRunner = null!;
    private WingetPackageManagerProvider _provider = null!;

    [SetUp]
    public void SetUp()
    {
        _processRunner = Substitute.For<IProcessRunner>();
        _provider = new WingetPackageManagerProvider(_processRunner);
    }

    [Test]
    public async Task ScanInstalled_WingetAvailable_ParsesNamesAndIds()
    {
        string output =
            "Name            Id         Version Available Source\r\n" +
            "---------------------------------------------------\r\n" +
            "Git             Git.Git    2.42.0  2.53.0    winget\r\n" +
            "7-Zip 19 (x64)  7zip.7zip  19.00  26.00     winget\r\n";

        _processRunner.RunAsync("winget", "list --accept-source-agreements", cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new ProcessRunResult(0, output, ""));

        var result = await _provider.ScanInstalledAsync();

        Assert.That(result.IsAvailable, Is.True);
        string[] names = result.Packages.Select(p => p.Name).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(names, Does.Contain("Git"));
            Assert.That(names, Does.Contain("Git.Git"));
            Assert.That(names, Does.Contain("7-Zip 19 (x64)"));
            Assert.That(names, Does.Contain("7zip.7zip"));
            Assert.That(result.Packages.All(p => p.Source == PackageManager.Winget), Is.True);
        });
    }

    [Test]
    public async Task ScanInstalled_ArpEntries_ParsesBothNameAndId()
    {
        string output =
            "Name                  Id                                 Version\r\n" +
            "---------------------------------------------------------------------\r\n" +
            "Beyond Compare 3.3.13 ARP\\Machine\\X86\\BeyondCompare3_is1 3.3.13.18981\r\n";

        _processRunner.RunAsync("winget", "list --accept-source-agreements", cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new ProcessRunResult(0, output, ""));

        var result = await _provider.ScanInstalledAsync();

        Assert.That(result.IsAvailable, Is.True);
        string[] names = result.Packages.Select(p => p.Name).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(names, Does.Contain("Beyond Compare 3.3.13"));
            Assert.That(names, Does.Contain("ARP\\Machine\\X86\\BeyondCompare3_is1"));
        });
    }

    [Test]
    public async Task ScanInstalled_WingetNotInstalled_ReturnsUnavailable()
    {
        _processRunner.RunAsync("winget", "list --accept-source-agreements", cancellationToken: Arg.Any<CancellationToken>())
            .Returns<ProcessRunResult>(_ => throw new Win32Exception("not found"));

        var result = await _provider.ScanInstalledAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsAvailable, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("not installed"));
        });
    }

    [Test]
    public async Task ScanInstalled_WingetReturnsError_ReturnsUnavailable()
    {
        _processRunner.RunAsync("winget", "list --accept-source-agreements", cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new ProcessRunResult(1, "", "error occurred"));

        var result = await _provider.ScanInstalledAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsAvailable, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("failed"));
        });
    }
}
