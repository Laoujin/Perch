using System.Collections.Immutable;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Perch.Core.Modules;

public sealed class ManifestParser
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public ManifestParseResult Parse(string yaml, string moduleName)
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            return ManifestParseResult.Failure("Manifest is empty.");
        }

        ManifestYamlModel model;
        try
        {
            model = Deserializer.Deserialize<ManifestYamlModel>(yaml);
        }
        catch (Exception ex)
        {
            return ManifestParseResult.Failure($"Invalid YAML: {ex.Message}");
        }

        if (model?.Links == null || model.Links.Count == 0)
        {
            return ManifestParseResult.Failure("Manifest must contain at least one entry in 'links'.");
        }

        var links = new List<LinkEntry>();
        for (int i = 0; i < model.Links.Count; i++)
        {
            var link = model.Links[i];
            if (string.IsNullOrWhiteSpace(link.Source))
            {
                return ManifestParseResult.Failure($"Link [{i}] is missing 'source'.");
            }

            if (string.IsNullOrWhiteSpace(link.Target))
            {
                return ManifestParseResult.Failure($"Link [{i}] is missing 'target'.");
            }

            var linkType = ParseLinkType(link.LinkType);
            links.Add(new LinkEntry(link.Source, link.Target, linkType));
        }

        string displayName = string.IsNullOrWhiteSpace(model.DisplayName) ? moduleName : model.DisplayName;
        var manifest = new AppManifest(moduleName, displayName, links.ToImmutableArray());
        return ManifestParseResult.Success(manifest);
    }

    private static LinkType ParseLinkType(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "junction" => LinkType.Junction,
            _ => LinkType.Symlink,
        };
}
