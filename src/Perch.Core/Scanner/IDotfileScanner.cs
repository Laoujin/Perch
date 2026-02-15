using System.Collections.Immutable;

namespace Perch.Core.Scanner;

public interface IDotfileScanner
{
    Task<ImmutableArray<DetectedDotfile>> ScanAsync(CancellationToken cancellationToken = default);
}
