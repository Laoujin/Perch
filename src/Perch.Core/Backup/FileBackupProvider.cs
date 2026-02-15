namespace Perch.Core.Backup;

public sealed class FileBackupProvider : IFileBackupProvider
{
    public string BackupFile(string path)
    {
        string backupPath = path + ".backup";
        int counter = 1;

        while (File.Exists(backupPath) || Directory.Exists(backupPath))
        {
            backupPath = $"{path}.backup.{counter}";
            counter++;
        }

        if (File.Exists(path))
        {
            File.Move(path, backupPath);
        }
        else if (Directory.Exists(path))
        {
            Directory.Move(path, backupPath);
        }

        return backupPath;
    }
}
