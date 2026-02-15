using Perch.Core.Backup;
using Perch.Core.Deploy;
using Perch.Core.Modules;

namespace Perch.Core.Symlinks;

public sealed class SymlinkOrchestrator
{
    private readonly ISymlinkProvider _symlinkProvider;
    private readonly IFileBackupProvider _backupProvider;

    public SymlinkOrchestrator(ISymlinkProvider symlinkProvider, IFileBackupProvider backupProvider)
    {
        _symlinkProvider = symlinkProvider;
        _backupProvider = backupProvider;
    }

    public DeployResult ProcessLink(string moduleName, string sourcePath, string targetPath, LinkType linkType)
    {
        try
        {
            string? targetDir = Path.GetDirectoryName(targetPath);
            if (targetDir != null && !Directory.Exists(targetDir))
            {
                return new DeployResult(moduleName, sourcePath, targetPath, ResultLevel.Error,
                    $"Parent directory does not exist: {targetDir}");
            }

            if (_symlinkProvider.IsSymlink(targetPath))
            {
                string? existingTarget = _symlinkProvider.GetSymlinkTarget(targetPath);
                if (string.Equals(existingTarget, sourcePath, StringComparison.OrdinalIgnoreCase))
                {
                    return new DeployResult(moduleName, sourcePath, targetPath, ResultLevel.Ok,
                        "Already linked (skipped)");
                }
            }

            if (File.Exists(targetPath) || Directory.Exists(targetPath))
            {
                string backupPath = _backupProvider.BackupFile(targetPath);
                CreateLink(targetPath, sourcePath, linkType);
                return new DeployResult(moduleName, sourcePath, targetPath, ResultLevel.Warning,
                    $"Backed up existing to {backupPath}, created link");
            }

            CreateLink(targetPath, sourcePath, linkType);
            return new DeployResult(moduleName, sourcePath, targetPath, ResultLevel.Ok, "Created link");
        }
        catch (Exception ex)
        {
            return new DeployResult(moduleName, sourcePath, targetPath, ResultLevel.Error, ex.Message);
        }
    }

    private void CreateLink(string linkPath, string targetPath, LinkType linkType)
    {
        if (linkType == LinkType.Junction)
        {
            _symlinkProvider.CreateJunction(linkPath, targetPath);
        }
        else
        {
            _symlinkProvider.CreateSymlink(linkPath, targetPath);
        }
    }
}
