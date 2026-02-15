using System.Collections.Immutable;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Perch.Core.Machines;

public sealed class MachineProfileService : IMachineProfileService
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public async Task<MachineProfile?> LoadAsync(string configRepoPath, CancellationToken cancellationToken = default)
    {
        string machinesDir = Path.Combine(configRepoPath, "machines");
        if (!Directory.Exists(machinesDir))
        {
            return null;
        }

        string hostname = Environment.MachineName;
        string? basePath = FindProfileFile(machinesDir, "base");
        string? hostnamePath = FindProfileFile(machinesDir, hostname);

        if (basePath == null && hostnamePath == null)
        {
            return null;
        }

        MachineProfileYamlModel? baseModel = basePath != null
            ? await DeserializeAsync(basePath, cancellationToken).ConfigureAwait(false)
            : null;
        MachineProfileYamlModel? hostnameModel = hostnamePath != null
            ? await DeserializeAsync(hostnamePath, cancellationToken).ConfigureAwait(false)
            : null;

        if (baseModel == null)
        {
            return ToProfile(hostnameModel!);
        }

        if (hostnameModel == null)
        {
            return ToProfile(baseModel);
        }

        return MergeProfiles(baseModel, hostnameModel);
    }

    private static MachineProfile MergeProfiles(MachineProfileYamlModel baseModel, MachineProfileYamlModel hostnameModel)
    {
        ImmutableArray<string> includes = hostnameModel.IncludeModules != null
            ? ToImmutableArray(hostnameModel.IncludeModules)
            : ToImmutableArray(baseModel.IncludeModules);

        ImmutableArray<string> excludes = hostnameModel.ExcludeModules != null
            ? ToImmutableArray(hostnameModel.ExcludeModules)
            : ToImmutableArray(baseModel.ExcludeModules);

        ImmutableDictionary<string, string> variables = ToImmutableDictionary(baseModel.Variables);
        if (hostnameModel.Variables != null)
        {
            variables = variables.SetItems(hostnameModel.Variables);
        }

        return new MachineProfile(includes, excludes, variables);
    }

    private static MachineProfile ToProfile(MachineProfileYamlModel model)
    {
        return new MachineProfile(
            ToImmutableArray(model.IncludeModules),
            ToImmutableArray(model.ExcludeModules),
            ToImmutableDictionary(model.Variables));
    }

    private static ImmutableArray<string> ToImmutableArray(List<string>? list) =>
        list?.Count > 0 ? list.ToImmutableArray() : ImmutableArray<string>.Empty;

    private static ImmutableDictionary<string, string> ToImmutableDictionary(Dictionary<string, string>? dict) =>
        dict?.Count > 0 ? dict.ToImmutableDictionary() : ImmutableDictionary<string, string>.Empty;

    private static async Task<MachineProfileYamlModel> DeserializeAsync(string path, CancellationToken cancellationToken)
    {
        string yaml = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        return Deserializer.Deserialize<MachineProfileYamlModel>(yaml);
    }

    private static string? FindProfileFile(string machinesDir, string name)
    {
        foreach (string file in Directory.GetFiles(machinesDir, "*.yaml"))
        {
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
            if (string.Equals(fileNameWithoutExt, name, StringComparison.OrdinalIgnoreCase))
            {
                return file;
            }
        }

        return null;
    }
}
