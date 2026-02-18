using Perch.Core.Catalog;

namespace Perch.Core.Tweaks;

public interface ITweakService
{
    TweakDetectionResult Detect(TweakCatalogEntry tweak);
    Task<TweakDetectionResult> DetectWithCaptureAsync(TweakCatalogEntry tweak, CancellationToken cancellationToken = default);
    TweakOperationResult Apply(TweakCatalogEntry tweak, bool dryRun = false);
    TweakOperationResult Revert(TweakCatalogEntry tweak, bool dryRun = false);
    Task<TweakOperationResult> RevertToCapturedAsync(TweakCatalogEntry tweak, bool dryRun = false, CancellationToken cancellationToken = default);
}
