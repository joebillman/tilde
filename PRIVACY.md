# Tilde Keys privacy policy

Last updated: 12 November 2026

Tilde Keys is a small Windows app: press and hold a letter to type its Spanish
accent. It is free and open source. This page explains exactly what it does
with your information. In short: **nothing leaves your computer, and nothing
you type is kept.**

## Our promises

- **Tilde Keys makes no network connections.** It never connects to the
  internet, sends data anywhere, or checks for updates. (Store updates are
  handled by the Microsoft Store, not by the app.)
- **Tilde Keys does not record, store, or send anything you type.**
- **Tilde Keys does not need administrator rights.**
- **The source code is public**, so anyone can check these promises:
  https://github.com/joebillman/tilde

## What the app looks at, and why

To know when you are holding a key, Tilde Keys uses a standard Windows
feature (a keyboard hook) that lets it see each key press as it happens.

- It looks only at the key being pressed right now. While you hold a letter
  such as `e`, it remembers that one key until you let go. Then it forgets it.
- It never keeps a history of keys, words, or text, never writes keys to a
  file or log, and never reads the clipboard.
- To check that it's still typing into the same window, it notes which window
  is in front while you hold a key.
- If you use **Excluded apps**, it checks the program file name of the window
  in front (for example `minecraft.exe`) against your list when you switch
  windows. It keeps only a yes/no answer, not the name.

## What the app stores

Only your settings, in a file on your own computer
(`%LOCALAPPDATA%\Tilde\settings.json`, or the app's private folder when
installed from the Store):

- whether Tilde Keys is on, the language, and the hold delay
- whether it starts with Windows
- your list of excluded program names, if you made one
- whether the How to use window has been shown

Nothing in this file comes from what you type. Uninstalling the app removes
it (for the Store version) or you can delete it yourself.

## Children and schools

Tilde Keys is made for students. It collects no personal information from
anyone, of any age: no accounts, no names, no email addresses, no device IDs,
no usage statistics, and no advertising.

## Third parties

There are none. Tilde Keys contains no analytics, advertising, or tracking
code, and shares nothing with anyone.

## Changes and contact

If this policy ever changes, the new version will be published here with a new
date, and the app's behavior will match it. Questions: open an issue at
https://github.com/joebillman/tilde/issues
