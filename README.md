# Gunsmith Trader

A server mod for **SPT 4.1.x** that adds a **Gunsmith** trader. He sells a finished build for every Gunsmith task, made to pass the task's checks so you can buy it and hand it in to Mechanic.

- Gunsmith Parts 1–25
- Gunsmith – Special Order
- Gunsmith – Old Friend's Request
- A bonus M700 build (can be turned off)

## Installation

1. Download the release zip.
2. Extract it into your SPT root folder (the folder that contains `SPT_Runtime`).
3. You should now have `SPT_Runtime\user\mods\GunsmithTrader\GunsmithTrader.dll`.
4. Start the server. The console shows: `[Gunsmith Trader] Loaded with 28 Gunsmith builds.`

No dependencies. The trader is unlocked from the start. Works on new and existing profiles.

**Uninstall:** delete the `GunsmithTrader` folder. Weapons you already bought stay in your stash.

## Configuration

Edit `config.json` in the mod folder, then restart the server.

| Setting | Default | What it does |
|---|---|---|
| `priceMultiplier` | `1.0` | Each build costs the handbook value of all its parts times this number. `0.5` = half price. |
| `fixedPriceRoubles` | `0` | Set above 0 to charge one flat price for every build (ignores the multiplier). |
| `loyaltyLevel` | `1` | Trader level needed to buy. The trader only has level 1. |
| `stockPerRestock` | `10` | How many of each build are in stock. |
| `buyLimitPerRestock` | `0` | Max you can buy of each build per restock. `0` = no limit. |
| `restockMinutesMin` / `restockMinutesMax` | `60` / `120` | Restock timer range in minutes. |
| `includeBonusBuilds` | `true` | `false` hides the bonus M700. |

To add or change a build, edit `db/presets.json`. Each entry is a normal SPT item tree, with the weapon as the first item.

## Building from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) and an SPT 4.1.x install.

```
dotnet build -c Release -p:SptRuntime="D:\SPT\SPT_Runtime" -o out
```

Copy everything in `out` to `SPT_Runtime\user\mods\GunsmithTrader\`.

## Credits

- Inspired by the original [Gunsmith](https://sp-mod.com/mod/761/gunsmith) trader mod by **alex** and **TEOA**, last updated for SPT 3.10. The Parts 1–25 builds are based on that mod's presets (public community task builds), updated and fixed for 4.1.x. This is a full rewrite for the SPT 4.x C# server, with no code from the original.
- Built with the SPT server mod examples from the SP-Tushonka team.

## License

MIT. See `LICENSE`.
