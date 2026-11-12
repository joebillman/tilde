# Tilde testing

## 1. What is tested how

| Layer | Method | Who |
|---|---|---|
| `Tilde.Core` (engine, map, settings) | xUnit, runs on any OS | agent or human, every change |
| Hook, translation, sending | manual smoke tests (§3) | human at a real keyboard |
| Behavior across applications | manual matrix (§4) | human, before release |

The hook ignores injected input (R12), so automation tools that simulate key
presses cannot test it. That is intentional. Do not weaken R12 to make
automated testing possible.

## 2. Unit tests for the engine

Use a fake `IEngineHost` that records `StartTimer`, `StopTimer`, and `Replace`
calls and lets the test set `FocusToken`. Tests drive time by calling
`OnTimer()` directly. Defaults: hold delay 500, cycle delay 600.

Rule numbers refer to `PRODUCT.md` §3.

| # | Scenario | Expect |
|---|---|---|
| T1 | Down `e` (mapped), up `e`, no timer | Down returns false; timer started 500 then stopped; no Replace (R1) |
| T2 | Down `e`, timer | Replace(`é`); no further timer started (R2, single variant) |
| T3 | Down `e`, repeat down `e` ×3 | Repeats return true; first down returned false (R3) |
| T4 | Down `e`, timer, repeat down `e` | Repeat returns true; still one Replace |
| T5 | Down `u`, timer ×4 | Replace `ú`, `ü`, `u`, `ú` in that order; timer restarted with 600 after each (R4) |
| T6 | Down `u`, timer, up `u`, timer | One Replace (`ú`); the late timer does nothing (R5) |
| T7 | Down `e`, down `s` (unmapped, null) | Timer stopped; `s` returns false; later timer does nothing (R6) |
| T8 | Down `e`, down `n` (mapped), timer | Replace(`ñ`) only; held key is now `n` (R6) |
| T9 | Down `e`, down `n`, up `e`, timer | Up of the old key does not cancel; Replace(`ñ`) |
| T10 | Down `E` (uppercase char), timer | Replace(`É`) (R7) |
| T11 | Down `U`, timer ×3 | `Ú`, `Ü`, `U` (R7 with R4) |
| T12 | Down `?`, timer | Replace(`¿`) |
| T13 | Down `!`, timer | Replace(`¡`) |
| T14 | Down with null mappedChar | Returns false; no timer; state Idle (R8, R9) |
| T15 | Down `e`, change FocusToken, timer | No Replace; timer stopped (R10) |
| T16 | T15, then repeat down `e` | Returns true (Suppressed swallows repeats) |
| T17 | T15, then up `e`, then down `e`, timer | Fresh hold works: Replace(`é`) |
| T18 | Down `u`, timer, change FocusToken, timer | One Replace (`ú`), then nothing (R10 during cycling) |
| T19 | Down `e`, Reset, timer | No Replace; state Idle |
| T20 | Up for a key never pressed | No change, no exception |
| T21 | Hold delay setting 300 | Timer started with 300 |
| T22 | OnTimer while Idle | Nothing happens |

AccentMap tests:

| # | Scenario | Expect |
|---|---|---|
| A1 | Load `es` | `a e i o u n ? !` mapped with the variants in `PRODUCT.md` §4 |
| A2 | Uppercase derived | `A`→`Á`, `U`→`Ú`,`Ü`, `N`→`Ñ` |
| A3 | Punctuation has no uppercase entry added | Only `?` and `!` |
| A4 | Unmapped character (`x`, `1`, space) | No variants |
| A5 | Language file with a two-unit variant | Load fails with a clear error |

Settings tests: defaults match `PRODUCT.md` §5; an out-of-range hold delay is
clamped to the nearest allowed value; unreadable JSON yields defaults.

## 3. Smoke tests (human, Notepad, US keyboard)

Run after M2 and again after any change to the hook, translator, or sender.

| # | Do | See |
|---|---|---|
| S1 | Type `hola` at normal speed | `hola`, no lag, no accents |
| S2 | Hold `e` about a second, release | `é` (one character) |
| S3 | Hold `n` | `ñ` |
| S4 | Hold `u` and keep holding | `ú`, then `ü`, then `u`, cycling; release keeps what shows |
| S5 | Hold Shift+`e`; then with Caps Lock on, hold `e` | `É` both times |
| S6 | Hold Shift+`/` and Shift+`1` | `¿` and `¡` |
| S7 | Hold `x`; hold Backspace over some text | `xxxx...` repeats; Backspace repeats |
| S8 | `Ctrl+A`, `Ctrl+E`, `Alt+E` (menu) | Normal shortcut behavior, no accents |

Also confirm once: type a full sentence quickly with many `e`/`a`/`o`
(`el elefante come manzanas`). No letter may be dropped, doubled, or reordered.

## 4. Application matrix (human, before release)

For each app run S1, S2, S4, S5, S6. Record Pass, Minor (cosmetic or rare), or
Breaks typing, with a note.

| Application | Result | Notes |
|---|---|---|
| Notepad | | |
| Microsoft Word (desktop) | | |
| Word / Google Docs in Edge | | |
| Google Docs in Chrome | | |
| Chrome address bar | | expect autocomplete trouble (ARCHITECTURE §8.1) |
| Edge address bar | | same |
| Outlook or Gmail compose | | |
| Microsoft Teams chat | | |
| Excel cell | | autocomplete may interfere |
| Windows search / Start menu | | |
| File Explorer rename | | |
| Windows Terminal | | |
| VS Code | | |
| A password field | | must behave; nothing is logged regardless |

Extra checks:

- With the US-International layout active, `'` then `e` still gives `é` (dead keys intact), and holding `e` also gives `é`.
- With a Spanish keyboard layout active, Tilde does no harm.
- Lock and unlock the PC, sleep and wake: holding `e` still works afterward.
- An app running as administrator: Tilde does nothing there, and typing is normal.
- Task Manager: idle CPU 0%, memory recorded here: ______.
