# Microsoft Store listing draft: Tilde Keys

Draft text to paste into Partner Center (RELEASE.md §5). Check every field
against the current Partner Center form before submitting; field names and
length limits change.

## Product name

Tilde Keys

## Short description

Press and hold a key to type Spanish accents: hold e for é, n for ñ, ? for ¿.

## Description

Typing Spanish on a US keyboard shouldn't mean memorizing Alt codes or
copying and pasting. Tilde Keys works the way your phone does: press and hold
a letter, and it becomes its accented form.

Hold a key → get:
• a e i o → á é í ó
• u → ú (keep holding for ü)
• n → ñ
• ? → ¿
• ! → ¡

Hold Shift or turn on Caps Lock for capitals (É, Ñ, Ü). A quick tap types the
normal letter with no delay, and shortcuts like Ctrl+C work as usual.

Tilde Keys sits quietly in the system tray. From its menu you can turn it on
or off, choose how long to hold, start it with Windows, and list apps (such as
games) where it should stay off.

Made for students and schools:
• Makes no network connections
• Never records, stores, or sends anything you type
• Needs no administrator rights
• Free and open source: github.com/joebillman/tilde

## What's new in this version

First release.

## Product features (one per line)

Press and hold a letter to type its Spanish accent
¿ and ¡ on the ? and ! keys
Hold u to cycle ú, ü, u
Capitals with Shift or Caps Lock
Quick taps type normally, with no delay
Turn off for chosen apps, such as games
No network access and no keystroke storage
No administrator rights needed

## Search terms

Spanish accents, accent marks, tilde, ñ, Spanish keyboard, press and hold

## Category

Utilities & tools (alternative: Education)

## Privacy policy URL

https://github.com/joebillman/tilde/blob/main/PRIVACY.md

## Website / support

https://github.com/joebillman/tilde

## App tile icon (1:1, 300×300)

`packaging/listing/AppTile-300.png`

## Screenshots (to capture by hand)

At least one is required. Suggested, at 1920×1080 or similar:

1. Notepad showing `¿Cómo estás? Mañana es el cumpleaños de mi papá.` with the
   tray menu open beside it.
2. The How to use window.
3. The Excluded apps window, with a game listed.

## Pricing and availability

Free. All markets.

## Age rating questionnaire

No user interaction or sharing, no purchases, no location, no personal
information collected, no ads. Expect the lowest rating (3+ / Everyone).

## Notes for certification

Tilde Keys is a packaged desktop (Win32) app that types Spanish accent
characters. It uses a low-level keyboard hook (WH_KEYBOARD_LL) to detect when
a letter key is held down, then replaces the typed letter with its accented
form using SendInput (one Backspace, then the Unicode character). It requires
runFullTrust because it is a desktop app.

It stores no keystrokes and has no network access (no internetClient
capability). It ignores software-generated (injected) key events by design, so
automated input tools will not trigger it; please test with a physical
keyboard.

How to test: launch Tilde Keys (a ~ icon appears in the system tray; the How
to use window opens on first run). Open Notepad, then press and hold the `e`
key for about a second: the `e` becomes `é`. Hold `u` to cycle ú → ü → u.
Hold Shift+/ for ¿. A quick tap types a normal letter. Right-click the tray
icon for settings.

Source code: https://github.com/joebillman/tilde
