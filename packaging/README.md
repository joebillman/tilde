# Packaging

Files for building the MSIX package for the Microsoft Store (PLAN.md M5,
RELEASE.md §4).

## Package identity

Assigned by Partner Center when the name was reserved (2026-11-12). The
manifest must use these exactly. They are public: they appear in every
installed package's manifest.

| Manifest field | Value |
|---|---|
| `Package/Identity/Name` | `JoeBillman.TildeKeys` |
| `Package/Identity/Publisher` | `CN=20C95762-C82C-4A5F-9AA1-6A448B472FFB` |
| `Package/Properties/PublisherDisplayName` | `Joe Billman` |
| Reserved Store name | **Tilde Keys** ("Tilde" was taken) |
| Store ID | `9P00H3RVW12L` |
| Store page | https://apps.microsoft.com/detail/9P00H3RVW12L |

## Artwork

The logo is defined once, in `src/Tilde.App/TildeGlyph.cs`. The tray icon draws
it at runtime. `AssetGenerator` uses the same file to write:

- `packaging/Assets/`: package images (Start menu, taskbar, Settings), with
  scale and target-size variants
- `packaging/listing/AppTile-300.png`: the 1:1 app tile for the Store listing
- `packaging/tilde-keys.svg`: a vector copy of the logo
- `src/Tilde.App/Tilde.ico`: the .exe icon

The outputs are committed. After changing `TildeGlyph.cs`, regenerate them from
the repository root:

```
dotnet run --project packaging/AssetGenerator
```

## Building the package

Needs the .NET 10 SDK and the Windows SDK (for `MakeAppx.exe` and
`MakePri.exe`). From the repository root:

```
pwsh packaging/build-package.ps1
```

This writes to `packaging/out/` (not committed):

- `msix/TildeKeys_<version>_x64.msix` and `..._arm64.msix`
- `TildeKeys_<version>.msixbundle`: the file to upload to Partner Center

The version comes from `<Version>` in `src/Tilde.App/Tilde.App.csproj`, with
`.0` added. Raise it before every Store update.

## Testing the package on this PC

Turn on Developer Mode (Settings > System > For developers), then:

```
pwsh packaging/build-package.ps1 -Register
```

This installs the x64 package folder without signing. Exit any copy of Tilde
started with `dotnet run` first. To remove the test install:

```
Get-AppxPackage JoeBillman.TildeKeys | Remove-AppxPackage
```
