using System.Collections.Immutable;

namespace Perch.Core.Fonts;

public sealed class FontManifestParseResult
{
    public bool IsSuccess { get; }
    public ImmutableArray<string> FontIds { get; }
    public string? Error { get; }

    private FontManifestParseResult(bool isSuccess, ImmutableArray<string> fontIds, string? error)
    {
        IsSuccess = isSuccess;
        FontIds = fontIds;
        Error = error;
    }

    public static FontManifestParseResult Ok(ImmutableArray<string> fontIds) => new(true, fontIds, null);
    public static FontManifestParseResult Failure(string error) => new(false, ImmutableArray<string>.Empty, error);
}
