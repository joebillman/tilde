# Tilde

Press and hold a key to type Spanish accents on Windows.

| Hold | Get |
|---|---|
| `a` `e` `i` `o` | `á` `é` `í` `ó` |
| `u` | `ú`, keep holding for `ü` |
| `n` | `ñ` |
| `?` | `¿` |
| `!` | `¡` |

Hold Shift (or use Caps Lock) for capitals. A quick tap types the plain letter
as usual.

Tilde sits in the system tray. It makes no network connections and never
records what you type.

## Status

Working from source on Windows (milestones M0–M3). Not yet packaged or in the
Microsoft Store.

Build and run (needs the .NET 10 SDK):

```
dotnet test
dotnet run --project src/Tilde.App
```

The plan and specs are in `docs/`:

- [Product spec](docs/PRODUCT.md)
- [Architecture](docs/ARCHITECTURE.md)
- [Development plan](docs/PLAN.md)
- [Testing](docs/TESTING.md)
- [Release and school deployment](docs/RELEASE.md)
- [Background and decisions](docs/BACKGROUND.md)

AI agents: start with [AGENTS.md](AGENTS.md). The prompt for kicking off a
session is in [docs/HANDOFF_PROMPT.md](docs/HANDOFF_PROMPT.md).

## License

MIT. See [LICENSE](LICENSE).
