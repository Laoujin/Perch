using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Perch.Core.Config;

public sealed class YamlSettingsProvider : ISettingsProvider
{
    private readonly string _settingsPath;

    private static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .Build();

    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public YamlSettingsProvider()
        : this(GetDefaultPath())
    {
    }

    internal YamlSettingsProvider(string settingsPath)
    {
        _settingsPath = settingsPath;
    }

    public async Task<PerchSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_settingsPath))
        {
            return new PerchSettings();
        }

        try
        {
            string yaml = await File.ReadAllTextAsync(_settingsPath, cancellationToken).ConfigureAwait(false);
            return Deserializer.Deserialize<PerchSettings>(yaml) ?? new PerchSettings();
        }
        catch (Exception)
        {
            return new PerchSettings();
        }
    }

    public async Task SaveAsync(PerchSettings settings, CancellationToken cancellationToken = default)
    {
        string? directory = Path.GetDirectoryName(_settingsPath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string yaml = Serializer.Serialize(settings);
        await File.WriteAllTextAsync(_settingsPath, yaml, cancellationToken).ConfigureAwait(false);
    }

    private static string GetDefaultPath() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "perch", "settings.yaml");
}
