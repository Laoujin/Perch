namespace Perch.Core.Startup;

public sealed record StartupEntry(
    string Id,
    string Name,
    string Command,
    string? ImagePath,
    StartupSource Source,
    bool IsEnabled);
