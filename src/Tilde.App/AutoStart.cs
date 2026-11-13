using Microsoft.Win32;
using Windows.ApplicationModel;
using static Tilde.App.NativeMethods;

namespace Tilde.App;

/// <summary>Turns "Start with Windows" on and off (ARCHITECTURE.md §9).</summary>
internal interface IAutoStart
{
    /// <summary>Whether Windows will actually start Tilde at login right now.</summary>
    bool IsEnabled { get; }

    /// <summary>Throws <see cref="InvalidOperationException"/> with a user-readable message if Windows refuses.</summary>
    void SetEnabled(bool enabled);
}

internal static class AutoStart
{
    /// <summary>
    /// Pick the right implementation. The Store build runs inside an MSIX
    /// package, where the registry method is silently ignored, so it must use
    /// Windows' StartupTask instead.
    /// </summary>
    public static IAutoStart Create() => IsPackaged() ? new PackagedAutoStart() : new RegistryAutoStart();

    private static bool IsPackaged()
    {
        uint length = 0;
        return GetCurrentPackageFullName(ref length, IntPtr.Zero) != APPMODEL_ERROR_NO_PACKAGE;
    }
}

/// <summary>
/// For builds run from source: a value under the current user's Run key. No
/// admin rights needed. Never turned on by Tilde itself (PRODUCT.md §5).
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

/// <summary>
/// For the Store build: the StartupTask declared in packaging/AppxManifest.xml.
/// The manifest sets it enabled, so Start with Windows is on by default once
/// installed (PRODUCT.md §5).
/// </summary>
internal sealed class PackagedAutoStart : IAutoStart
{
    /// <summary>Must match TaskId in the manifest's windows.startupTask extension.</summary>
    public const string TaskId = "TildeKeysStartup";

    public bool IsEnabled => GetTask().State is StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy;

    public void SetEnabled(bool enabled)
    {
        StartupTask task = GetTask();
        if (!enabled)
        {
            task.Disable();
            return;
        }

        // Windows only lets an app turn its startup task back on if the user
        // didn't switch it off themselves in Settings or Task Manager. In that
        // case the user has to switch it on there; an app can't override them.
        StartupTaskState state = task.RequestEnableAsync().AsTask().GetAwaiter().GetResult();
        if (state is StartupTaskState.DisabledByUser)
        {
            throw new InvalidOperationException(
                "Start with Windows was turned off in Windows Settings. To turn it back on, " +
                "open Settings > Apps > Startup and switch on " + AppInfo.ProductName + ".");
        }
        if (state is StartupTaskState.DisabledByPolicy)
        {
            throw new InvalidOperationException("Your organization's settings don't allow apps to start with Windows.");
        }
    }

    // GetAsync finishes almost instantly. Waiting for it here is safe because
    // Windows completes it on its own thread, not on our UI thread.
    private static StartupTask GetTask() => StartupTask.GetAsync(TaskId).AsTask().GetAwaiter().GetResult();
}
