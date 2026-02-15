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
        string appYaml = """
            name: Firefox
            category: Browsers
            install:
              winget: Mozilla.Firefox
            """;

        _cache.GetAsync("apps/firefox.yaml", Arg.Any<CancellationToken>()).Returns((string?)null);
        _fetcher.FetchAsync("apps/firefox.yaml", Arg.Any<CancellationToken>()).Returns(appYaml);

        var app = await _service.GetAppAsync("firefox");

        Assert.That(app, Is.Not.Null);
        Assert.That(app!.Name, Is.EqualTo("Firefox"));
        Assert.That(app.Install!.Winget, Is.EqualTo("Mozilla.Firefox"));
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
