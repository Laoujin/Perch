namespace Perch.Core.Startup;

public interface IStartupService
{
    Task<IReadOnlyList<StartupEntry>> GetAllAsync(CancellationToken ct = default);
    Task SetEnabledAsync(StartupEntry entry, bool enabled, CancellationToken ct = default);
    Task RemoveAsync(StartupEntry entry, CancellationToken ct = default);
    Task AddAsync(string name, string command, StartupSource source, CancellationToken ct = default);
}
