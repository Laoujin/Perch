using System.Collections.Immutable;

namespace Perch.Core.Fonts;

public sealed record FontOnboardingResult(
    ImmutableArray<string> CopiedFiles,
    ImmutableArray<string> Errors);
