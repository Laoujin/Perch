using System.Collections.Immutable;

namespace Perch.Core.Packages;

public sealed record PackageManifestParseResult
{
    public ImmutableArray<PackageDefinition> Packages { get; }
    public ImmutableArray<string> Errors { get; }
    public bool IsSuccess => Packages.Length > 0 || Errors.Length == 0;

    private PackageManifestParseResult(ImmutableArray<PackageDefinition> packages, ImmutableArray<string> errors)
    {
        Packages = packages;
        Errors = errors;
    }

    public static PackageManifestParseResult Success(ImmutableArray<PackageDefinition> packages) =>
        new(packages, ImmutableArray<string>.Empty);

    public static PackageManifestParseResult PartialSuccess(ImmutableArray<PackageDefinition> packages, ImmutableArray<string> errors) =>
        new(packages, errors);

    public static PackageManifestParseResult Failure(ImmutableArray<string> errors) =>
        new(ImmutableArray<PackageDefinition>.Empty, errors);

    public static PackageManifestParseResult Failure(string error) =>
        new(ImmutableArray<PackageDefinition>.Empty, ImmutableArray.Create(error));
}
