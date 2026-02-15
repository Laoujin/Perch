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
        string? profilePath = FindProfileFile(machinesDir, hostname);
        if (profilePath == null)
        {
            return null;
        }

        string yaml = await File.ReadAllTextAsync(profilePath, cancellationToken).ConfigureAwait(false);
        MachineProfileYamlModel model = Deserializer.Deserialize<MachineProfileYamlModel>(yaml);

        ImmutableArray<string> includes = model.IncludeModules?.Count > 0
            ? model.IncludeModules.ToImmutableArray()
            : ImmutableArray<string>.Empty;
        ImmutableArray<string> excludes = model.ExcludeModules?.Count > 0
            ? model.ExcludeModules.ToImmutableArray()
            : ImmutableArray<string>.Empty;

        return new MachineProfile(includes, excludes);
    }

    private static string? FindProfileFile(string machinesDir, string hostname)
    {
        foreach (string file in Directory.GetFiles(machinesDir, "*.yaml"))
        {
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
            if (string.Equals(fileNameWithoutExt, hostname, StringComparison.OrdinalIgnoreCase))
            {
                return file;
            }
        }

        return null;
    }
}
