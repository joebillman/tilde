# Tilde background and decisions

## 1. Where the idea came from

Phones let you press and hold a letter to get its accented forms. Joe wanted
the same on a desktop: an app in the system tray where you pick a language and
holding a key types the accent. His son wants to build it and get his school to
adopt it for Spanish classes.

The school's student laptops are small Windows 11 machines. The Microsoft Store
is blocked for students; apps arrive through Intune Company Portal.

## 2. What already exists (researched November 2026)

| Tool | Platform | How it works | Gap |
|---|---|---|---|
| PowerToys Quick Accent | Windows | Hold a letter and press Space or an arrow to open a picker. Since v0.101 there is an opt-in "Press and hold the letter" mode that opens the picker by itself. A language can be selected to shorten the list. | Still a picker: you select with arrows or Space before releasing. `¿` and `¡` live under the comma key, not under `?` and `!`. Installing PowerToys needs admin rights. |
| Accentra | Windows | Tray app. Hold about half a second and the letter is swapped for an accent, no popup; further taps cycle variants. Free, open source, in the Store. | Not language-aware; narrowing to Spanish means hand-editing a JSON file. Default maps are letters only. Closest existing product. |
| Holdkey (holdkey.eu) | Windows | Hold a letter, a small window of variants appears, choose by number or mouse. | Old (v4.0), picker-based, paid Pro tier. |
| Capstan | Windows | Mac-style overlay on long-press; punctuation supported. | Hobby project, no adoption. |
| Long Press (AutoHotkey) | Windows | Long press sends one configured replacement per key. | A script the user configures, not an app. Schools will not deploy AutoHotkey. |
| macOS accent menu | Mac | Built in: hold a letter for a menu. | Symbol keys just repeat; no hold for `¿` or `¡`. |

**Tilde's niche:** choose Spanish once, then hold a key and get the accent
with no picker, including `¿` and `¡`, in a small no-admin app a school can
approve.

## 3. Ideas considered and parked

**Whole-word auto-correct** (`tambien` → `también`). It works for words whose
unaccented form is not a word, but Spanish has many pairs where both spellings
are real words with different meanings: `esta`/`está`, `si`/`sí`, `el`/`él`,
`tu`/`tú`, `mas`/`más`, `papa`/`papá`, `hablo`/`habló`. Only context separates
them, and a wrong correction is worse than none. It would also require the app
to track every word typed in every program, which conflicts with the privacy
promises. Windows 11 and macOS both have built-in autocorrect that may already
cover the easy cases. Not pursued.

**A real keyboard / input method** selectable from the Windows language
switcher. More robust than a hook, but far more work (a COM component loaded
into every app) and it needs an admin install. Rejected for v1.

## 4. Decision log

Add new entries at the bottom with a date.

**2026-11-12. Hook-based tray app, not an input method.** A low-level keyboard
hook needs no admin rights, is a few hundred lines, and can suppress repeat on
only the mapped keys. See §3 for the alternative.

**2026-11-12. Type first, then replace.** The plain letter is typed
immediately and replaced via Backspace if the hold completes. The alternative,
withholding the letter until key-up or timeout, avoids Backspace problems in
autocomplete fields but delays every `a e i o u n` a student types and risks
reordering fast typing. Zero typing lag was judged more important.

**2026-11-12. No picker; hold to cycle.** Spanish has one variant per key
except `u`. A picker would add a selection step to solve a one-key problem.
Revisit when a language with many variants per letter is added.

**2026-11-12. The cycle includes the original character.** `u` → `ú` → `ü` →
`u` → ... so a user who overshoots can get back without deleting.

**2026-11-12. Mapped keys lose auto-repeat.** Required for hold-to-accent to
work in ordinary text fields, which otherwise fill with repeated letters.

**2026-11-12. C# on .NET with Windows Forms.** Readable for a student, first-
class Windows API access, and a tray icon needs nothing more than `NotifyIcon`.
Electron and similar were ruled out as too heavy for small school laptops. A
self-contained build avoids any runtime prerequisite. .NET Framework 4.8 (built
into Windows, tiny output) is the fallback if package size becomes a blocker.

**2026-11-12. Core logic in a platform-neutral library.** So the state machine
is unit-tested and so agents not running on Windows can still verify their work.

**2026-11-12. Character detection by `ToUnicodeEx`, not fixed key codes.**
Keeps `?` and `!` correct on any keyboard layout. Fallback noted in
ARCHITECTURE §5.

**2026-11-12. Ignore all injected input.** Simplest way to ignore Tilde's own
output and avoids fighting other automation tools. Cost: the hook cannot be
tested by simulated key presses.

**2026-11-12. Name: Tilde.** In Spanish, "tilde" is the everyday word for the
accent mark itself. Risks and fallback in `RELEASE.md` §3.

**2026-11-12. Public repository, MIT license.** Public source is the strongest
answer to a school asking what a keyboard-watching app does.

**2026-11-12. Distribution through the Microsoft Store as MSIX.** Free
signing and updates, and the easiest thing for school IT to add to Intune.

## 5. Sources

- PowerToys Quick Accent docs: https://learn.microsoft.com/en-us/windows/powertoys/quick-accent
- PowerToys Quick Accent developer notes: https://microsoft.github.io/PowerToys/modules/quickaccent/
- PowerToys v0.101 release notes (press-and-hold mode): https://github.com/microsoft/PowerToys/releases/tag/v0.101.2362.0
- PowerToys issue, loss of key repeat in hold mode: https://github.com/microsoft/powertoys/issues/50613
- Accentra: https://ipavicevic.github.io/Accentra
- Holdkey review: https://dottech.org/?p=79854
- Capstan: https://github.com/fr2019/capstan
- macOS accent menu: https://support.apple.com/en-vn/guide/mac-help/mh27474/26/mac/26
- Windows 11 physical-keyboard autocorrect: https://mashtips.com/?p=105710

