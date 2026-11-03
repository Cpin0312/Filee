// "Open in mi" on the donut: usable for text files where mi is installed, and it opens them in mi as they are.

using Avalonia.Headless.XUnit;
using Filee.App.Services;
using Filee.Core.Conversion;
using Filee.Core.Formats;
using Filee.Core.Localization;
using Filee.Core.Platform;
using Filee.Core.Presets;

namespace Filee.App.Tests;

public class OpenInMiTests
{
    [AvaloniaFact]
    public void The_tool_is_usable_for_text_files_only_where_mi_is_installed()
    {
        TestServices.EnsureInitialized("en");
        var loc = AppHost.Get<ILocalizer>();
        var tool = BuiltInData.CreatePresets().Single(p => p.Id == "open-in-mi");
        FileFormat txt = FormatRegistry.Get("txt"), md = FormatRegistry.Get("md"), png = FormatRegistry.Get("png");

        var withMi = new PresetAvailability(new ConverterCatalog([]), loc, []) { HasMi = () => true };
        Assert.Equal(PresetUse.Usable, withMi.ForPalette([txt, md]).Judge(tool).Use);
        Assert.Equal(PresetUse.Usable, withMi.ForPalette([png, txt]).Judge(tool).Use); // the picture is left out
        Assert.Equal(PresetUse.Unusable, withMi.ForPalette([png]).Judge(tool).Use);

        var withoutMi = new PresetAvailability(new ConverterCatalog([]), loc, []);
        Assert.Equal(loc["engine.reason.unsupported_platform"], withoutMi.Check(tool, [txt]));
    }

    [Fact]
    public void Only_text_files_that_exist_are_opened_and_nothing_without_mi()
    {
        var dir = Directory.CreateTempSubdirectory("filee-mi-").FullName;
        try
        {
            var text = Path.Combine(dir, "notes.txt");
            var picture = Path.Combine(dir, "a.png");
            File.WriteAllText(text, "hello");
            File.WriteAllBytes(picture, [1]);
            var platform = new MiPlatform { HasMi = true };

            MiEditor.Open([text, picture, Path.Combine(dir, "gone.md")], platform);
            Assert.Equal([text], platform.Opened);

            platform.HasMi = false;
            MiEditor.Open([text], platform);
            Assert.Single(platform.Opened);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    private sealed class MiPlatform : IPlatformServices
    {
        public List<string> Opened { get; } = [];
        public bool HasMi { get; set; }
        public void OpenInMi(string path) => Opened.Add(path);
        public bool IsFileManagerAt(int x, int y) => false;
        public string? ProcessNameAt(int x, int y) => null;
        public IReadOnlyList<string> GetFileManagerSelection() => [];
        public int SystemDragThreshold => 4;
        public bool PrefersReducedMotion => false;
        public void SetStartWithSystem(bool enabled, string executablePath) { }
        public void SetContextMenu(bool enabled, string executablePath, string label) { }
        public ModernContextMenuState GetModernContextMenuState(string executablePath) => ModernContextMenuState.Unsupported;
        public Task<ModernContextMenuResult> SetModernContextMenuAsync(bool enabled, string executablePath) => Task.FromResult(new ModernContextMenuResult(false));
        public void RevealInFileManager(string path) { }
        public void OpenFile(string path) { }
        public void RaiseTopmost(nint windowHandle) { }
    }
}
