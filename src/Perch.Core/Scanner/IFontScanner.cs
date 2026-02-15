using System.Collections.Immutable;

namespace Perch.Core.Scanner;

public interface IFontScanner
{
    Task<ImmutableArray<DetectedFont>> ScanAsync(CancellationToken cancellationToken = default);
}
