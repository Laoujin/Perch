using YamlDotNet.Serialization;

namespace Perch.Core.Registry;

public sealed class CapturedRegistryEntry
{
    [YamlMember(Alias = "value")]
    public string? Value { get; set; }

    [YamlMember(Alias = "kind")]
    public RegistryValueType Kind { get; set; }

    [YamlMember(Alias = "captured-at")]
    public DateTime CapturedAt { get; set; }
}

public sealed class CapturedRegistryData
{
    [YamlMember(Alias = "entries")]
    public Dictionary<string, CapturedRegistryEntry> Entries { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
