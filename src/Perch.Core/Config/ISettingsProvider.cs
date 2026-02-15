namespace Perch.Core.Config;

public interface ISettingsProvider
{
    Task<PerchSettings> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(PerchSettings settings, CancellationToken cancellationToken = default);
}
