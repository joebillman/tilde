# Tilde architecture

Behavior rules R1–R12 are defined in `PRODUCT.md` §3 and referenced here by
number.

## 1. Stack

- **Language and runtime:** C# on the current .NET LTS release. At the time of writing that is .NET 10; confirm with `dotnet --list-sdks` and Microsoft's .NET support policy before creating projects, and use the newest LTS.
- **UI:** Windows Forms, used only for the tray icon (`NotifyIcon`), its menu, and two small windows. No main window.
- **Platform APIs:** Win32 through P/Invoke: a low-level keyboard hook to observe keys and `SendInput` to type the replacement.
- **Tests:** xUnit, for the Core project only.
- **Distribution:** self-contained publish, packaged as MSIX for the Microsoft Store (`RELEASE.md`).

Why these: see the decision log in `BACKGROUND.md`.

## 2. Repository layout

```
tilde/
  AGENTS.md  CLAUDE.md  README.md  LICENSE  .gitignore
  Tilde.sln
  docs/
  src/
    Tilde.Core/            # net10.0. No Windows or UI references.
      HoldEngine.cs        # the state machine (§4)
      IEngineHost.cs       # what the engine needs from the outside world
      AccentMap.cs         # loads a language file, answers "variants for this char?"
      Languages/es.json    # embedded resource (§10)
      Settings.cs          # settings model and defaults
    Tilde.App/             # net10.0-windows. WinForms + Win32.
      Program.cs           # single-instance check, starts TrayContext
      TrayContext.cs       # ApplicationContext: tray icon, menu, wiring
      KeyboardHook.cs      # WH_KEYBOARD_LL install/remove and callback (§6)
      KeyTranslator.cs     # key event -> character (§5)
      InputSender.cs       # Backspace + Unicode via SendInput (§7)
      EngineHost.cs        # IEngineHost over a WinForms Timer + InputSender
      SettingsStore.cs     # load/save JSON (§9)
      AutoStart.cs         # start-with-Windows, packaged and unpackaged (§9)
      NativeMethods.cs     # all P/Invoke declarations in one place
      HowToWindow.cs  AboutWindow.cs
  tests/
    Tilde.Core.Tests/      # xUnit
  packaging/               # MSIX manifest, assets, build script (M5)
```

The split matters: everything with logic worth testing lives in `Tilde.Core`,
which builds and tests on any OS. `Tilde.App` is a thin Windows shell.

## 3. How a key press flows

```
physical key
  -> Windows calls KeyboardHook callback (UI thread)
       -> ignore if injected (R12) or Tilde disabled (R11)
       -> KeyTranslator: which character would this type? (null if shortcut/unmapped)
       -> HoldEngine.OnKeyDown / OnKeyUp      returns "swallow this event?"
  -> hook returns 1 (swallow) or calls CallNextHookEx (pass through)

later, on the UI thread:
  WinForms Timer tick -> HoldEngine.OnTimer
       -> EngineHost.Replace -> InputSender -> SendInput(Backspace, new char)
```

Everything runs on the single UI thread. There are no locks and no background
threads.

## 4. The hold engine

### Interface to the outside

```csharp
public interface IEngineHost
{
    void StartTimer(int milliseconds);   // one-shot; restarting replaces any pending tick
    void StopTimer();
    void Replace(string text);           // delete the one character showing, type `text`
    long FocusToken { get; }             // changes when the foreground window changes
}

public sealed class HoldEngine
{
    public HoldEngine(IEngineHost host, AccentMap map, Settings settings);
    public int? HeldKey { get; }                       // key code being tracked, if any
    public bool OnKeyDown(int key, char? mappedChar);  // true = swallow the event
    public void OnKeyUp(int key);
    public void OnTimer();
    public void Reset();                               // disable, language change, shutdown
}
```

`key` is an opaque integer identifying the physical key (the virtual-key code).
`mappedChar` is the character this press would type if it is a mapped
character and no shortcut modifier is down, otherwise `null`. The caller may
pass `null` without translating when `key == HeldKey`, because the engine
decides that case from `key` alone.

### States

| State | Meaning |
|---|---|
| Idle | Nothing tracked. |
| Pending | A mapped key is down, its character has been typed, hold delay is running. |
| Active | A variant has replaced the character. Cycle timer runs if there is more than one variant. |
| Suppressed | The hold was cancelled (R10) but the key is still physically down. Swallow its repeats, do nothing else. |

