using System.Runtime.Versioning;
using System.Text.RegularExpressions;

using Perch.Core.Registry;

namespace Perch.Core.Startup;

[SupportedOSPlatform("windows")]
public sealed partial class WindowsStartupService : IStartupService
{
    private const string HkcuRunKey = @"HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string HklmRunKey = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string HkcuApprovedKey = @"HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
    private const string HklmApprovedKey = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";

    private const string HkcuStartupApprovedFolder =
        @"HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\StartupFolder";

    private readonly IRegistryProvider _registry;
    private readonly string _userStartupFolder;
    private readonly string _allUsersStartupFolder;

    public WindowsStartupService(IRegistryProvider registry)
        : this(registry,
            Environment.GetFolderPath(Environment.SpecialFolder.Startup),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup))
    {
    }

    internal WindowsStartupService(IRegistryProvider registry, string userStartupFolder, string allUsersStartupFolder)
    {
        _registry = registry;
        _userStartupFolder = userStartupFolder;
        _allUsersStartupFolder = allUsersStartupFolder;
    }

    public Task<IReadOnlyList<StartupEntry>> GetAllAsync(CancellationToken ct = default)
    {
        var entries = new List<StartupEntry>();

        AddRegistryEntries(entries, HkcuRunKey, HkcuApprovedKey, StartupSource.RegistryCurrentUser);
        AddRegistryEntries(entries, HklmRunKey, HklmApprovedKey, StartupSource.RegistryLocalMachine);
        AddStartupFolderEntries(entries, _userStartupFolder, HkcuStartupApprovedFolder, StartupSource.StartupFolderUser);
        AddStartupFolderEntries(entries, _allUsersStartupFolder, null, StartupSource.StartupFolderAllUsers);

        return Task.FromResult<IReadOnlyList<StartupEntry>>(entries);
    }

    public Task SetEnabledAsync(StartupEntry entry, bool enabled, CancellationToken ct = default)
    {
        var approvedKey = entry.Source switch
        {
            StartupSource.RegistryCurrentUser => HkcuApprovedKey,
            StartupSource.RegistryLocalMachine => HklmApprovedKey,
            StartupSource.StartupFolderUser => HkcuStartupApprovedFolder,
            _ => null,
        };

        if (approvedKey is null)
            return Task.CompletedTask;

        var bytes = new byte[12];
        bytes[0] = enabled ? (byte)0x02 : (byte)0x03;
        _registry.SetValue(approvedKey, entry.Name, bytes, RegistryValueType.Binary);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(StartupEntry entry, CancellationToken ct = default)
    {
        switch (entry.Source)
        {
            case StartupSource.RegistryCurrentUser:
                _registry.DeleteValue(HkcuRunKey, entry.Name);
                _registry.DeleteValue(HkcuApprovedKey, entry.Name);
                break;
            case StartupSource.RegistryLocalMachine:
                _registry.DeleteValue(HklmRunKey, entry.Name);
                _registry.DeleteValue(HklmApprovedKey, entry.Name);
                break;
            case StartupSource.StartupFolderUser:
            {
                var path = Path.Combine(_userStartupFolder, entry.Name);
                if (File.Exists(path))
                    File.Delete(path);
                _registry.DeleteValue(HkcuStartupApprovedFolder, entry.Name);
                break;
            }
            case StartupSource.StartupFolderAllUsers:
            {
                var path = Path.Combine(_allUsersStartupFolder, entry.Name);
                if (File.Exists(path))
                    File.Delete(path);
                break;
            }
        }

        return Task.CompletedTask;
    }

    public Task AddAsync(string name, string command, StartupSource source, CancellationToken ct = default)
    {
        if (source is StartupSource.RegistryCurrentUser or StartupSource.RegistryLocalMachine)
        {
            var runKey = source == StartupSource.RegistryCurrentUser ? HkcuRunKey : HklmRunKey;
            _registry.SetValue(runKey, name, command, RegistryValueType.String);
        }

        return Task.CompletedTask;
    }

    private void AddRegistryEntries(List<StartupEntry> entries, string runKey, string approvedKey, StartupSource source)
    {
        var values = _registry.EnumerateValues(runKey);
        var approved = BuildApprovedMap(approvedKey);

        foreach (var val in values)
        {
            var command = val.Value?.ToString() ?? string.Empty;
            var isEnabled = !approved.TryGetValue(val.Name, out bool disabled) || !disabled;
            entries.Add(new StartupEntry(
                Id: $"{source}:{val.Name}",
                Name: val.Name,
                Command: command,
                ImagePath: ExtractImagePath(command),
                Source: source,
                IsEnabled: isEnabled));
        }
    }

    private void AddStartupFolderEntries(List<StartupEntry> entries, string folder, string? approvedKey, StartupSource source)
    {
        if (!Directory.Exists(folder))
            return;

        var approved = approvedKey is not null ? BuildApprovedMap(approvedKey) : new Dictionary<string, bool>();

        foreach (var file in Directory.EnumerateFiles(folder, "*.lnk"))
        {
            var fileName = Path.GetFileName(file);
            var isEnabled = !approved.TryGetValue(fileName, out bool disabled) || !disabled;
            entries.Add(new StartupEntry(
                Id: $"{source}:{fileName}",
                Name: fileName,
                Command: file,
                ImagePath: file,
                Source: source,
                IsEnabled: isEnabled));
        }
    }

    private Dictionary<string, bool> BuildApprovedMap(string approvedKey)
    {
        var map = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        var approvedValues = _registry.EnumerateValues(approvedKey);
        foreach (var av in approvedValues)
        {
            if (av.Value is byte[] bytes && bytes.Length >= 1)
                map[av.Name] = bytes[0] == 0x03;
        }
        return map;
    }

    internal static string? ExtractImagePath(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return null;

        var match = ImagePathRegex().Match(command);
        return match.Success ? match.Groups[1].Value : null;
    }

    [GeneratedRegex("""^"?([^"]+\.exe)""", RegexOptions.IgnoreCase)]
    private static partial Regex ImagePathRegex();
}
