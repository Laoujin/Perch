using System.Collections.Immutable;

namespace Perch.Core.Scanner;

public interface IVsCodeService
{
    bool IsInstalled { get; }
    Task<ImmutableArray<DetectedVsCodeExtension>> GetInstalledExtensionsAsync(CancellationToken cancellationToken = default);
}
