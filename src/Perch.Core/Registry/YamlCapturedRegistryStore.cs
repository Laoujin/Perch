using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Perch.Core.Registry;

public sealed class YamlCapturedRegistryStore : ICapturedRegistryStore
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .Build();

    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public YamlCapturedRegistryStore()
        : this(GetDefaultPath())
    {
    }

    internal YamlCapturedRegistryStore(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<CapturedRegistryData> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!File.Exists(_filePath))
                return new CapturedRegistryData();

            string yaml = await File.ReadAllTextAsync(_filePath, cancellationToken).ConfigureAwait(false);
            return Deserializer.Deserialize<CapturedRegistryData>(yaml) ?? new CapturedRegistryData();
        }
        catch (Exception)
        {
            return new CapturedRegistryData();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveAsync(CapturedRegistryData data, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            string? directory = Path.GetDirectoryName(_filePath);
            if (directory != null && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string yaml = Serializer.Serialize(data);
            await File.WriteAllTextAsync(_filePath, yaml, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    private static string GetDefaultPath() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "perch", "captured-registry.yaml");
}