**2026-11-12. SDK pinned with `global.json`.** `10.0.100` with
`rollForward: latestFeature`, so any .NET 10 SDK builds the repo and CI's
`setup-dotnet` installs the same major version. `Tilde.sln` uses the classic
`.sln` format (`dotnet new sln --format sln`) because ARCHITECTURE §2 names
it; .NET 10 would otherwise create `.slnx`.

**2026-11-12. Test project drops `coverlet.collector`.** The xUnit template
adds it; AGENTS.md allows xUnit only. The test SDK and xUnit runner stay
because `dotnet test` needs them.

**2026-11-12. First-run flag stored as a hidden setting.** PRODUCT §6 says to
show How to use once on first run. `Settings.HowToShown` records that in
`settings.json`; it has no menu entry.

**2026-11-12. Cycle delay clamped to 200–3000 ms.** PRODUCT §5 doesn't give a
range for the cycle delay because it isn't in the UI. The clamp only guards
against a hand-edited settings file.

**2026-11-12. Language files also reject combining marks and surrogates.** A
lone combining accent (U+0301) or half of a surrogate pair is one UTF-16 unit
but isn't a usable precomposed character, so `AccentMap` rejects it with the
same error as a two-unit variant.

**2026-11-12. Development builds don't turn on Start with Windows by
themselves.** With the spec's default of on, the first `dotnet run` would add
`bin\Debug\Tilde.exe` to every login. Joe chose: unpackaged builds only write
the registry `Run` key when the user ticks the menu item, and the checkmark
reflects whether the key exists. The Store build keeps default-on. PRODUCT §5
updated.

**2026-11-12. Hard-rules review (M4).** Searched all app source for
networking, file and registry writes, logging, telemetry, clipboard, process
launches, and elevation. Only the settings file and the startup `Run` key are
written; nothing else was found. Two in-memory leftovers were tightened: the
key translator now clears its character buffer after each call, and the engine
forgets the held key code when a hold ends. Neither was ever written anywhere.

**2026-11-12. Excluded apps (R13), as Joe chose.** A new behavior rule, R13:
while a listed program is in front, Tilde acts as if disabled. The list starts
empty and holds program file names (`minecraft.exe`), edited one per line in a
plain text window. To keep the keyboard hook fast, Tilde does not look up the
program on each key press. A `SetWinEventHook(EVENT_SYSTEM_FOREGROUND)`
callback, out of context so nothing is injected into other apps, checks the
program name when the foreground window changes and keeps only a yes/no flag;
the name is compared with the list and not stored. A picker of running apps
was considered and left for later.

**2026-11-12. M4 closed without three extra checks.** Joe chose not to run
the US-International layout, Spanish layout, and run-as-administrator checks
in TESTING §4 for now. All 14 apps in the matrix passed, and idle use was
reported as very low. The dead-key check matters most if the school uses
US-International keyboards, so run it before the pilot (M6) if that is
likely.

**2026-11-12. Store name is "Tilde Keys".** "Tilde" was already taken when
Joe reserved the name in Partner Center, so the RELEASE §3 fallback was used.
Package identity values are in `packaging/README.md`.

**2026-11-12. "Tilde Keys" is the user-facing name; code stays "Tilde".**
Joe's call, so students and IT see one name that matches the Store. User-visible
strings come from `AppInfo.ProductName`. Internal names (namespace, .exe, the
settings folder, the mutex, the `Run` value) stay "Tilde" so existing settings
are kept.

**2026-11-12. App targets `net10.0-windows10.0.19041.0`.** Approved by Joe.
The Store build needs Windows' `StartupTask` API for Start with Windows, which
the plain `net10.0-windows` target can't reach. The versioned target makes the
SDK fetch Microsoft's Windows API reference pack at restore; there is still no
NuGet package in the project. Minimum Windows version: 10 2004 (19041).
`AutoStart.Create()` picks the StartupTask version when the process has
package identity, otherwise the registry version.

**2026-11-12. Packages built with MakeAppx from a script, not a Visual Studio
packaging project.** `packaging/build-package.ps1` publishes each
architecture, writes the manifest, indexes images with MakePri, and packs and
bundles with MakeAppx. It runs from a terminal with only the .NET SDK and the
Windows SDK, is short enough to read, and doesn't need Visual Studio, as
RELEASE.md §4 suggested. All image scales go in the main package; there are no
separate resource packs.

**2026-11-12. First certification kit run: passed with warnings.** The
optional "Blocked executables" test flagged references inside the bundled .NET
runtime (strings such as "cmd" and process-launch APIs). Tilde never starts a
process, so this is explained in the certification notes rather than changed.
The DPI-awareness warning was fixed with an embedded `app.manifest` declaring
PerMonitorV2, plus `asInvoker` to state Hard rule 4 explicitly. WinForms'
WFO0003 rule, which prefers DPI settings in the project only, is suppressed
because the kit reads only the manifest; both settings agree.

**2026-11-12. Installed package verified.** Joe installed the x64 package
locally. S1–S8, Start with Windows on by default (and off after unticking),
and the About version passed. Correction (same day): that installed copy
predated the DPI manifest, so the windows were not yet checked with the DPI
change, and a second certification kit run still showed the DPI warning. The
package must be rebuilt and the kit rerun.
After rebuilding, the third kit run (report 3) reports PASS overall. The DPI
warning is gone, and only the optional "Blocked executables" false positive
remains.
