using System.Text.Json;

namespace Tilde.Core;

/// <summary>
/// User settings and their defaults (PRODUCT.md §5). The Windows app saves
/// these to %LOCALAPPDATA%\Tilde\settings.json; the parsing and clamping
/// live here so they can be unit tested on any OS.
/// </summary>
public sealed class Settings
{
    public const int ShortHoldDelayMs = 300;
    public const int MediumHoldDelayMs = 500;
    public const int LongHoldDelayMs = 800;
    public const int DefaultCycleDelayMs = 600;
    public const int MinCycleDelayMs = 200;
    public const int MaxCycleDelayMs = 3000;
    public const string DefaultLanguage = "es";

    public static readonly IReadOnlyList<int> AllowedHoldDelays = [ShortHoldDelayMs, MediumHoldDelayMs, LongHoldDelayMs];
    public static readonly IReadOnlyList<string> AllowedLanguages = [DefaultLanguage];

    public bool Enabled { get; set; } = true;
    public string Language { get; set; } = DefaultLanguage;
    public int HoldDelayMs { get; set; } = MediumHoldDelayMs;
    public int CycleDelayMs { get; set; } = DefaultCycleDelayMs;
    public bool StartWithWindows { get; set; } = true;

    /// <summary>True once the How to use window has been shown on first run.</summary>
    public bool HowToShown { get; set; }

    /// <summary>Fix any out-of-range values in place so the rest of the app can trust them.</summary>
    public Settings Clamp()
    {
        HoldDelayMs = NearestHoldDelay(HoldDelayMs);
        CycleDelayMs = Math.Clamp(CycleDelayMs, MinCycleDelayMs, MaxCycleDelayMs);
        if (!AllowedLanguages.Contains(Language))
        {
            Language = DefaultLanguage;
        }
        return this;
    }

    /// <summary>The allowed hold delay closest to <paramref name="ms"/>.</summary>
    public static int NearestHoldDelay(int ms) =>
        AllowedHoldDelays.MinBy(allowed => Math.Abs((long)allowed - ms));

    /// <summary>
    /// Read settings from JSON. Missing, empty, or broken input gives the
    /// defaults rather than an error: a bad settings file must never stop
    /// someone from typing.
    /// </summary>
    public static Settings FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Settings();
        }

        try
        {
            var settings = JsonSerializer.Deserialize<Settings>(json, JsonOptions) ?? new Settings();
            return settings.Clamp(); // also replaces a missing or null language
        }
        catch (JsonException)
        {
            return new Settings();
        }
    }

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };
}
