using System.Collections.Immutable;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Perch.Core.Packages;

public sealed class InstallManifestParser
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public InstallManifestParseResult Parse(string yaml)
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            return InstallManifestParseResult.Failure("Install manifest is empty.");
        }

        InstallYamlModel model;
        try
        {
            model = Deserializer.Deserialize<InstallYamlModel>(yaml);
        }
        catch (YamlException ex)
        {
            return InstallManifestParseResult.Failure($"Invalid YAML: {ex.Message}");
        }

        var apps = model?.Apps?.Where(a => !string.IsNullOrWhiteSpace(a)).ToImmutableArray()
            ?? ImmutableArray<string>.Empty;

        var machines = ImmutableDictionary<string, MachineInstallOverrides>.Empty;
        if (model?.Machines != null)
        {
            var builder = ImmutableDictionary.CreateBuilder<string, MachineInstallOverrides>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in model.Machines)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key) || kvp.Value == null)
                {
                    continue;
                }

                var add = kvp.Value.Add?.Where(a => !string.IsNullOrWhiteSpace(a)).ToImmutableArray()
                    ?? ImmutableArray<string>.Empty;
                var exclude = kvp.Value.Exclude?.Where(e => !string.IsNullOrWhiteSpace(e)).ToImmutableArray()
                    ?? ImmutableArray<string>.Empty;
                builder[kvp.Key] = new MachineInstallOverrides(add, exclude);
            }

            machines = builder.ToImmutable();
        }

        return InstallManifestParseResult.Ok(new InstallManifest(apps, machines));
    }
}

internal sealed class InstallYamlModel
{
    public List<string>? Apps { get; set; }
    public Dictionary<string, MachineOverridesYamlModel>? Machines { get; set; }
}

internal sealed class MachineOverridesYamlModel
{
    public List<string>? Add { get; set; }
    public List<string>? Exclude { get; set; }
}
