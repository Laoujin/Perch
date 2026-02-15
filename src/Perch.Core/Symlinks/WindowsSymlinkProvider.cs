using System.Diagnostics;

namespace Perch.Core.Symlinks;

public sealed class WindowsSymlinkProvider : ISymlinkProvider
{
    public void CreateSymlink(string linkPath, string targetPath)
    {
        if (File.Exists(targetPath))
        {
            File.CreateSymbolicLink(linkPath, targetPath);
        }
        else if (Directory.Exists(targetPath))
        {
            Directory.CreateSymbolicLink(linkPath, targetPath);
        }
        else
        {
            File.CreateSymbolicLink(linkPath, targetPath);
        }
    }

    public void CreateJunction(string linkPath, string targetPath)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c mklink /J \"{linkPath}\" \"{targetPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        });
        process!.WaitForExit();
        if (process.ExitCode != 0)
        {
            string error = process.StandardError.ReadToEnd();
            throw new IOException($"Failed to create junction: {error}");
        }
    }

    public bool IsSymlink(string path)
    {
        var info = new FileInfo(path);
        if (info.Exists)
        {
            return info.LinkTarget != null;
        }

        var dirInfo = new DirectoryInfo(path);
        if (dirInfo.Exists)
        {
            return dirInfo.LinkTarget != null;
        }

        return false;
    }

    public string? GetSymlinkTarget(string path)
    {
        var info = new FileInfo(path);
        if (info.Exists)
        {
            return info.LinkTarget;
        }

        var dirInfo = new DirectoryInfo(path);
        return dirInfo.Exists ? dirInfo.LinkTarget : null;
    }
}
