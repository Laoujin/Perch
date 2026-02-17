namespace Perch.Core.Startup;

public sealed class NoOpStartupService : IStartupService
{
    public Task<IReadOnlyList<StartupEntry>> GetAllAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<StartupEntry>>([]);

    public Task SetEnabledAsync(StartupEntry entry, bool enabled, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task RemoveAsync(StartupEntry entry, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task AddAsync(string name, string command, StartupSource source, CancellationToken ct = default) =>
        Task.CompletedTask;
}
