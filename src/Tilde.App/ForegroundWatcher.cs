using Tilde.Core;
using static Tilde.App.NativeMethods;

namespace Tilde.App;

/// <summary>
/// Keeps track of whether the program in front is on the excluded list (R13).
///
/// Windows tells us whenever a different window comes to the front, so the
/// check happens then, once, instead of inside the keyboard hook on every key
/// press, where looking up a program's name would be too slow. The program
/// name is compared with the list and then forgotten.
/// </summary>
internal sealed class ForegroundWatcher : IDisposable
{
    private readonly Settings _settings;
    private readonly Action _enteredExcludedApp;

    // Same reason as in KeyboardHook: Windows holds only a raw pointer to this.
    private readonly WinEventProc _callback;
    private IntPtr _hook;

    // Reused buffer for the program path; only used on the UI thread.
    private readonly char[] _pathBuffer = new char[1024];

    public ForegroundWatcher(Settings settings, Action enteredExcludedApp)
    {
        _settings = settings;
        _enteredExcludedApp = enteredExcludedApp;
        _callback = OnForegroundChanged;
    }

    /// <summary>True while an excluded program is in front. Cheap to read from the keyboard hook.</summary>
    public bool ForegroundIsExcluded { get; private set; }

    /// <summary>Start listening. Call on the UI thread.</summary>
    public void Start()
    {
        if (_hook == IntPtr.Zero)
        {
            _hook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero,
                                    _callback, 0, 0, WINEVENT_OUTOFCONTEXT);
        }
        Refresh();
    }

    /// <summary>Check the program in front now, for example after the list changes.</summary>
    public void Refresh()
    {
        bool wasExcluded = ForegroundIsExcluded;
        ForegroundIsExcluded = _settings.ExcludedApps.Count > 0 &&
                               ExclusionList.Contains(_settings.ExcludedApps, ForegroundProgramName());
        if (ForegroundIsExcluded && !wasExcluded)
        {
            _enteredExcludedApp();
        }
    }

    public void Dispose()
    {
        if (_hook != IntPtr.Zero)
        {
            UnhookWinEvent(_hook);
            _hook = IntPtr.Zero;
        }
    }

    private void OnForegroundChanged(IntPtr hook, uint eventType, IntPtr hwnd,
                                     int idObject, int idChild, uint thread, uint time)
    {
        try
        {
            Refresh();
        }
        catch
        {
            // Never let an exception escape into Windows (same rule as the keyboard hook).
        }
    }

    /// <summary>File name of the program that owns the foreground window, like "notepad.exe", or null.</summary>
    private string? ForegroundProgramName()
    {
        GetWindowThreadProcessId(GetForegroundWindow(), out uint processId);
        if (processId == 0)
        {
            return null;
        }

        // Limited access is enough to read the name and works for other users'
        // programs. It fails for elevated programs, which Tilde can't type into anyway.
        IntPtr process = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, processId);
        if (process == IntPtr.Zero)
        {
            return null;
        }
        try
        {
            uint length = (uint)_pathBuffer.Length;
            if (!QueryFullProcessImageName(process, 0, _pathBuffer, ref length))
            {
                return null;
            }
            return Path.GetFileName(new string(_pathBuffer, 0, (int)length));
        }
        finally
        {
            CloseHandle(process);
        }
    }
}
