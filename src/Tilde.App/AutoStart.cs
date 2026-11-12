using Microsoft.Win32;

namespace Tilde.App;

/// <summary>Turns "Start with Windows" on and off (ARCHITECTURE.md §9).</summary>
internal interface IAutoStart
{
    /// <summary>Whether Windows will actually start Tilde at login right now.</summary>
    bool IsEnabled { get; }

    void SetEnabled(bool enabled);
}

internal static class AutoStart
{
    /// <summary>
    /// Pick the right implementation. For now there is only the unpackaged one;
    /// M5 adds the packaged (Store) version, chosen when the app has package identity.
    /// </summary>
    public static IAutoStart Create() => new RegistryAutoStart();
}

/// <summary>
/// For builds run from source: a value under the current user's Run key. No
/// admin rights needed. Inside an MSIX package this key is virtualized and
/// does nothing, which is why the Store build will use a different method.
/// </summary>
internal sealed class RegistryAutoStart : IAutoStart
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "Tilde";

    public bool IsEnabled
    {
        get
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
            return key?.GetValue(ValueName) is string;
        }
    }

    public void SetEnabled(bool enabled)
    {
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
        if (enabled)
        {
            // Quoted, because the path may contain spaces.
            key.SetValue(ValueName, $"\"{Environment.ProcessPath}\"");
        }
        else
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
        }
    }
}
