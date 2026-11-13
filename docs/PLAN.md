# Tilde development plan

Work the milestones in order. Each has tasks and acceptance criteria; a
milestone is finished when every criterion passes. Tick boxes as you go.

"Human" marks steps an AI agent prepares for but a person performs.

## Status

| Milestone | State |
|---|---|
| M0 Project setup | done (CI green on first push, 2026-11-12) |
| M1 Core engine | done |
| M2 Windows integration | done (S1–S8 confirmed by Joe, 2026-11-12) |
| M3 Tray app and settings | done (checks and S1–S8 confirmed by Joe, 2026-11-12) |
| M4 Hardening | done (3 extra checks skipped by Joe; see TESTING §4) |
| M5 Packaging and Store | in progress |
| M6 School rollout | not started |

**v1 is M0 through M5.** M6 is the reason the project exists but is mostly
human work.

---

## M0. Project setup

- [x] Confirm the newest .NET LTS SDK is installed (`dotnet --list-sdks`); if not, tell the human what to install.
- [x] Create `Tilde.sln` with `src/Tilde.Core` (class library), `src/Tilde.App` (WinForms, `OutputType` `WinExe`), `tests/Tilde.Core.Tests` (xUnit), laid out as in `ARCHITECTURE.md` §2.
- [x] Enable nullable reference types and treat warnings as errors in all three projects (a shared `Directory.Build.props`).
- [x] `Tilde.App` references `Tilde.Core`; the test project references `Tilde.Core` only.
- [x] Add a GitHub Actions workflow on `windows-latest`: restore, build, test, on push and pull request.

**Done when:** `dotnet build` and `dotnet test` succeed on a clean clone (zero tests is fine), and the workflow file is committed. It can only go green after the human pushes; ask them to push and check it.

## M1. Core engine

All work is in `Tilde.Core` and its tests. No Windows code.

- [x] `Settings` model with the defaults and allowed values from `PRODUCT.md` §5.
- [x] `Languages/es.json` as an embedded resource, per `ARCHITECTURE.md` §10.
- [x] `AccentMap`: load a language, derive uppercase entries, reject multi-unit variants, look up variants for a character.
- [x] `IEngineHost` and `HoldEngine` exactly as specified in `ARCHITECTURE.md` §4.
- [x] Unit tests for every case listed in `TESTING.md` §2, using a fake host.

**Done when:** every test in `TESTING.md` §2 exists and passes, and `Tilde.Core` has no reference to Windows Forms or P/Invoke.

## M2. Windows integration

- [x] `NativeMethods`: declarations for the hook, `SendInput` (with the full `INPUT` union), `ToUnicodeEx`, key-state, foreground-window, and keyboard-layout calls.
- [x] `KeyTranslator` per `ARCHITECTURE.md` §5.
- [x] `InputSender` per §7.
- [x] `EngineHost` implementing `IEngineHost` with a WinForms `Timer` and `InputSender`.
- [x] `KeyboardHook` per §6, including the try/catch and the delegate field.
- [x] Temporary `Program.cs` that installs the hook with default settings and runs a message loop (a bare tray icon with Exit is enough).

**Done when:** a human confirms smoke tests S1–S8 in `TESTING.md` §3 pass in Notepad. An agent cannot confirm this alone.

## M3. Tray app and settings

- [x] `SettingsStore` per `ARCHITECTURE.md` §9, with tests for the load/clamp/default logic placed in Core where possible.
- [x] `TrayContext` with the full menu from `PRODUCT.md` §6; changes apply immediately and are saved.
- [x] Tray icon: a simple tilde glyph, readable on light and dark taskbars, dimmed when disabled. Drawing it at runtime is acceptable for v1.
- [x] `AutoStart`, unpackaged implementation (registry `Run` key).
- [x] Single-instance mutex.
- [x] `HowToWindow` (the table from `PRODUCT.md` §4) shown once on first run; `AboutWindow` with version and the privacy statement from `PRODUCT.md` §8.
- [x] Disabling calls `engine.Reset()`.

**Done when:** every menu item works, settings survive a restart, a second launch does not create a second icon, and smoke tests S1–S8 still pass.

## M4. Hardening

- [x] Run the full application matrix in `TESTING.md` §4 (human). Fix what can be fixed; list the rest under Known issues in `README.md`.
- [x] Reinstall the hook on session unlock and resume from sleep (`SystemEvents.SessionSwitch`, `PowerModeChanged`).
- [x] Excluded apps: a list of process names in settings for which Tilde stays idle, editable from the tray (a simple text window is enough). Ask the human before designing anything more elaborate.
- [x] Idle memory and CPU check: record working-set size and confirm no measurable CPU while not typing. The target machines are small school laptops.
- [x] Review the code against the Hard rules in `AGENTS.md`: search for any networking, file writes other than settings, logging, or clipboard use.

**Done when:** the matrix results are recorded in `docs/TESTING.md` §4, no item is marked "breaks typing", and the review finds nothing.

## M5. Packaging and Store

Details in `RELEASE.md`.

- [x] Human: create the Microsoft Store developer account and reserve the app name. Record the package identity values in `packaging/README.md`.
- [x] `AutoStart`, packaged implementation (`StartupTask`), selected automatically.
- [x] Confirm the installed (Store) build has Start with Windows on by default (`PRODUCT.md` §5); development builds deliberately do not.
- [x] MSIX manifest, visual assets, and a script that publishes and packs x64 and ARM64.
- [x] Replace the runtime-drawn tray icon with one drawn from a vector design, so it stays crisp at 16 px and matches the Store assets.
- [x] Install the package locally and rerun smoke tests S1–S8 from the installed app.
- [x] `PRIVACY.md` in the repo root, published at a public URL, stating `PRODUCT.md` §8.
- [ ] Store listing text and screenshots drafted in `packaging/listing/`.
- [x] Run the Windows App Certification Kit and fix failures.
- [ ] Human: submit for certification.

**Done when:** the app is live in the Microsoft Store and installs on a machine that has never had a .NET SDK.

## M6. School rollout

Mostly human work; an agent can draft the documents.

- [ ] One-page summary for school IT: what it does, the privacy promises, Store link, source link, how to add it in Intune.
- [ ] Confirm with IT which Windows edition the student laptops run and how apps get into Company Portal.
- [ ] A Spanish teacher willing to sponsor the request.
- [ ] Pilot with one class; collect problems; fix; then wider rollout.

**Done when:** students at the school can install Tilde from Company Portal.

---

## After v1

Not planned in detail. Do not start any of these without the human asking.

- More languages (French, German, Portuguese). Several variants per letter will test whether hold-to-cycle is enough or a picker is needed.
- A setting to restore auto-repeat on mapped keys.
- A Mac version. macOS already has a hold menu for letters, so the gap there is `¿` and `¡`.
- Whole-word auto-correct. Parked for the reasons in `BACKGROUND.md`.