### Transitions

`OnKeyDown(key, mappedChar)`:

1. State is not Idle and `key == HeldKey` → this is auto-repeat. Return **true** (R3). No state change.
2. State is not Idle and `key != HeldKey` → stop the timer, go to Idle (R6), then continue with step 3.
3. `mappedChar` is null → return false (R8, R9).
4. Otherwise → remember `key`, the base character, its variants, and `host.FocusToken`. Start the timer with the hold delay. State = Pending. Return **false** so the character is typed normally (R1).

`OnKeyUp(key)`:

- State is not Idle and `key == HeldKey` → stop the timer, go to Idle (R5).
- Anything else → no change. Key-up events are never swallowed.

`OnTimer()`:

1. State is Idle or Suppressed → return.
2. `host.FocusToken` differs from the one remembered → stop the timer, State = Suppressed (R10). Return.
3. State is Pending → `host.Replace(variants[0])`. State = Active. If there is more than one variant, start the timer with the cycle delay (R2, R4).
4. State is Active → advance through the loop `[variant 1, ..., variant n, base character]`, wrapping to the start; `host.Replace(next)`; start the timer with the cycle delay (R4).

`Reset()`: stop the timer, go to Idle.

The engine never stores anything beyond the single held key and its variants.

## 5. From key event to character

`KeyTranslator` answers: "if this key-down goes through, which single
character will it type?"

1. If Ctrl, Alt, or either Windows key is down (`GetAsyncKeyState`), return null (R8). AltGr reports as Ctrl+Alt, so it is covered.
2. Build a 256-byte key-state array: set the Shift entry to `0x80` if Shift is down, and the Caps Lock entry to `0x01` if Caps Lock is toggled on (`GetKeyState(VK_CAPITAL) & 1`).
3. Get the foreground window's keyboard layout: `GetKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), out _))`.
4. Call `ToUnicodeEx(vk, scanCode, state, buffer, size, flags: 0x4, layout)`. Flag `0x4` tells Windows not to modify its internal keyboard state (available since Windows 10 1607). Without it, calling this from a hook corrupts dead-key typing.
5. If the result is exactly one character, return it. Zero, more than one, or a negative result (dead key) → null.

The engine's caller then asks `AccentMap` whether that character is mapped.

This approach is layout-independent: `?` is recognized as `?` wherever the
layout puts it. If it proves unreliable in testing, the fallback is a fixed
US-layout table (letters by virtual-key code, `?` = Shift+`VK_OEM_2`, `!` =
Shift+`VK_1`); record the switch in the decision log.

## 6. The keyboard hook

`SetWindowsHookEx(WH_KEYBOARD_LL, callback, moduleHandle, 0)` installed on the
UI thread, which must be pumping messages (`Application.Run`).

Callback, in order:

1. `nCode < 0` → `CallNextHookEx`.
2. `flags & LLKHF_INJECTED (0x10)` → `CallNextHookEx` (R12). This also makes Tilde ignore its own output.
3. Tilde disabled → `CallNextHookEx` (R11).
4. Key-down (`WM_KEYDOWN`, `WM_SYSKEYDOWN`): translate unless `vk == engine.HeldKey`, call `engine.OnKeyDown`. If it returns true, return `(IntPtr)1` without calling `CallNextHookEx`.
5. Key-up (`WM_KEYUP`, `WM_SYSKEYUP`): call `engine.OnKeyUp`.
6. `CallNextHookEx`.

Rules for the callback:

- **Keep it fast and allocation-light.** Windows silently removes a low-level hook whose callback is too slow (the limit is the `LowLevelHooksTimeout` registry value, at most 1 second on current Windows). No file, registry, or UI work here.
- **Never call `SendInput` inside the callback.** Replacement happens on the timer tick.
- **Wrap the body in try/catch** that swallows exceptions and passes the key through. An exception escaping into native code kills the process and, worse, can leave the user's typing stuck.
- **Keep the callback delegate in a field** for the life of the hook. If the garbage collector frees it, the next key press crashes the app.
- Unhook on exit (`UnhookWindowsHookEx`).

## 7. Sending the replacement

`InputSender.Replace(text)` makes one `SendInput` call with, in order:

1. `VK_BACK` key-down, `VK_BACK` key-up.
2. For each UTF-16 unit of `text`: a `KEYEVENTF_UNICODE` key-down then key-up, with `wVk = 0` and `wScan` set to the unit.

