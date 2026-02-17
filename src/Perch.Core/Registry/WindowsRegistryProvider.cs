using System.Runtime.Versioning;
using Microsoft.Win32;

namespace Perch.Core.Registry;

[SupportedOSPlatform("windows")]
public sealed class WindowsRegistryProvider : IRegistryProvider
{
    public object? GetValue(string keyPath, string valueName)
    {
        ParseKeyPath(keyPath, out RegistryKey hive, out string subKey);
        using RegistryKey? key = hive.OpenSubKey(subKey);
        return key?.GetValue(valueName);
    }

    public void SetValue(string keyPath, string valueName, object value, RegistryValueType kind)
    {
        ParseKeyPath(keyPath, out RegistryKey hive, out string subKey);
        using RegistryKey key = hive.CreateSubKey(subKey);
        key.SetValue(valueName, value, ToRegistryValueKind(kind));
    }

    public IReadOnlyList<RegistryValueEntry> EnumerateValues(string keyPath)
    {
        ParseKeyPath(keyPath, out RegistryKey hive, out string subKey);
        using RegistryKey? key = hive.OpenSubKey(subKey);
        if (key is null)
            return [];

        var names = key.GetValueNames();
        var entries = new List<RegistryValueEntry>(names.Length);
        foreach (var name in names)
        {
            var value = key.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            var kind = key.GetValueKind(name);
            entries.Add(new RegistryValueEntry(name, value, FromRegistryValueKind(kind)));
        }
        return entries;
    }

    public void DeleteValue(string keyPath, string valueName)
    {
        ParseKeyPath(keyPath, out RegistryKey hive, out string subKey);
        using RegistryKey? key = hive.OpenSubKey(subKey, writable: true);
        key?.DeleteValue(valueName, throwOnMissingValue: false);
    }

    private static RegistryValueKind ToRegistryValueKind(RegistryValueType kind) =>
        kind switch
        {
            RegistryValueType.DWord => RegistryValueKind.DWord,
            RegistryValueType.QWord => RegistryValueKind.QWord,
            RegistryValueType.ExpandString => RegistryValueKind.ExpandString,
            RegistryValueType.Binary => RegistryValueKind.Binary,
            _ => RegistryValueKind.String,
        };

    private static RegistryValueType FromRegistryValueKind(RegistryValueKind kind) =>
        kind switch
        {
            RegistryValueKind.DWord => RegistryValueType.DWord,
            RegistryValueKind.QWord => RegistryValueType.QWord,
            RegistryValueKind.ExpandString => RegistryValueType.ExpandString,
            RegistryValueKind.Binary => RegistryValueType.Binary,
            _ => RegistryValueType.String,
        };

    private static void ParseKeyPath(string keyPath, out RegistryKey hive, out string subKey)
    {
        int separatorIndex = keyPath.IndexOf('\\');
        if (separatorIndex < 0)
        {
            throw new ArgumentException($"Invalid registry key path: {keyPath}");
        }

        string hivePrefix = keyPath[..separatorIndex].ToUpperInvariant();
        subKey = keyPath[(separatorIndex + 1)..];

        hive = hivePrefix switch
        {
            "HKCU" or "HKEY_CURRENT_USER" => Microsoft.Win32.Registry.CurrentUser,
            "HKLM" or "HKEY_LOCAL_MACHINE" => Microsoft.Win32.Registry.LocalMachine,
            "HKCR" or "HKEY_CLASSES_ROOT" => Microsoft.Win32.Registry.ClassesRoot,
            "HKU" or "HKEY_USERS" => Microsoft.Win32.Registry.Users,
            "HKCC" or "HKEY_CURRENT_CONFIG" => Microsoft.Win32.Registry.CurrentConfig,
            _ => throw new ArgumentException($"Unknown registry hive: {hivePrefix}"),
        };
    }
}
