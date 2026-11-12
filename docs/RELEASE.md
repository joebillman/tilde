# Tilde release: packaging, Store, and schools

Store rules and Windows packaging tools change. Facts here were checked in
November 2026 and are linked to their sources at the end. Before acting on
any of them, re-check the linked Microsoft page.

## 1. The route to a student's laptop

```
source -> self-contained publish -> MSIX package -> Microsoft Store
       -> school IT adds the Store app in Intune -> appears in Company Portal
       -> student installs (or IT pushes it to every device)
```

The target school blocks the Microsoft Store app for students and distributes
software through Intune Company Portal. That does not block Store apps as a
source: IT can publish a Store app to Company Portal. Being in the Store is
what makes Tilde easy for IT to add.

## 2. Who does what

| Step | Who |
|---|---|
| Developer account, name reservation, submission clicks | Human (Joe) |
| Manifest, assets, build script, listing text, privacy policy | Agent drafts, human reviews |
| Talking to school IT and teachers | Human (Joe's son, with Joe) |

The developer account requires identity verification with a government ID, so
it should be held by an adult. Individual and company accounts are both free.
The publisher name on the account is shown publicly on the listing.

## 3. Name

- Working name: **Tilde**. A search of the US Microsoft Store on 12 November 2026 found no app with that name.
- That search cannot see reserved-but-unpublished names. Reserving the name in Partner Center is the only real check. Do this early.
- Fallback if taken: **Tilde Keys**.
- A Latvian language-technology company is also named Tilde (tilde.ai). It is not in the consumer Store, but avoid wording or branding that could be confused with it.

## 4. Building the package

1. **Publish** self-contained for each architecture:
   ```
   dotnet publish src/Tilde.App -c Release -r win-x64   --self-contained -o publish/x64
   dotnet publish src/Tilde.App -c Release -r win-arm64 --self-contained -o publish/arm64
   ```
   Self-contained means the laptop needs no .NET runtime installed. The cost is size (tens of megabytes). If IT objects to the size, raise it with the human; do not switch runtimes on your own.

2. **Manifest.** `packaging/AppxManifest.xml` needs at least:
   - `Identity` with the `Name`, `Publisher`, and version. Name and Publisher come from Partner Center after the name is reserved; they must match exactly.
   - `Application` with `EntryPoint="Windows.FullTrustApplication"` and the app executable.
   - The restricted capability `runFullTrust`. This is normal for any packaged desktop app.
   - A `windows.startupTask` extension so Start with Windows works (ARCHITECTURE §9).
   - Visual assets: 44×44 and 150×150 app logos and the Store logo, with scale variants.

3. **Pack** into `.msix` per architecture and combine into an `.msixbundle`. `MakeAppx.exe` from the Windows SDK does both and scripts cleanly. A Visual Studio packaging project is the alternative. Check Microsoft's current packaging guidance, pick one, and record the choice in the decision log.

4. **Test the package locally** before submitting: with Developer Mode on, `Add-AppxPackage -Register packaging\<layout>\AppxManifest.xml` installs from loose files without signing. Rerun the smoke tests against the installed app, and specifically check that Start with Windows works, since it uses a different code path when packaged.

5. **Windows App Certification Kit**: run it on the final package and fix every failure.

Signing: for Store submission of an MSIX, Microsoft signs the package after
certification. No certificate purchase is needed.

## 5. Store submission checklist

- [ ] Name reserved; identity values copied into the manifest.
- [ ] Package bundle (x64 + ARM64) uploaded.
- [ ] Description: what it does, the key table, and the four privacy promises from `PRODUCT.md` §8.
- [ ] At least one screenshot (tray menu and the How to use window; a short clip of holding `e` if the listing allows video).
- [ ] Category: Utilities & tools, or Education.
- [ ] Age-rating questionnaire completed.
- [ ] Privacy policy URL. Host `PRIVACY.md` with GitHub Pages or link to the file on GitHub.
- [ ] Notes for certification, stating plainly: the app uses a low-level keyboard hook to detect a held key and replace it with an accented character; it needs `runFullTrust` because it is a desktop app; it stores no keystrokes and has no network access; here is how to test it (hold `e` in Notepad).
- [ ] Pricing: free.

Certification of a new app typically takes a few business days. A keyboard hook
may draw reviewer questions; a clear certification note and public source are
the best preparation. At least one comparable hook-based accent tool (Accentra)
is already in the Store.

## 6. School deployment

### Questions for school IT, before building the pitch

1. Which Windows edition are the student laptops on? If it is Windows 11 SE, only apps IT has explicitly allowed will run, and Microsoft ended support for SE in October 2026, so ask whether they are migrating.
2. How does an app get added to Company Portal, and who approves it?
3. Do you require anything from a vendor (privacy agreement, security questionnaire)?
4. Would you make it available for students to install, or install it for a class or grade?

### What IT does

In the Intune admin center: Apps → add → **Microsoft Store app (new)** → search
for Tilde → assign to a student group as Available (shows in Company Portal) or
Required (installs automatically).

This works when the Store is blocked with the "Turn off the Store application"
policy. If the school instead uses the older "Require private store only"
setting, Store app installs through Intune can fail; that is theirs to adjust,
but it helps to know.

### The one-page pitch for IT

- What it does, in one sentence, with the key table.
- Why students need it (Spanish classes, US keyboards, no good built-in way).
- Privacy: no network, no keystroke storage, no admin rights, public source (link).
- Footprint: package size and idle memory, from `TESTING.md` §4.
- How to add it (the Intune steps above) and how to remove it.
- A teacher's name as sponsor.

## 7. Updating after release

MSIX apps from the Store update through the Store's own mechanism, including on
Intune-managed devices. Tilde itself must never check for or download updates
(Hard rule 1).

Each release: bump the version in the manifest, rebuild both architectures, run
smoke tests on the installed package, submit an update.

## Sources

- Free developer registration (individuals): https://blogs.windows.com/windowsdeveloper/2025/09/10/free-developer-registration-for-individual-developers-on-microsoft-store/
- Free registration (companies): https://blogs.windows.com/windowsdeveloper/2026/05/07/publish-to-microsoft-store-as-a-company-now-with-free-registration-and-faster-onboarding/
- Publish your first app: https://learn.microsoft.com/en-gb/windows/apps/package-and-deploy/publish-first-app
- Code signing options: https://learn.microsoft.com/windows/apps/package-and-deploy/code-signing-options
- Distributing a Win32 app through the Store: https://learn.microsoft.com/en-us/windows/apps/distribute-through-store/how-to-distribute-your-win32-app-through-microsoft-store
- Capability declarations (`runFullTrust`): https://learn.microsoft.com/en-us/WINDOWS/APPS/package-and-deploy/app-capability-declarations
- Intune: new Microsoft Store apps and Autopilot: https://techcommunity.microsoft.com/t5/intune-customer-success/announcing-support-of-the-new-microsoft-store-apps-during/ba-p/3770228
- Store blocking policies and Intune: https://www.anoopcnair.com/turn-off-store-application-setting-intune
- Windows 11 SE overview and end of support: https://learn.microsoft.com/education/windows/windows-11-se-overview
- Accentra (comparable app in the Store): https://ipavicevic.github.io/Accentra
