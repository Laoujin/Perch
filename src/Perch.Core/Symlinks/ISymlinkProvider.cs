namespace Perch.Core.Symlinks;

public interface ISymlinkProvider
{
    void CreateSymlink(string linkPath, string targetPath);
    void CreateJunction(string linkPath, string targetPath);
    bool IsSymlink(string path);
    string? GetSymlinkTarget(string path);
}
