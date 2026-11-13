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

Exclusion list tests (R13):

| # | Scenario | Expect |
|---|---|---|
| X1 | Names are normalized | `Minecraft.EXE`, ` minecraft `, `C:\Games\minecraft.exe` all become `minecraft.exe` |
| X2 | Parsing the text box | Blank lines dropped, duplicates removed, order kept |
| X3 | Bad names | Names with `< > : " \| ? *` are dropped |
| X4 | Lookup | Matches the program name in any case; no match for `null` or an unlisted name |
| X5 | Limit | More than 100 entries keeps the first 100 |
| X6 | Settings file | Missing or `null` list loads as empty; the list survives a save and load |

## 3. Smoke tests (human, Notepad, US keyboard)

Run after M2 and again after any change to the hook, translator, sender, or any P/Invoke declaration, including the soak test below.

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

**Soak test.** Leave Tilde running for at least 10 minutes of ordinary typing,
in Notepad and one other app, using plain letters, held accents, Shift, and
Caps Lock. Tilde must still be in the tray at the end. A memory bug in native
interop (`ARCHITECTURE.md` §8.10) can crash the app at a random later moment,
so S1–S8 passing in a quick run is not enough. If Tilde disappears, check
Event Viewer → Windows Logs → Application for a `Tilde.exe` error and note the
exception code.

## 4. Application matrix (human, before release)

For each app run S1, S2, S4, S5, S6. Record Pass, Minor (cosmetic or rare), or
Breaks typing, with a note.

| Application | Result | Notes |
|---|---|---|
| Notepad | Pass | Joe, 2026-11-12 |
| Microsoft Word (desktop) | Pass | Joe, 2026-11-12 |
| Word / Google Docs in Edge | Pass | Joe, 2026-11-12 |
| Google Docs in Chrome | Pass | Joe, 2026-11-12 |
| Chrome address bar | Pass | expect autocomplete trouble (ARCHITECTURE §8.1); no trouble seen; Joe, 2026-11-12 |
| Edge address bar | Pass | same; no trouble seen; Joe, 2026-11-12 |
| Outlook or Gmail compose | Pass | Joe, 2026-11-12 |
| Microsoft Teams chat | Pass | Joe, 2026-11-12 |
| Excel cell | Pass | autocomplete may interfere; no trouble seen; Joe, 2026-11-12 |
| Windows search / Start menu | Pass | Joe, 2026-11-12 |
| File Explorer rename | Pass | Joe, 2026-11-12 |
| Windows Terminal | Pass | Joe, 2026-11-12 |
| VS Code | Pass | Joe, 2026-11-12 |
| A password field | Pass | must behave; nothing is logged regardless; Joe, 2026-11-12 |

Extra checks:

- With the US-International layout active, `'` then `e` still gives `é` (dead keys intact), and holding `e` also gives `é`. *Not run for v1 (Joe's call, 2026-11-12).*
- With a Spanish keyboard layout active, Tilde does no harm. *Not run for v1 (Joe's call, 2026-11-12).*
- Excluded apps (R13): with `notepad.exe` listed, holding `e` in Notepad repeats normally and other apps still get `é`; removing it takes effect without a restart; switching to an excluded app mid-hold types nothing more. **Pass** (Joe, 2026-11-12).
- Lock and unlock the PC, sleep and wake: holding `e` still works afterward. **Pass** (Joe, 2026-11-12, Notepad).
- An app running as administrator: Tilde does nothing there, and typing is normal. *Not run for v1 (Joe's call, 2026-11-12).*
- Task Manager: idle CPU 0%, memory recorded here: ______. Joe reports both "really low" (2026-11-12); exact Release-build numbers on a school-type laptop not yet recorded. For reference, a Debug build on Joe's PC idled at 0.05% of one core over 60 s, 19.6 MB private memory, 68 MB working set.
