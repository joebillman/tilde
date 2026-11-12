using System.ComponentModel;
using System.Runtime.InteropServices;
using Tilde.Core;
using static Tilde.App.NativeMethods;

namespace Tilde.App;

/// <summary>
/// Watches every key press on the system with a low-level keyboard hook and
/// passes it to the hold engine (ARCHITECTURE.md §6).
///
/// The callback runs for every key the user presses in every app, so it must
/// be quick: no file or registry access, no UI, no SendInput. If it is too
/// slow, Windows quietly removes the hook and Tilde stops working.
/// </summary>
internal sealed class KeyboardHook : IDisposable
{
    private readonly HoldEngine _engine;
    private readonly KeyTranslator _translator;
    private readonly AccentMap _map;
    private readonly Func<bool> _isEnabled;

    // Windows only holds a raw pointer to the callback. Keeping the delegate in
    // a field stops the garbage collector from freeing it while the hook is
    // installed; otherwise the next key press would crash the app.
    private readonly LowLevelKeyboardProc _callback;
    private IntPtr _hook;

    public KeyboardHook(HoldEngine engine, KeyTranslator translator, AccentMap map, Func<bool> isEnabled)
    {
        _engine = engine;
        _translator = translator;
        _map = map;
        _isEnabled = isEnabled;
        _callback = Callback;
    }

    /// <summary>Install the hook. Call on the UI thread, which must be running a message loop.</summary>
    public void Install()
    {
        if (_hook != IntPtr.Zero)
        {
            return;
        }
        _hook = SetWindowsHookEx(WH_KEYBOARD_LL, _callback, GetModuleHandle(null), 0);
        if (_hook == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not install the keyboard hook.");
        }
    }

    public void Uninstall()
    {
        if (_hook != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hook);
            _hook = IntPtr.Zero;
        }
    }

    public void Dispose() => Uninstall();

    private IntPtr Callback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            if (nCode >= 0 && ShouldSwallow((int)wParam, lParam))
            {
                return 1; // non-zero tells Windows to drop the key event
            }
        }
        catch
        {
            // An exception escaping into Windows would kill the app and could
            // leave the user's keyboard stuck. Let the key through instead.
        }
        return CallNextHookEx(_hook, nCode, wParam, lParam);
    }

    private bool ShouldSwallow(int message, IntPtr lParam)
    {
        var info = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

        // R12: ignore keys typed by software, including Tilde's own replacement.
        if ((info.flags & LLKHF_INJECTED) != 0)
        {
            return false;
        }

        // R11: when off, change nothing.
        if (!_isEnabled())
        {
            return false;
        }

        int key = (int)info.vkCode;
        switch (message)
        {
            case WM_KEYDOWN:
            case WM_SYSKEYDOWN:
                // A repeat of the held key is decided from the key code alone,
                // so skip the (relatively slow) translation for it.
                char? mapped = null;
                if (key != _engine.HeldKey && _translator.Translate(info.vkCode, info.scanCode) is char c && _map.IsMapped(c))
                {
                    mapped = c;
                }
                return _engine.OnKeyDown(key, mapped);

            case WM_KEYUP:
            case WM_SYSKEYUP:
                _engine.OnKeyUp(key);
                return false; // key-ups always pass through
        }
        return false;
    }
}
