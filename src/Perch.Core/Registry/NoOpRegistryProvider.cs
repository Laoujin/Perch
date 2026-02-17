namespace Perch.Core.Registry;

public sealed class NoOpRegistryProvider : IRegistryProvider
{
    public object? GetValue(string keyPath, string valueName) => null;

    public void SetValue(string keyPath, string valueName, object value, RegistryValueType kind) { }

    public IReadOnlyList<RegistryValueEntry> EnumerateValues(string keyPath) => [];

    public void DeleteValue(string keyPath, string valueName) { }
}
