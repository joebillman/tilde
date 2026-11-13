using Tilde.Core;

namespace Tilde.Core.Tests;

/// <summary>Exclusion list tests X1–X6 from docs/TESTING.md §2 (rule R13).</summary>
public class ExclusionListTests
{
    [Theory]
    [InlineData("minecraft.exe")]
    [InlineData("Minecraft.EXE")]
    [InlineData("  minecraft  ")]
    [InlineData(@"C:\Games\minecraft.exe")]
    [InlineData("C:/Games/Minecraft")]
    public void X1_NamesAreNormalized(string typed)
    {
        Assert.Equal("minecraft.exe", ExclusionList.NormalizeName(typed));
    }

    [Fact]
    public void X2_Parse_DropsBlanksAndDuplicatesAndKeepsOrder()
    {
        string text = "roblox\r\n\r\n  \nMinecraft.exe\nROBLOX.EXE\r\nnotepad";

        Assert.Equal(["roblox.exe", "minecraft.exe", "notepad.exe"], ExclusionList.Parse(text));
    }

    [Theory]
    [InlineData("game?.exe")]
    [InlineData("a*b")]
    [InlineData("<bad>")]
    [InlineData("pipe|name")]
    [InlineData("quote\"d")]
    [InlineData("")]
    [InlineData(@"C:\Games\")]
    public void X3_BadNamesAreDropped(string typed)
    {
        Assert.Null(ExclusionList.NormalizeName(typed));
        Assert.Empty(ExclusionList.Parse(typed));
    }

    [Fact]
    public void X4_Lookup_IgnoresCase()
    {
        var list = ExclusionList.Parse("minecraft.exe");

        Assert.True(ExclusionList.Contains(list, "Minecraft.exe"));
        Assert.True(ExclusionList.Contains(list, "MINECRAFT.EXE"));
        Assert.False(ExclusionList.Contains(list, "notepad.exe"));
        Assert.False(ExclusionList.Contains(list, null));
    }

    [Fact]
    public void X5_KeepsAtMostOneHundred()
    {
        var names = Enumerable.Range(1, 150).Select(i => $"game{i}");

        var list = ExclusionList.Normalize(names);

        Assert.Equal(100, list.Count);
        Assert.Equal("game1.exe", list[0]);
        Assert.Equal("game100.exe", list[^1]);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("""{ "ExcludedApps": null }""")]
    public void X6_MissingOrNullList_LoadsEmpty(string json)
    {
        Assert.Empty(Settings.FromJson(json).ExcludedApps);
    }

    [Fact]
    public void X6_ListSurvivesSaveAndLoad()
    {
        var original = new Settings { ExcludedApps = ExclusionList.Parse("Roblox\nminecraft.exe") };

        var loaded = Settings.FromJson(original.ToJson());

        Assert.Equal(["roblox.exe", "minecraft.exe"], loaded.ExcludedApps);
    }

    [Fact]
    public void X6_HandEditedFile_IsCleanedOnLoad()
    {
        var loaded = Settings.FromJson("""{ "ExcludedApps": ["Roblox.EXE", "", "roblox", "a?b"] }""");

        Assert.Equal(["roblox.exe"], loaded.ExcludedApps);
    }

    [Fact]
    public void Format_PutsOneNamePerLine()
    {
        Assert.Equal($"a.exe{Environment.NewLine}b.exe", ExclusionList.Format(["a.exe", "b.exe"]));
    }
}
