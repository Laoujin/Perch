namespace Perch.Core.Registry;

public interface IRegistryProvider
{
    object? GetValue(string keyPath, string valueName);
    void SetValue(string keyPath, string valueName, object value, RegistryValueType kind);
}
