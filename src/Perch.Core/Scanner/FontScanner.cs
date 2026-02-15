using System.Collections.Immutable;

namespace Perch.Core.Scanner;

public sealed class FontScanner : IFontScanner
{
    private static readonly string[] FontExtensions = [".ttf", ".otf", ".ttc", ".woff", ".woff2"];

    public Task<ImmutableArray<DetectedFont>> ScanAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<DetectedFont>();

        foreach (string fontDir in GetFontDirectories())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!Directory.Exists(fontDir))
            {
                continue;
            }

            foreach (string file in Directory.EnumerateFiles(fontDir))
            {
                cancellationToken.ThrowIfCancellationRequested();
                string ext = Path.GetExtension(file);
                if (FontExtensions.Any(e => string.Equals(e, ext, StringComparison.OrdinalIgnoreCase)))
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    results.Add(new DetectedFont(name, null, file));
                }
            }
        }

        return Task.FromResult(results.ToImmutableArray());
    }

    private static IEnumerable<string> GetFontDirectories()
    {
        if (OperatingSystem.IsWindows())
        {
            yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));
            string localFonts = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Microsoft", "Windows", "Fonts");
            yield return localFonts;
        }
        else if (OperatingSystem.IsLinux())
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            yield return Path.Combine(home, ".local", "share", "fonts");
            yield return "/usr/share/fonts";
            yield return "/usr/local/share/fonts";
        }
        else if (OperatingSystem.IsMacOS())
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            yield return Path.Combine(home, "Library", "Fonts");
            yield return "/Library/Fonts";
            yield return "/System/Library/Fonts";
        }
    }
}
