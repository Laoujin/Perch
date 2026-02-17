namespace Perch.Core.Registry;

public interface IRegistryProvider
{
    object? GetValue(string keyPath, string valueName);
    void SetValue(string keyPath, string valueName, object value, RegistryValueType kind);
    IReadOnlyList<RegistryValueEntry> EnumerateValues(string keyPath);
    void DeleteValue(string keyPath, string valueName);
}

public sealed record RegistryValueEntry(string Name, object? Value, RegistryValueType Kind);
