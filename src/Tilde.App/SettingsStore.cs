using Tilde.Core;

namespace Tilde.App;

/// <summary>
/// Reads and writes %LOCALAPPDATA%\Tilde\settings.json (ARCHITECTURE.md §9).
/// This is the only file Tilde ever writes, and it holds settings only, never
/// anything the user typed.
/// </summary>
internal static class SettingsStore
{
    private static readonly string FolderPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Tilde");

    public static string FilePath { get; } = Path.Combine(FolderPath, "settings.json");

    /// <summary>Load settings. A missing or unreadable file gives the defaults.</summary>
    public static Settings Load()
    {
        try
        {
            string? json = File.Exists(FilePath) ? File.ReadAllText(FilePath) : null;
            return Settings.FromJson(json);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new Settings();
        }
    }

    /// <summary>Save settings. Failing to save is not worth interrupting the user over.</summary>
    public static void Save(Settings settings)
    {
        try
        {
            Directory.CreateDirectory(FolderPath);

            // Write to a temporary file and then swap it in, so a crash or a
            // full disk mid-write can't leave a half-written settings file.
            string temp = FilePath + ".tmp";
            File.WriteAllText(temp, settings.ToJson());
            File.Move(temp, FilePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Keep running with the settings in memory.
        }
    }
}
