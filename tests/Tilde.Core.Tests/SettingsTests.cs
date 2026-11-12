using Tilde.Core;

namespace Tilde.Core.Tests;

/// <summary>Settings tests from docs/TESTING.md §2.</summary>
public class SettingsTests
{
    [Fact]
    public void Defaults_MatchProductSpec()
    {
        var s = new Settings();

        Assert.True(s.Enabled);
        Assert.Equal("es", s.Language);
        Assert.Equal(500, s.HoldDelayMs);
        Assert.Equal(600, s.CycleDelayMs);
        Assert.True(s.StartWithWindows);
        Assert.False(s.HowToShown);
    }

    [Theory]
    [InlineData(0, 300)]
    [InlineData(350, 300)]
    [InlineData(450, 500)]
    [InlineData(500, 500)]
    [InlineData(700, 800)]
    [InlineData(99999, 800)]
    [InlineData(-5, 300)]
    public void OutOfRangeHoldDelay_IsClampedToNearestAllowed(int stored, int expected)
    {
        var s = Settings.FromJson($$"""{ "HoldDelayMs": {{stored}} }""");
        Assert.Equal(expected, s.HoldDelayMs);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not json {")]
    [InlineData("""{ "HoldDelayMs": "slow" }""")]
    [InlineData("[]")]
    public void UnreadableJson_GivesDefaults(string? json)
    {
        var s = Settings.FromJson(json);

        Assert.True(s.Enabled);
        Assert.Equal(500, s.HoldDelayMs);
        Assert.Equal("es", s.Language);
    }

    [Fact]
    public void UnknownLanguage_FallsBackToSpanish()
    {
        Assert.Equal("es", Settings.FromJson("""{ "Language": "fr" }""").Language);
        Assert.Equal("es", Settings.FromJson("""{ "Language": null }""").Language);
    }

    [Fact]
    public void RoundTrip_KeepsValues()
    {
        var original = new Settings { Enabled = false, HoldDelayMs = 800, StartWithWindows = false, HowToShown = true };

        var loaded = Settings.FromJson(original.ToJson());

        Assert.False(loaded.Enabled);
        Assert.Equal(800, loaded.HoldDelayMs);
        Assert.False(loaded.StartWithWindows);
        Assert.True(loaded.HowToShown);
    }
}
