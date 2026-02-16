namespace Perch.Core.Packages;

public sealed class InstallManifestParseResult
{
    public bool IsSuccess { get; }
    public InstallManifest? Manifest { get; }
    public string? Error { get; }

    private InstallManifestParseResult(bool isSuccess, InstallManifest? manifest, string? error)
    {
        IsSuccess = isSuccess;
        Manifest = manifest;
        Error = error;
    }

    public static InstallManifestParseResult Ok(InstallManifest manifest) => new(true, manifest, null);
    public static InstallManifestParseResult Failure(string error) => new(false, null, error);
}
