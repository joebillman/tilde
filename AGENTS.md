# Tilde: instructions for AI agents

Tilde is a small Windows tray app. While it runs, pressing and holding a letter
types its accented form (hold `e` → `é`, hold `n` → `ñ`, hold `?` → `¿`).
Spanish is the first and, for now, only language.

Read this file first, then the docs in the order listed. They are the source of
truth; if code and docs disagree, stop and ask.

## Read in this order

1. `docs/PRODUCT.md`: what the app does, exactly. The behavior rules are numbered; tests refer to them.
2. `docs/ARCHITECTURE.md`: stack, project layout, the hold state machine, Win32 details, known pitfalls.
3. `docs/PLAN.md`: milestones with tasks and acceptance criteria. Work top to bottom.
4. `docs/TESTING.md`: unit tests to write and the manual test matrix.
5. `docs/RELEASE.md`: packaging, Microsoft Store, and school deployment.
6. `docs/BACKGROUND.md`: why the product exists, competitors, and the decision log.

## Hard rules

These are product promises made to schools. Do not break them, even if asked to
"just add" something that conflicts. Raise it with the human instead.

1. **No network.** The app never opens a socket, makes an HTTP call, or checks for updates itself. No package that does so on its behalf.
2. **No keystroke storage.** Never log, buffer, or persist what the user types. The engine holds only the one key currently held. Debug logging must never include characters or key codes in Release builds.
3. **No telemetry or analytics**, and no clipboard access.
4. **Never run elevated.** No admin rights, no installer actions that need them.
5. **The hook callback stays tiny.** No I/O, no blocking, no `SendInput` inside it. See ARCHITECTURE.md §6.
6. **No new dependencies without asking.** The app should build with the .NET SDK alone; the test project may use xUnit.

## How to work

- Work one milestone at a time, in order. Do not start a milestone until the previous one's acceptance criteria pass.
- Tick the checkboxes in `docs/PLAN.md` as tasks finish, in the same commit as the work.
- When you make a design choice the docs don't cover, add a dated entry to the decision log in `docs/BACKGROUND.md`.
- When behavior changes, update `docs/PRODUCT.md` first, then tests, then code.
- Small commits with plain messages (`Add HoldEngine cycling`, not `feat(core): ...`). Do not push or open pull requests unless the human asks.
- Before saying a task is done: `dotnet build` has no warnings you introduced, and `dotnet test` passes.

## Commands

```
dotnet build                                   # whole solution (App needs Windows)
dotnet test                                    # Core tests; run anywhere
dotnet run --project src/Tilde.App             # run the tray app (Windows only)
dotnet publish src/Tilde.App -c Release -r win-x64 --self-contained
```

## What you can and cannot verify

- `Tilde.Core` has no Windows dependencies. Its tests run on any OS and must always pass.
- `Tilde.App` builds and runs only on Windows. If you are not on Windows, write the code, say plainly that it is unbuilt and untested, and list what the human must check.
- The keyboard hook cannot be exercised by injected input, because Tilde ignores injected keys by design. Real hold-a-key behavior needs a human at a keyboard following `docs/TESTING.md` §3. Never report hook behavior as verified unless a human confirmed it.

## Ask the human before

- Changing anything in `docs/PRODUCT.md` §3 (behavior rules) or the Hard rules above.
- Adding a dependency, a language, or a settings option not in the plan.
- Anything involving accounts: Partner Center, Store submission, GitHub settings, signing certificates. Humans do those steps; you prepare the files.

## People

- Joe (repo owner, `joebillman`): experienced software developer, makes product calls.
- Joe's son: the student this project is for. He wants to build on it and get his school to adopt it. Write code he can read: clear names, short files, comments that explain *why* where the Win32 behavior is surprising.
