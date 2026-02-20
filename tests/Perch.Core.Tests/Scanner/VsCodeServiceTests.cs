using NSubstitute;

using Perch.Core.Packages;
using Perch.Core.Scanner;

namespace Perch.Core.Tests.Scanner;

[TestFixture]
public sealed class VsCodeServiceTests
{
    private IProcessRunner _processRunner = null!;
    private VsCodeService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _processRunner = Substitute.For<IProcessRunner>();
        _service = new TestableVsCodeService(_processRunner);
    }

    private sealed class TestableVsCodeService(IProcessRunner runner) : VsCodeService(runner)
    {
        protected override string? FindCodePath() => "code";
    }

    [Test]
    public async Task GetInstalledExtensionsAsync_ParsesOutput()
    {
        string output = """
            dbaeumer.vscode-eslint@3.0.10
            esbenp.prettier-vscode@11.0.0
            eamodio.gitlens@15.6.0
            """;

        _processRunner.RunAsync(Arg.Any<string>(), "--list-extensions --show-versions", null, Arg.Any<CancellationToken>())
            .Returns(new ProcessRunResult(0, output, string.Empty));

        var extensions = await _service.GetInstalledExtensionsAsync();

        Assert.That(extensions, Has.Length.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(extensions[0].Id, Is.EqualTo("dbaeumer.vscode-eslint"));
            Assert.That(extensions[0].Version, Is.EqualTo("3.0.10"));
            Assert.That(extensions[1].Id, Is.EqualTo("esbenp.prettier-vscode"));
        });
    }

    [Test]
    public async Task GetInstalledExtensionsAsync_NonZeroExitCode_ReturnsEmpty()
    {
        _processRunner.RunAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new ProcessRunResult(1, string.Empty, "error"));

        var extensions = await _service.GetInstalledExtensionsAsync();

        Assert.That(extensions, Is.Empty);
    }

    [Test]
    public void IsInstalled_WhenCodePathExists_ReturnsTrue()
    {
        Assert.That(_service.IsInstalled, Is.True);
    }

    [Test]
    public void IsInstalled_WhenCodePathNull_ReturnsFalse()
    {
        var svc = new NullCodePathService(_processRunner);
        Assert.That(svc.IsInstalled, Is.False);
    }

    [Test]
    public async Task GetInstalledExtensionsAsync_CodePathNull_ReturnsEmpty()
    {
        var svc = new NullCodePathService(_processRunner);
        var extensions = await svc.GetInstalledExtensionsAsync();
        Assert.That(extensions, Is.Empty);
    }

    [Test]
    public async Task GetInstalledExtensionsAsync_ExtensionWithoutVersion_ParsesIdOnly()
    {
        _processRunner.RunAsync(Arg.Any<string>(), "--list-extensions --show-versions", null, Arg.Any<CancellationToken>())
            .Returns(new ProcessRunResult(0, "ms-dotnettools.csharp", string.Empty));

        var extensions = await _service.GetInstalledExtensionsAsync();

        Assert.Multiple(() =>
        {
            Assert.That(extensions, Has.Length.EqualTo(1));
            Assert.That(extensions[0].Id, Is.EqualTo("ms-dotnettools.csharp"));
            Assert.That(extensions[0].Version, Is.Null);
        });
    }

    [Test]
    public async Task GetInstalledExtensionsAsync_MixedWithAndWithoutVersion()
    {
        string output = "dbaeumer.vscode-eslint@3.0.10\nms-dotnettools.csharp\nesbenp.prettier-vscode@11.0.0";
        _processRunner.RunAsync(Arg.Any<string>(), "--list-extensions --show-versions", null, Arg.Any<CancellationToken>())
            .Returns(new ProcessRunResult(0, output, string.Empty));

        var extensions = await _service.GetInstalledExtensionsAsync();

        Assert.Multiple(() =>
        {
            Assert.That(extensions, Has.Length.EqualTo(3));
            Assert.That(extensions[0].Version, Is.EqualTo("3.0.10"));
            Assert.That(extensions[1].Id, Is.EqualTo("ms-dotnettools.csharp"));
            Assert.That(extensions[1].Version, Is.Null);
            Assert.That(extensions[2].Version, Is.EqualTo("11.0.0"));
        });
    }

    private sealed class NullCodePathService(IProcessRunner runner) : VsCodeService(runner)
    {
        protected override string? FindCodePath() => null;
    }
}
