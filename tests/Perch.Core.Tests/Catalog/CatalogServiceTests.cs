using System.Collections.Immutable;

using NSubstitute;

using Perch.Core.Catalog;

namespace Perch.Core.Tests.Catalog;

[TestFixture]
public sealed class CatalogServiceTests
{
    private ICatalogFetcher _fetcher = null!;
    private ICatalogCache _cache = null!;
    private CatalogParser _parser = null!;
    private CatalogService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _fetcher = Substitute.For<ICatalogFetcher>();
        _cache = Substitute.For<ICatalogCache>();
        _parser = new CatalogParser();
        _service = new CatalogService(_fetcher, _cache, _parser);
    }

    [Test]
    public async Task GetIndexAsync_FetchesAndParsesIndex()
    {
        string indexYaml = """
            apps:
              - id: vscode
                name: VS Code
                category: Dev
            fonts: []
            tweaks: []
            """;

        _cache.GetAsync("index.yaml", Arg.Any<CancellationToken>()).Returns((string?)null);
        _fetcher.FetchAsync("index.yaml", Arg.Any<CancellationToken>()).Returns(indexYaml);

        var index = await _service.GetIndexAsync();

        Assert.That(index.Apps, Has.Length.EqualTo(1));
        Assert.That(index.Apps[0].Id, Is.EqualTo("vscode"));
        await _cache.Received(1).SetAsync("index.yaml", indexYaml, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetIndexAsync_UsesCacheWhenAvailable()
    {
        string indexYaml = """
            apps:
              - id: vscode
                name: VS Code
                category: Dev
            fonts: []
            tweaks: []
            """;

        _cache.GetAsync("index.yaml", Arg.Any<CancellationToken>()).Returns(indexYaml);

        var index = await _service.GetIndexAsync();

        Assert.That(index.Apps, Has.Length.EqualTo(1));
        await _fetcher.DidNotReceive().FetchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetAppAsync_FetchesAndParses()
    {
        string indexYaml = """
            apps:
              - id: firefox
                name: Firefox
                category: Browsers
            fonts: []
            tweaks: []
            """;

        string appYaml = """
            name: Firefox
            category: Browsers
            install:
              winget: Mozilla.Firefox
            """;

        _cache.GetAsync("index.yaml", Arg.Any<CancellationToken>()).Returns(indexYaml);
        _cache.GetAsync("apps/firefox.yaml", Arg.Any<CancellationToken>()).Returns((string?)null);
        _fetcher.FetchAsync("apps/firefox.yaml", Arg.Any<CancellationToken>()).Returns(appYaml);

        var app = await _service.GetAppAsync("firefox");

        Assert.That(app, Is.Not.Null);
        Assert.That(app!.Name, Is.EqualTo("Firefox"));
        Assert.That(app.Install!.Winget, Is.EqualTo("Mozilla.Firefox"));
    }

    [Test]
    public async Task GetAppAsync_UsesPathFromIndex()
    {
        string indexYaml = """
            apps:
              - id: dotnet-sdk
                name: .NET SDK
                category: Development/.NET
                path: apps/dotnet/dotnet-sdk.yaml
            fonts: []
            tweaks: []
            """;

        string appYaml = """
            name: .NET SDK
            category: Development/.NET
            install:
              winget: Microsoft.DotNet.SDK.9
            """;

        _cache.GetAsync("index.yaml", Arg.Any<CancellationToken>()).Returns(indexYaml);
        _cache.GetAsync("apps/dotnet/dotnet-sdk.yaml", Arg.Any<CancellationToken>()).Returns(appYaml);

        var app = await _service.GetAppAsync("dotnet-sdk");

        Assert.That(app, Is.Not.Null);
        Assert.That(app!.Name, Is.EqualTo(".NET SDK"));
    }

    [Test]
    public async Task GetTweakAsync_UsesPathFromIndex()
    {
        string indexYaml = """
            apps: []
            fonts: []
            tweaks:
              - id: dark-mode
                name: Dark Mode
                category: Appearance/Theme
                path: tweaks/appearance/dark-mode.yaml
            """;

        string tweakYaml = """
            name: Dark Mode
            category: Appearance/Theme
            reversible: true
            registry:
              - key: HKCU\Software\Test
                name: DarkMode
                value: 1
                type: dword
            """;

        _cache.GetAsync("index.yaml", Arg.Any<CancellationToken>()).Returns(indexYaml);
        _cache.GetAsync("tweaks/appearance/dark-mode.yaml", Arg.Any<CancellationToken>()).Returns(tweakYaml);

        var tweak = await _service.GetTweakAsync("dark-mode");

        Assert.That(tweak, Is.Not.Null);
        Assert.That(tweak!.Name, Is.EqualTo("Dark Mode"));
    }

    [Test]
    public async Task GetAllAppsAsync_FetchesIndexThenEachApp()
    {
        string indexYaml = """
            apps:
              - id: vscode
                name: VS Code
                category: Dev
            fonts: []
            tweaks: []
            """;

        string appYaml = """
            name: Visual Studio Code
            category: Development
            install:
              winget: Microsoft.VisualStudio.Code
            """;

        _cache.GetAsync("index.yaml", Arg.Any<CancellationToken>()).Returns(indexYaml);
        _cache.GetAsync("apps/vscode.yaml", Arg.Any<CancellationToken>()).Returns((string?)null);
        _fetcher.FetchAsync("apps/vscode.yaml", Arg.Any<CancellationToken>()).Returns(appYaml);

        var apps = await _service.GetAllAppsAsync();

        Assert.That(apps, Has.Length.EqualTo(1));
        Assert.That(apps[0].Name, Is.EqualTo("Visual Studio Code"));
    }
}
