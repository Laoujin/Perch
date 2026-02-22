using Perch.Core.Deploy;
using Perch.Core.Packages;
using Perch.Core.Scanner;

namespace Perch.Core.Tests.Deploy;

[TestFixture]
public sealed class VscodeExtensionInstallerTests
{
    private IProcessRunner _processRunner = null!;
    private IVsCodeService _vsCodeService = null!;
    private VscodeExtensionInstaller _installer = null!;

    [SetUp]
    public void SetUp()
    {
        _processRunner = Substitute.For<IProcessRunner>();
        _vsCodeService = Substitute.For<IVsCodeService>();
        _vsCodeService.GetCodePath().Returns("code");
        _installer = new VscodeExtensionInstaller(_processRunner, _vsCodeService);
    }

    [Test]
    public async Task InstallAsync_VsCodeNotFound_ReturnsError()
    {
        _vsCodeService.GetCodePath().Returns((string?)null);

        DeployResult result = await _installer.InstallAsync("VS Code", "ms-dotnettools.csharp", dryRun: false);

        Assert.Multiple(() =>
        {
            Assert.That(result.Level, Is.EqualTo(ResultLevel.Error));
            Assert.That(result.Message, Does.Contain("VS Code not found"));
        });
    }

    [Test]
    public async Task InstallAsync_DryRun_DoesNotRunProcess()
    {
        DeployResult result = await _installer.InstallAsync("VS Code", "ms-dotnettools.csharp", dryRun: true);

        Assert.Multiple(() =>
        {
            Assert.That(result.Level, Is.EqualTo(ResultLevel.Ok));
            Assert.That(result.Message, Does.Contain("Would run"));
        });
        await _processRunner.DidNotReceive().RunAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task InstallAsync_Success_ReturnsOk()
    {
        _processRunner.RunAsync("code", Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new ProcessRunResult(0, "Installing...", ""));

        DeployResult result = await _installer.InstallAsync("VS Code", "ms-dotnettools.csharp", dryRun: false);

        Assert.Multiple(() =>
        {
            Assert.That(result.Level, Is.EqualTo(ResultLevel.Ok));
            Assert.That(result.Message, Does.Contain("Installed"));
        });
    }

    [Test]
    public async Task InstallAsync_Failure_ReturnsError()
    {
        _processRunner.RunAsync("code", Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new ProcessRunResult(1, "", "Extension not found"));

        DeployResult result = await _installer.InstallAsync("VS Code", "bad.extension", dryRun: false);

        Assert.Multiple(() =>
        {
            Assert.That(result.Level, Is.EqualTo(ResultLevel.Error));
            Assert.That(result.Message, Does.Contain("failed"));
        });
    }
}
