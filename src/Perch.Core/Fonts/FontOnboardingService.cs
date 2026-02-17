using System.Collections.Immutable;

namespace Perch.Core.Fonts;

public sealed class FontOnboardingService : IFontOnboardingService
{
    public async Task<FontOnboardingResult> OnboardAsync(
        IReadOnlyList<string> sourcePaths,
        string configRepoPath,
        CancellationToken cancellationToken = default)
    {
        var fontsDir = Path.Combine(configRepoPath, "fonts");
        Directory.CreateDirectory(fontsDir);

        var copied = ImmutableArray.CreateBuilder<string>();
        var errors = ImmutableArray.CreateBuilder<string>();

        foreach (var source in sourcePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var fileName = Path.GetFileName(source);
                var dest = Path.Combine(fontsDir, fileName);

                if (File.Exists(dest) && await AreBytesIdenticalAsync(source, dest, cancellationToken))
                    continue;

                File.Copy(source, dest, overwrite: true);
                copied.Add(fileName);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                errors.Add($"{Path.GetFileName(source)}: {ex.Message}");
            }
        }

        return new FontOnboardingResult(copied.ToImmutable(), errors.ToImmutable());
    }

    private static async Task<bool> AreBytesIdenticalAsync(
        string path1, string path2, CancellationToken cancellationToken)
    {
        var info1 = new FileInfo(path1);
        var info2 = new FileInfo(path2);
        if (info1.Length != info2.Length)
            return false;

        const int bufferSize = 81920;
        var buf1 = new byte[bufferSize];
        var buf2 = new byte[bufferSize];

        await using var stream1 = File.OpenRead(path1);
        await using var stream2 = File.OpenRead(path2);

        int read1;
        while ((read1 = await stream1.ReadAsync(buf1, cancellationToken)) > 0)
        {
            var read2 = await stream2.ReadAsync(buf2.AsMemory(0, read1), cancellationToken);
            if (read1 != read2 || !buf1.AsSpan(0, read1).SequenceEqual(buf2.AsSpan(0, read2)))
                return false;
        }

        return true;
    }
}