One call keeps the events contiguous so the user's own typing cannot land
between the Backspace and the new character. Check that the return value equals
the number of events sent.

The `INPUT` structure must match the native size exactly (40 bytes on 64-bit).
Declare it with an explicit union covering mouse, keyboard, and hardware input.
A struct that only declares the keyboard part is too small on 64-bit and
`SendInput` fails without an error you would notice.

`FocusToken` in `EngineHost` is the foreground window handle
(`GetForegroundWindow`) as a `long`.

## 8. Known pitfalls and limits

1. **Backspace is an assumption.** Replacement assumes the typed letter sits just left of the caret. Where typing a letter triggers selection or insertion (browser address-bar autocomplete, spreadsheet cell autocomplete, auto-closing pairs in code editors), Backspace removes the wrong thing. v1 accepts this and documents it. The alternative is a "defer" design that withholds the letter until key-up or timeout; it fixes this class of bug but delays every mapped letter and complicates fast typing, so it was rejected for v1 (see decision log).
2. **Shift is often held during replacement** (`É`, `¿`, `¡`). The injected Backspace therefore arrives as Shift+Backspace. Most apps treat it as Backspace; verify in the manual matrix. If an app misbehaves, inject Shift-up before and Shift-down after the Backspace.
3. **Hook removal.** If Windows drops the hook, Tilde goes quiet with no error. Mitigation (M4): reinstall the hook on session unlock and resume from sleep.
4. **Elevated windows.** A non-elevated hook does not see keys typed into elevated apps. Accepted; do not "fix" by running elevated.
5. **Caps Lock state** read from a hook thread can lag by one event. Test Caps Lock explicitly.
6. **Dead-key layouts** (US-International and others). The `0x4` flag in §5 is what keeps them working; test with US-International enabled.
7. **Games and full-screen apps.** Holding a mapped key (`a`, `e` are common game keys) will trigger a replacement. v1 answer is the tray toggle; M4 adds exclusions.
8. **Packaged autostart.** Inside an MSIX package, writes to the registry `Run` key are virtualized and have no effect. Use the `StartupTask` API there (§9).
9. **Remote desktop and virtual machines.** Not a target for v1; behavior there is untested.
10. **P/Invoke string and character marshaling.** `[DllImport]` defaults to `CharSet.Ansi`, which passes a `char[]` or `string` as 1-byte characters. Every Win32 `...W` or Unicode-only function (`ToUnicodeEx`, `GetModuleHandle`, anything writing UTF-16 into a buffer) must be declared with `CharSet = CharSet.Unicode`. Without it, Windows writes 2-byte characters into a 1-byte buffer and corrupts the heap. The process then dies with `0xC0000374` at some unrelated later moment, which makes the cause hard to see and lets a short test pass by luck. This happened with `ToUnicodeEx` during M2 (fixed in `c8ff9a1`). When adding a native call that takes text, check its `CharSet`, and run the soak test in `TESTING.md` §3.

## 9. Settings and autostart

- `SettingsStore` reads and writes `%LOCALAPPDATA%\Tilde\settings.json` with `System.Text.Json`. Missing or unreadable file → defaults, no crash. Out-of-range values are clamped to the allowed set in `PRODUCT.md` §5.
- `AutoStart` has two implementations behind one interface:
  - **Unpackaged** (development runs): `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` value `Tilde`.
  - **Packaged** (Store build): `Windows.ApplicationModel.StartupTask`, declared in the MSIX manifest.
  - Choose at startup by asking Windows whether the process has package identity (`GetCurrentPackageFullName` returns error 15700 when unpackaged).
- **Single instance:** a named mutex in `Program.cs`. A second launch exits quietly.

## 10. Language data

One JSON file per language, embedded in `Tilde.Core`:

```json
{
  "code": "es",
  "name": "Español",
  "map": {
    "a": ["á"],
    "e": ["é"],
    "i": ["í"],
    "o": ["ó"],
    "u": ["ú", "ü"],
    "n": ["ñ"],
    "?": ["¿"],
    "!": ["¡"]
  }
}
```

`AccentMap` loads it and, for every key that is a lowercase letter, also
registers the uppercase key with uppercase variants (`ToUpperInvariant`).
Loading must reject any variant that is not a single UTF-16 unit, since
replacement sends one Backspace per replacement.

Adding a language later means adding a file and a menu entry. The engine does
not change.
