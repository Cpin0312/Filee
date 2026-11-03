// "Open in mi": text files go to the mi editor as they are, nothing converted. Which files is the same rule as the
// "Open in mi" button on a result card.

using Filee.Core.Formats;
using Filee.Core.Platform;

namespace Filee.App.Services;

public static class MiEditor
{
    /// <summary>True for a file mi reads: text, data and CSV.</summary>
    public static bool CanOpen(FileFormat format) =>
        format.Category is FormatCategory.Text or FormatCategory.Data || format.Id == "csv";

    /// <summary>Opens those of <paramref name="files"/> mi reads in mi; the others are left out. Nothing without mi.</summary>
    public static void Open(IEnumerable<string> files, IPlatformServices platform)
    {
        if (!platform.HasMi)
            return;
        foreach (var file in files)
        {
            if (File.Exists(file) && FormatRegistry.Detect(file) is { } format && CanOpen(format))
                platform.OpenInMi(file);
        }
    }
}
