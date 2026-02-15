namespace Perch.Core.Machines;

public interface IMachineProfileService
{
    Task<MachineProfile?> LoadAsync(string configRepoPath, CancellationToken cancellationToken = default);
}
