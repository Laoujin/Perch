using System.Collections.Immutable;

using Perch.Core.Git;
using Perch.Core.Modules;
using Perch.Core.Registry;

using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Perch.Core.Catalog;

public sealed class CatalogParser
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public CatalogParseResult<CatalogEntry> ParseApp(string yaml, string id)
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            return CatalogParseResult<CatalogEntry>.Failure("YAML content is empty.");
        }

        AppCatalogYamlModel model;
        try
        {
            model = Deserializer.Deserialize<AppCatalogYamlModel>(yaml);
        }
        catch (YamlException ex)
        {
            return CatalogParseResult<CatalogEntry>.Failure($"Invalid YAML: {ex.Message}");
        }

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return CatalogParseResult<CatalogEntry>.Failure("App entry is missing 'name'.");
        }

        var category = model.Category ?? "Uncategorized";
        var entry = new CatalogEntry(
            id,
            model.Name!,
            model.DisplayName,
            category,
            ToImmutableTags(model.Tags),
            model.Description,
            model.Logo,
            ParseLinks(model.Links),
            ParseInstall(model.Install),
            ParseConfig(model.Config),
            ParseExtensions(model.Extensions),
            DeriveKind(category, model.CliTool),
            ParseAppOwnedTweaks(model.Tweaks),
            ToImmutableTags(model.Profiles),
            ToImmutableTags(model.Os),
            model.License,
            ToImmutableTags(model.Alternatives),
            ToImmutableTags(model.Suggests),
            ToImmutableTags(model.Requires),
            model.Hot,
            model.Sort);

        return CatalogParseResult<CatalogEntry>.Ok(entry);
    }

    public CatalogParseResult<FontCatalogEntry> ParseFont(string yaml, string id)
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            return CatalogParseResult<FontCatalogEntry>.Failure("YAML content is empty.");
        }

        FontCatalogYamlModel model;
        try
        {
            model = Deserializer.Deserialize<FontCatalogYamlModel>(yaml);
        }
        catch (YamlException ex)
        {
            return CatalogParseResult<FontCatalogEntry>.Failure($"Invalid YAML: {ex.Message}");
        }

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return CatalogParseResult<FontCatalogEntry>.Failure("Font entry is missing 'name'.");
        }

        var entry = new FontCatalogEntry(
            id,
            model.Name!,
            model.Category ?? "Fonts",
            ToImmutableTags(model.Tags),
            model.Description,
            model.Logo,
            model.PreviewText,
            ParseInstall(model.Install),
            ToImmutableTags(model.Profiles),
            model.License,
            model.Sort);

        return CatalogParseResult<FontCatalogEntry>.Ok(entry);
    }

    public CatalogParseResult<TweakCatalogEntry> ParseTweak(string yaml, string id)
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            return CatalogParseResult<TweakCatalogEntry>.Failure("YAML content is empty.");
        }

        TweakCatalogYamlModel model;
        try
        {
            model = Deserializer.Deserialize<TweakCatalogYamlModel>(yaml);
        }
        catch (YamlException ex)
        {
            return CatalogParseResult<TweakCatalogEntry>.Failure($"Invalid YAML: {ex.Message}");
        }

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return CatalogParseResult<TweakCatalogEntry>.Failure("Tweak entry is missing 'name'.");
        }

        var registryEntries = ParseTweakRegistry(model.Registry);

        var entry = new TweakCatalogEntry(
            id,
            model.Name!,
            model.Category ?? "Uncategorized",
            ToImmutableTags(model.Tags),
            model.Description,
            model.Reversible,
            ToImmutableTags(model.Profiles),
            registryEntries,
            model.Script,
            model.UndoScript,
            ToImmutableTags(model.Suggests),
            ToImmutableTags(model.Requires),
            ToImmutableTags(model.Alternatives),
            model.WindowsVersions?.ToImmutableArray() ?? ImmutableArray<int>.Empty,
            model.License,
            model.Source,
            model.Sort);

        return CatalogParseResult<TweakCatalogEntry>.Ok(entry);
    }

    public IReadOnlyDictionary<string, int> ParseGitHubStars(string yaml)
    {
        if (string.IsNullOrWhiteSpace(yaml))
            return new Dictionary<string, int>();

        Dictionary<string, GitHubStarYamlModel> model;
        try
        {
            model = Deserializer.Deserialize<Dictionary<string, GitHubStarYamlModel>>(yaml);
        }
        catch (YamlException)
        {
            return new Dictionary<string, int>();
        }

        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in model)
        {
            var appId = key.StartsWith("app/", StringComparison.OrdinalIgnoreCase)
                ? key[4..]
                : key;
            result[appId] = value.Stars;
        }

        return result;
    }

    public ImmutableDictionary<string, CategoryDefinition> ParseCategories(string yaml)
    {
        if (string.IsNullOrWhiteSpace(yaml))
            return ImmutableDictionary<string, CategoryDefinition>.Empty;

        Dictionary<string, CategoryDefinitionYamlModel> model;
        try
        {
            model = Deserializer.Deserialize<Dictionary<string, CategoryDefinitionYamlModel>>(yaml);
        }
        catch (YamlException)
        {
            return ImmutableDictionary<string, CategoryDefinition>.Empty;
        }

        return ConvertCategories(model);
    }

    private static ImmutableDictionary<string, CategoryDefinition> ConvertCategories(
        Dictionary<string, CategoryDefinitionYamlModel>? model)
    {
        if (model == null || model.Count == 0)
            return ImmutableDictionary<string, CategoryDefinition>.Empty;

        var builder = ImmutableDictionary.CreateBuilder<string, CategoryDefinition>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, value) in model)
        {
            var children = ConvertCategories(value.Children);
            builder[name] = new CategoryDefinition(name, value.Sort, value.Pattern, children);
        }

        return builder.ToImmutable();
    }

    public CatalogParseResult<CatalogIndex> ParseIndex(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return CatalogParseResult<CatalogIndex>.Failure("Index content is empty.");
        }

        CatalogIndexYamlModel model;
        try
        {
            model = Deserializer.Deserialize<CatalogIndexYamlModel>(json);
        }
        catch (YamlException ex)
        {
            return CatalogParseResult<CatalogIndex>.Failure($"Invalid index: {ex.Message}");
        }

        var index = new CatalogIndex(
            ParseIndexEntries(model.Apps),
            ParseIndexEntries(model.Fonts),
            ParseIndexEntries(model.Tweaks));

        return CatalogParseResult<CatalogIndex>.Ok(index);
    }

    private static ImmutableArray<CatalogIndexEntry> ParseIndexEntries(List<CatalogIndexEntryYamlModel>? entries)
    {
        if (entries == null || entries.Count == 0)
        {
            return ImmutableArray<CatalogIndexEntry>.Empty;
        }

        return entries
            .Where(e => !string.IsNullOrWhiteSpace(e.Id) && !string.IsNullOrWhiteSpace(e.Name))
            .Select(e => new CatalogIndexEntry(
                e.Id!,
                e.Name!,
                e.Category ?? "Uncategorized",
                ToImmutableTags(e.Tags),
                DeriveKind(e.Category ?? "Uncategorized", e.CliTool),
                ToImmutableTags(e.Profiles),
                e.Path,
                e.Sort))
            .ToImmutableArray();
    }

    private static ImmutableArray<string> ToImmutableTags(List<string>? tags) =>
        tags == null || tags.Count == 0
            ? ImmutableArray<string>.Empty
            : tags.Where(t => !string.IsNullOrWhiteSpace(t)).ToImmutableArray();

    private static CatalogLinks? ParseLinks(CatalogLinksYamlModel? model) =>
        model == null ? null : new CatalogLinks(model.Website, model.Docs, model.GitHub);

    private static InstallDefinition? ParseInstall(InstallYamlModel? model) =>
        model == null ? null : new InstallDefinition(model.Winget, model.Choco, model.DotnetTool, model.NodePackage, model.Detect);

    private static CatalogConfigDefinition? ParseConfig(CatalogConfigYamlModel? model)
    {
        if (model?.Links == null || model.Links.Count == 0)
        {
            return null;
        }

        var links = new List<CatalogConfigLink>();
        foreach (var link in model.Links)
        {
            if (string.IsNullOrWhiteSpace(link.Source) || link.Target == null)
            {
                continue;
            }

            var targets = new Dictionary<Platform, string>();
            foreach (var kvp in link.Target)
            {
                if (Enum.TryParse<Platform>(kvp.Key, ignoreCase: true, out var platform))
                {
                    targets[platform] = kvp.Value;
                }
            }

            if (targets.Count > 0)
            {
                var linkType = ParseLinkType(link.LinkType);
                var platforms = ParsePlatforms(link.Platforms);
                links.Add(new CatalogConfigLink(link.Source, targets.ToImmutableDictionary(), linkType, platforms, link.Template));
            }
        }

        if (links.Count == 0)
        {
            return null;
        }

        var cleanFilter = ParseCatalogCleanFilter(model.CleanFilter);
        return new CatalogConfigDefinition(links.ToImmutableArray(), cleanFilter);
    }

    private static LinkType ParseLinkType(string? linkType) =>
        linkType?.ToLowerInvariant() switch
        {
            "junction" => LinkType.Junction,
            _ => LinkType.Symlink,
        };

    private static ImmutableArray<Platform> ParsePlatforms(List<string>? platforms)
    {
        if (platforms == null || platforms.Count == 0)
        {
            return default;
        }

        return platforms
            .Where(p => Enum.TryParse<Platform>(p, ignoreCase: true, out _))
            .Select(p => Enum.Parse<Platform>(p, ignoreCase: true))
            .ToImmutableArray();
    }

    private static CatalogCleanFilter? ParseCatalogCleanFilter(CatalogCleanFilterYamlModel? model)
    {
        if (model?.Files == null || model.Files.Count == 0 || model.Rules == null || model.Rules.Count == 0)
        {
            return null;
        }

        var rules = new List<FilterRule>();
        foreach (var rule in model.Rules)
        {
            if (string.IsNullOrWhiteSpace(rule.Type))
            {
                continue;
            }

            var patterns = rule.Type switch
            {
                "strip-xml-elements" => rule.Elements,
                "strip-ini-keys" => rule.Keys,
                "strip-json-keys" => rule.Keys,
                _ => null,
            };

            if (patterns != null && patterns.Count > 0)
            {
                rules.Add(new FilterRule(rule.Type, patterns.ToImmutableArray()));
            }
        }

        if (rules.Count == 0)
        {
            return null;
        }

        return new CatalogCleanFilter(model.Files.ToImmutableArray(), rules.ToImmutableArray());
    }

    private static CatalogKind DeriveKind(string category, bool cliTool)
    {
        if (cliTool)
            return CatalogKind.CliTool;

        var parts = category.Split('/');
        if (parts.Length >= 2 && parts[0].Equals("Languages", StringComparison.OrdinalIgnoreCase))
        {
            if (parts.Length >= 3 && parts[2].Equals("Runtimes", StringComparison.OrdinalIgnoreCase))
                return CatalogKind.Runtime;
        }

        if (parts[0].Equals("Terminal", StringComparison.OrdinalIgnoreCase) &&
            parts.Length >= 2 && parts[1].Equals("Shells", StringComparison.OrdinalIgnoreCase))
        {
            return CatalogKind.Dotfile;
        }

        return CatalogKind.App;
    }

    private static CatalogExtensions? ParseExtensions(CatalogExtensionsYamlModel? model)
    {
        if (model == null)
        {
            return null;
        }

        return new CatalogExtensions(
            ToImmutableTags(model.Bundled),
            ToImmutableTags(model.Recommended));
    }

    private static ImmutableArray<AppOwnedTweak> ParseAppOwnedTweaks(List<AppOwnedTweakYamlModel>? tweaks)
    {
        if (tweaks == null || tweaks.Count == 0)
        {
            return ImmutableArray<AppOwnedTweak>.Empty;
        }

        var result = new List<AppOwnedTweak>();
        foreach (var tweak in tweaks)
        {
            if (string.IsNullOrWhiteSpace(tweak.Id) || string.IsNullOrWhiteSpace(tweak.Name))
            {
                continue;
            }

            result.Add(new AppOwnedTweak(
                tweak.Id!,
                tweak.Name!,
                tweak.Description,
                ParseTweakRegistry(tweak.Registry),
                tweak.Script,
                tweak.UndoScript,
                tweak.Reversible));
        }

        return result.ToImmutableArray();
    }

    private static ImmutableArray<RegistryEntryDefinition> ParseTweakRegistry(List<TweakRegistryYamlModel>? entries)
    {
        if (entries == null || entries.Count == 0)
        {
            return ImmutableArray<RegistryEntryDefinition>.Empty;
        }

        var result = new List<RegistryEntryDefinition>();
        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Key) || entry.Name == null)
            {
                continue;
            }

            var kind = ParseRegistryValueType(entry.Type);
            object? value = entry.Value != null ? CoerceRegistryValue(entry.Value, kind) : null;
            object? defaultValue = entry.HasDefaultValue && entry.DefaultValue != null
                ? CoerceRegistryValue(entry.DefaultValue, kind)
                : null;
            result.Add(new RegistryEntryDefinition(entry.Key!, entry.Name!, value, kind, defaultValue));
        }

        return result.ToImmutableArray();
    }

    private static RegistryValueType ParseRegistryValueType(string? type) =>
        type?.ToLowerInvariant() switch
        {
            "dword" => RegistryValueType.DWord,
            "qword" => RegistryValueType.QWord,
            "expandstring" => RegistryValueType.ExpandString,
            _ => RegistryValueType.String,
        };

    private static object CoerceRegistryValue(object value, RegistryValueType kind) =>
        kind switch
        {
            RegistryValueType.DWord when value is int i => i,
            RegistryValueType.DWord when value is long l => (int)l,
            RegistryValueType.DWord when value is string s && int.TryParse(s, out int parsed) => parsed,
            RegistryValueType.QWord when value is long l => l,
            RegistryValueType.QWord when value is int i => (long)i,
            RegistryValueType.QWord when value is string s && long.TryParse(s, out long parsed) => parsed,
            _ => value.ToString() ?? string.Empty,
        };
}
