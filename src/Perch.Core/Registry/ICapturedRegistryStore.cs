namespace Perch.Core.Registry;

public interface ICapturedRegistryStore
{
    Task<CapturedRegistryData> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(CapturedRegistryData data, CancellationToken cancellationToken = default);
}
