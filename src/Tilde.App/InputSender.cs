using System.Runtime.InteropServices;
using static Tilde.App.NativeMethods;

namespace Tilde.App;

/// <summary>
/// Types the replacement: one Backspace to delete the plain letter, then the
/// accented character (ARCHITECTURE.md §7). Never call this from the hook
/// callback; it runs on the timer tick.
/// </summary>
internal static class InputSender
{
    private static readonly int InputSize = Marshal.SizeOf<INPUT>();

    /// <summary>
    /// Fail loudly at startup if INPUT has the wrong size, rather than having
    /// SendInput silently type nothing later. Native size: 40 bytes on 64-bit, 28 on 32-bit.
    /// </summary>
    public static void CheckStructSize()
    {
        int expected = IntPtr.Size == 8 ? 40 : 28;
        if (InputSize != expected)
        {
            throw new InvalidOperationException($"INPUT is {InputSize} bytes; Windows expects {expected}.");
        }
    }

    /// <summary>Returns false if Windows did not accept every event.</summary>
    public static bool Replace(string text)
    {
        var inputs = new INPUT[2 + 2 * text.Length];
        int i = 0;
        inputs[i++] = Key(VK_BACK, 0, 0);
        inputs[i++] = Key(VK_BACK, 0, KEYEVENTF_KEYUP);
        foreach (char unit in text)
        {
            // KEYEVENTF_UNICODE types the character directly, whatever the
            // keyboard layout, with no virtual-key code.
            inputs[i++] = Key(0, unit, KEYEVENTF_UNICODE);
            inputs[i++] = Key(0, unit, KEYEVENTF_UNICODE | KEYEVENTF_KEYUP);
        }

        // One call keeps the events together, so a key the user presses at
        // the same moment cannot land between the Backspace and the new letter.
        uint sent = SendInput((uint)inputs.Length, inputs, InputSize);
        return sent == inputs.Length;
    }

    private static INPUT Key(ushort virtualKey, ushort scan, uint flags) => new()
    {
        type = INPUT_KEYBOARD,
        u = new InputUnion { ki = new KEYBDINPUT { wVk = virtualKey, wScan = scan, dwFlags = flags } },
    };
}
