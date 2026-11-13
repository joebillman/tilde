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
