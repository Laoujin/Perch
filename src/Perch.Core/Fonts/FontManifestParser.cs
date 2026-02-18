using System.Collections.Immutable;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Perch.Core.Fonts;

public sealed class FontManifestParser
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public FontManifestParseResult Parse(string yaml)
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            return FontManifestParseResult.Ok(ImmutableArray<string>.Empty);
        }

        List<string> ids;
        try
        {
            ids = Deserializer.Deserialize<List<string>>(yaml);
        }
        catch (YamlException ex)
        {
            return FontManifestParseResult.Failure($"Invalid fonts YAML: {ex.Message}");
        }

        if (ids == null || ids.Count == 0)
        {
            return FontManifestParseResult.Ok(ImmutableArray<string>.Empty);
        }

        var fontIds = ids
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .ToImmutableArray();

        return FontManifestParseResult.Ok(fontIds);
    }
}
