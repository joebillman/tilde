# Handoff prompt

Paste everything below the line into a new agent session opened in this
repository.

---

You're picking up Tilde, a small Windows tray app that types Spanish accents
when you press and hold a key. The repository has a full plan and specs but no
app code yet. Your job is to build the first working version.

## The problems this solves

- Typing Spanish on a US keyboard in Windows is slow. The built-in ways are alt codes, switching keyboard layouts, or copy and paste. Students skip the accents instead.
- Phones and Macs let you hold a letter to get its accent. Windows has nothing built in, and holding a key in most apps just repeats the letter.
- The closest tools (PowerToys Quick Accent, Accentra) either make you pick from a list of every language's accents, need admin rights to install, or don't put `¿` and `¡` on the `?` and `!` keys.
- The intended users are students on school-managed Windows 11 laptops. Anything they use has to be approved by school IT, so it must be small, need no admin rights, make no network connections, and never record keystrokes.

## The objective

A tray app, run from source on Windows, where:

- Holding `a e i o u n` types `á é í ó ú ñ`; continuing to hold `u` cycles `ú` → `ü` → `u`; holding `?` and `!` types `¿` and `¡`; Shift or Caps Lock gives capitals.
- A quick tap types the normal letter with no delay, shortcuts like Ctrl+A are unaffected, and every other key behaves exactly as before.
- The tray menu lets the user turn Tilde on and off, set the hold delay, choose Start with Windows, and open How to use and About. Settings survive a restart.

Done means milestones **M0 through M3** in `docs/PLAN.md` are complete:

1. Every acceptance criterion for M0–M3 is met and its boxes are ticked.
2. `dotnet build` and `dotnet test` pass, with every engine test listed in `docs/TESTING.md` §2 written and passing.
3. I have personally run smoke tests S1–S8 from `docs/TESTING.md` §3 at a real keyboard and told you they pass. You cannot verify these yourself, because Tilde ignores simulated key presses by design. When you reach that point, stop and give me the exact steps to run.
4. The hard rules in `AGENTS.md` hold: no network code, no keystroke storage, no telemetry, no elevation.

Stop after M3 and summarize what was built, what you could not verify, and
anything you'd change in the plan. Hardening, packaging, and the Store (M4–M5)
come later.

## Where things are

Start with `AGENTS.md`; it lists the docs in reading order and the rules for
working here. `docs/PRODUCT.md` defines the behavior as numbered rules,
`docs/ARCHITECTURE.md` specifies the design down to the state machine, and
`docs/PLAN.md` is the task list. The docs are the source of truth. If you
think one is wrong, say so and ask before departing from it.

## Before you begin

Read the docs, check what's installed on this machine (`dotnet --list-sdks`),
and then ask me any clarifying questions you have before writing code. I'd
rather answer questions now than undo work later. Things worth raising: gaps or
contradictions in the specs, design choices you'd make differently, anything
missing from the environment, and anything about the school or Store context
that would change how you build.

If you have no questions, tell me your plan for M0 and M1 in a few lines and
start.
