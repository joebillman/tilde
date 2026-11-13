using static Tilde.App.NativeMethods;

namespace Tilde.App;

/// <summary>
/// Answers: "if this key-down goes through, which single character will it
/// type?" (ARCHITECTURE.md §5). Asking Windows, rather than using a fixed
/// table, keeps ? and ! right on any keyboard layout.
/// </summary>
internal sealed class KeyTranslator
{
    // Reused on every call. The hook callback must stay fast and should not
    // allocate, and everything runs on one thread, so sharing is safe.
    private readonly byte[] _keyState = new byte[256];
    private readonly char[] _buffer = new char[8];

    /// <summary>The character this key would type, or null for shortcuts, dead keys, and non-character keys.</summary>
    public char? Translate(uint virtualKey, uint scanCode)
    {
        // R8: with Ctrl, Alt, or Windows held, this is a shortcut. AltGr
        // reports as Ctrl+Alt, so it is covered too.
        if (IsDown(VK_CONTROL) || IsDown(VK_MENU) || IsDown(VK_LWIN) || IsDown(VK_RWIN))
        {
            return null;
        }

        // Only Shift and Caps Lock matter for which character a key types.
        Array.Clear(_keyState);
        if (IsDown(VK_SHIFT))
        {
            _keyState[VK_SHIFT] = 0x80;
        }
        if ((GetKeyState(VK_CAPITAL) & 1) != 0)
        {
            _keyState[VK_CAPITAL] = 0x01;
        }

        // Use the layout of the window being typed in, not Tilde's own.
        uint thread = GetWindowThreadProcessId(GetForegroundWindow(), out _);
        IntPtr layout = GetKeyboardLayout(thread);

        // The no-state-change flag matters: without it, asking Windows about a
        // key from inside the hook breaks dead keys (' then e on US-International).
        int count = ToUnicodeEx(virtualKey, scanCode, _keyState, _buffer, _buffer.Length,
                                TOUNICODE_NO_STATE_CHANGE, layout);

        // 1 = one character. 0 = none, negative = dead key, more = ligature.
        char? result = count == 1 ? _buffer[0] : null;

        // Don't leave the last typed character sitting in memory (Hard rule 2).
        Array.Clear(_buffer);
        return result;
    }

    private static bool IsDown(int virtualKey) => (GetAsyncKeyState(virtualKey) & 0x8000) != 0;
}
