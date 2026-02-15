namespace Perch.Core.Backup;

public interface IFileBackupProvider
{
    string BackupFile(string path);
}
