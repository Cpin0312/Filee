// "Open in mi" (schema 59): text files opened as they are in the mi editor, first on the text donut on macOS.

using Filee.Core.Presets;
using Filee.Core.Profiles;
using Filee.Core.Settings;

namespace Filee.Core.Tests;

public class OpenInMiTests
{
    [Fact]
    public void A_new_library_has_the_tool_first_on_the_text_donut_on_macos()
    {
        var tool = BuiltInData.CreatePresets().Single(p => p.Id == "open-in-mi");
        Assert.True(tool.IsOpenInMiTool);
        Assert.False(tool.IsCropTool);

        var text = BuiltInData.CreateProfiles().Single(p => p.Id == "text");
        if (OperatingSystem.IsMacOS())
            Assert.Equal("open-in-mi", text.PresetIds[0]);
        else
            Assert.DoesNotContain("open-in-mi", text.PresetIds);
    }

    [Fact]
    public void Version_58_libraries_get_the_tool_first_on_their_text_donut()
    {
        var presets = BuiltInData.CreatePresets();
        presets.RemoveAll(p => p.Id == "open-in-mi");
        var profiles = BuiltInData.CreateProfiles();
        var text = profiles.Single(p => p.Id == "text");
        text.PresetIds = ["to-pdf", "my-preset"]; // laid out by the user

        Assert.True(SettingsMigrations.ApplyToLibrary(58, presets, profiles));

        Assert.Equal(presets.FindIndex(p => p.Id == "tool-hide") + 1, presets.FindIndex(p => p.Id == "open-in-mi"));
        Assert.Equal(OperatingSystem.IsMacOS() ? ["open-in-mi", "to-pdf", "my-preset"] : ["to-pdf", "my-preset"], text.PresetIds);
        Assert.False(SettingsMigrations.ApplyToLibrary(58, presets, profiles)); // once only
    }

    [Fact]
    public void A_full_front_gets_the_tool_on_the_back_and_a_user_who_placed_it_keeps_it()
    {
        if (!OperatingSystem.IsMacOS())
            return;
        var presets = BuiltInData.CreatePresets();
        var profiles = BuiltInData.CreateProfiles();
        var text = profiles.Single(p => p.Id == "text");
        text.PresetIds = [.. Enumerable.Range(0, ToolbarProfile.MaxSlices).Select(i => $"p{i}")];
        text.BackPresetIds = ["zip-all"];

        Assert.True(SettingsMigrations.ApplyToLibrary(58, presets, profiles));
        Assert.Equal(["zip-all", "open-in-mi"], text.BackPresetIds);

        text.BackPresetIds = ["open-in-mi", "zip-all"];
        text.PresetIds = ["to-pdf"];
        Assert.False(SettingsMigrations.ApplyToLibrary(58, presets, profiles));
        Assert.Equal(["to-pdf"], text.PresetIds);
    }
}
