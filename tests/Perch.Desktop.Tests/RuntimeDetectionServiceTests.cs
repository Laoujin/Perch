using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.Versioning;

using Perch.Core.Catalog;
using Perch.Core.Packages;
using Perch.Desktop.Services;

namespace Perch.Desktop.Tests;

[TestFixture]
[Platform("Win")]
[SupportedOSPlatform("windows")]
public sealed class RuntimeDetectionServiceTests
{
    private IProcessRunner _processRunner = null!;
    private RuntimeDetectionService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _processRunner = Substitute.For<IProcessRunner>();
        _service = new RuntimeDetectionService(
            _processRunner,
            Substitute.For<ILogger<RuntimeDetectionService>>());
    }

    [Test]
    public async Task DetectRuntimeAsync_DotnetInstalled_ReturnsVersionFromStdout()
    {
        var entry = MakeRuntime("dotnet-sdk", "dotnet");
        _processRunner.RunAsync("dotnet", "--version", null, Arg.Any<CancellationToken>())
            .Returns(new ProcessRunResult(0, "10.0.100\n", ""));

        var result = await _service.DetectRuntimeAsync(entry);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsInstalled, Is.True);
            Assert.That(result.Version, Is.EqualTo("10.0.100"));
        });
    }

    [Test]
    public async Task DetectRuntimeAsync_NodeInstalled_ReturnsVersionStripped()
    {
        var entry = MakeRuntime("node", "node");
        _processRunner.RunAsync("node", "--version", null, Arg.Any<CancellationToken>())
            .Returns(new ProcessRunResult(0, "v22.14.0\n", ""));

        var result = await _service.DetectRuntimeAsync(entry);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsInstalled, Is.True);
            Assert.That(result.Version, Is.EqualTo("22.14.0"));
        });
    }

    [Test]
    public async Task DetectRuntimeAsync_PythonInstalled_ReturnsVersion()
    {
        var entry = MakeRuntime("python", "python");
        _processRunner.RunAsync("python", "--version", null, Arg.Any<CancellationToken>())
            .Returns(new ProcessRunResult(0, "Python 3.12.1\n", ""));

        var result = await _service.DetectRuntimeAsync(entry);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsInstalled, Is.True);
            Assert.That(result.Version, Is.EqualTo("3.12.1"));
        });
    }

    [Test]
    public async Task DetectRuntimeAsync_GoInstalled_ParsesGoVersionOutput()
    {
        var entry = MakeRuntime("go", "go");
        _processRunner.RunAsync("go", "version", null, Arg.Any<CancellationToken>())
            .Returns(new ProcessRunResult(0, "go version go1.22.1 windows/amd64\n", ""));

        var result = await _service.DetectRuntimeAsync(entry);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsInstalled, Is.True);
            Assert.That(result.Version, Is.EqualTo("1.22.1"));
        });
    }

    [Test]
    public async Task DetectRuntimeAsync_JavaInstalled_ParsesJavaVersionFromStderr()
    {
        var entry = MakeRuntime("java", "java");
        _processRunner.RunAsync("java", "--version", null, Arg.Any<CancellationToken>())
            .Returns(new ProcessRunResult(0, "openjdk 21.0.2 2024-01-16\nOpenJDK Runtime Environment\n", ""));

        var result = await _service.DetectRuntimeAsync(entry);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsInstalled, Is.True);
            Assert.That(result.Version, Is.EqualTo("21.0.2"));
        });
    }

    [Test]
    public async Task DetectRuntimeAsync_RubyInstalled_ReturnsVersion()
    {
        var entry = MakeRuntime("ruby", "ruby");
        _processRunner.RunAsync("ruby", "--version", null, Arg.Any<CancellationToken>())
            .Returns(new ProcessRunResult(0, "ruby 3.3.0 (2023-12-25 revision 5124f9ac75) [x64-mingw-ucrt]\n", ""));

        var result = await _service.DetectRuntimeAsync(entry);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsInstalled, Is.True);
            Assert.That(result.Version, Is.EqualTo("3.3.0"));
        });
    }

    [Test]
    public async Task DetectRuntimeAsync_RustInstalled_ParsesRustupVersion()
    {
        var entry = MakeRuntime("rust", "rustup");
        _processRunner.RunAsync("rustup", "--version", null, Arg.Any<CancellationToken>())
            .Returns(new ProcessRunResult(0, "rustup 1.27.0 (bbb9276d2 2024-03-08)\n", ""));

        var result = await _service.DetectRuntimeAsync(entry);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsInstalled, Is.True);
            Assert.That(result.Version, Is.EqualTo("1.27.0"));
        });
    }

    [Test]
    public async Task DetectRuntimeAsync_CommandNotFound_ReturnsNotInstalled()
    {
        var entry = MakeRuntime("dotnet-sdk", "dotnet");
        _processRunner.RunAsync("dotnet", "--version", null, Arg.Any<CancellationToken>())
            .Returns<ProcessRunResult>(x => throw new Win32Exception("The system cannot find the file specified"));

        var result = await _service.DetectRuntimeAsync(entry);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsInstalled, Is.False);
            Assert.That(result.Version, Is.Null);
        });
    }

    [Test]
    public async Task DetectRuntimeAsync_Timeout_ReturnsNotInstalled()
    {
        var entry = MakeRuntime("node", "node");
        _processRunner.RunAsync("node", "--version", null, Arg.Any<CancellationToken>())
            .Returns<ProcessRunResult>(x => throw new OperationCanceledException());

        var result = await _service.DetectRuntimeAsync(entry);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsInstalled, Is.False);
            Assert.That(result.Version, Is.Null);
        });
    }

    [Test]
    public async Task DetectRuntimeAsync_NonZeroExitCode_ReturnsNotInstalled()
    {
        var entry = MakeRuntime("dotnet-sdk", "dotnet");
        _processRunner.RunAsync("dotnet", "--version", null, Arg.Any<CancellationToken>())
            .Returns(new ProcessRunResult(1, "", "error"));

        var result = await _service.DetectRuntimeAsync(entry);

        Assert.That(result.IsInstalled, Is.False);
    }

    [Test]
    public async Task DetectRuntimeAsync_UnknownRuntime_ReturnsNotInstalled()
    {
        var entry = MakeRuntime("unknown-lang", "unknown");

        var result = await _service.DetectRuntimeAsync(entry);

        Assert.That(result.IsInstalled, Is.False);
    }

    [Test]
    public async Task DetectRuntimeAsync_NonRuntimeKind_ReturnsNotInstalled()
    {
        var entry = new CatalogEntry(
            "vscode", "VS Code", null, "Development/IDEs",
            [], null, null, null,
            new InstallDefinition("Microsoft.VisualStudioCode", null),
            null, null, CatalogKind.App);

        var result = await _service.DetectRuntimeAsync(entry);

        Assert.That(result.IsInstalled, Is.False);
    }

    [Test]
    public async Task DetectGlobalToolsAsync_DotnetTools_MatchesCatalogEntries()
    {
        _processRunner.RunAsync("dotnet", "tool list -g", null, Arg.Any<CancellationToken>())
            .Returns(new ProcessRunResult(0,
                "Package Id         Version      Commands\n" +
                "-------------------------------------------\n" +
                "csharpier          0.29.2       dotnet-csharpier\n" +
                "dotnet-ef          9.0.0        dotnet-ef\n", ""));

        var candidates = new[]
        {
            MakeTool("csharpier", dotnetTool: "csharpier"),
            MakeTool("dotnet-ef", dotnetTool: "dotnet-ef"),
            MakeTool("unrelated-tool", dotnetTool: "something-else"),
        };

        var matches = await _service.DetectGlobalToolsAsync("dotnet-sdk", candidates);

        Assert.Multiple(() =>
        {
            Assert.That(matches, Has.Length.EqualTo(2));
            Assert.That(matches.Select(m => m.CatalogEntryId), Is.EquivalentTo(new[] { "csharpier", "dotnet-ef" }));
        });
    }

    [Test]
    public async Task DetectGlobalToolsAsync_NpmPackages_MatchesCatalogEntries()
    {
        _processRunner.RunAsync("npm", "list -g --json", null, Arg.Any<CancellationToken>())
            .Returns(new ProcessRunResult(0,
                """
                {
                  "dependencies": {
                    "typescript": { "version": "5.3.3" },
                    "eslint": { "version": "8.56.0" }
                  }
                }
                """, ""));

        var candidates = new[]
        {
            MakeTool("typescript", nodePackage: "typescript"),
            MakeTool("eslint", nodePackage: "eslint"),
            MakeTool("unrelated", nodePackage: "something-else"),
        };

        var matches = await _service.DetectGlobalToolsAsync("node", candidates);

        Assert.Multiple(() =>
        {
            Assert.That(matches, Has.Length.EqualTo(2));
            Assert.That(matches.Select(m => m.CatalogEntryId), Is.EquivalentTo(new[] { "typescript", "eslint" }));
        });
    }

    [Test]
    public async Task DetectGlobalToolsAsync_CommandFails_ReturnsEmpty()
    {
        _processRunner.RunAsync("dotnet", "tool list -g", null, Arg.Any<CancellationToken>())
            .Returns<ProcessRunResult>(x => throw new Win32Exception("not found"));

        var candidates = new[] { MakeTool("csharpier", dotnetTool: "csharpier") };

        var matches = await _service.DetectGlobalToolsAsync("dotnet-sdk", candidates);

        Assert.That(matches, Is.Empty);
    }

    [Test]
    public async Task DetectGlobalToolsAsync_UnsupportedRuntime_ReturnsEmpty()
    {
        var candidates = new[] { MakeTool("some-tool") };

        var matches = await _service.DetectGlobalToolsAsync("ruby", candidates);

        Assert.That(matches, Is.Empty);
    }

    [Test]
    public async Task DetectGlobalToolsAsync_NpmInvalidJson_ReturnsEmpty()
    {
        _processRunner.RunAsync("npm", "list -g --json", null, Arg.Any<CancellationToken>())
            .Returns(new ProcessRunResult(0, "not json", ""));

        var candidates = new[] { MakeTool("typescript", nodePackage: "typescript") };

        var matches = await _service.DetectGlobalToolsAsync("node", candidates);

        Assert.That(matches, Is.Empty);
    }

    private static CatalogEntry MakeRuntime(string id, string cliName) =>
        new(id, id, null, "Development/Languages",
            [], null, null, null,
            new InstallDefinition(null, null, Detect: cliName),
            null, null, CatalogKind.Runtime);

    private static CatalogEntry MakeTool(string id, string? dotnetTool = null, string? nodePackage = null) =>
        new(id, id, null, "Development/Tools",
            [], null, null, null,
            new InstallDefinition(null, null, DotnetTool: dotnetTool, NodePackage: nodePackage),
            null, null, CatalogKind.CliTool);
}
