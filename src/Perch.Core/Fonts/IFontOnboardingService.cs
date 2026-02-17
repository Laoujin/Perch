namespace Perch.Core.Fonts;

public interface IFontOnboardingService
{
    Task<FontOnboardingResult> OnboardAsync(
        IReadOnlyList<string> sourcePaths,
        string configRepoPath,
        CancellationToken cancellationToken = default);
}
