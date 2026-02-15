namespace Perch.Core.Modules;

public sealed record ManifestParseResult
{
    public AppManifest? Manifest { get; }
    public string? Error { get; }
    public bool IsSuccess => Manifest != null;

    private ManifestParseResult(AppManifest? manifest, string? error)
    {
        Manifest = manifest;
        Error = error;
    }

    public static ManifestParseResult Success(AppManifest manifest) => new(manifest, null);
    public static ManifestParseResult Failure(string error) => new(null, error);
}
