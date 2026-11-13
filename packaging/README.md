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
