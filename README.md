# Gunsmith Trader

A server mod for **SPT 4.1.x** that adds a **Gunsmith** trader. He sells a finished build for every Gunsmith task, made to pass the task's checks so you can buy it and hand it in to Mechanic.

- Gunsmith Parts 1–25 (Part 21 needs two guns: both the M700 and the M1911A1 are included)
- Gunsmith – Special Order
- Gunsmith – Old Friend's Request (all three guns: T-5000M, PP-19-01 Vityaz and Glock 17)

That's 30 builds in total, one for every gun Mechanic asks for. Each build is checked against the 4.1.x task data (correct base weapon, every required part, parts in slots that accept them, no conflicting parts, and the task's ergonomics, weight and magazine-size limits). They also pass the updated Gunsmith tasks from QuestBackport.

## Installation

1. Download the release zip.
2. Extract it into your SPT root folder (the folder that contains `SPT_Runtime`).
3. You should now have `SPT_Runtime\user\mods\GunsmithTrader\GunsmithTrader.dll`.
4. Start the server. The console shows: `[Gunsmith Trader] Loaded with 30 Gunsmith builds.`

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

To add or change a build, edit `db/presets.json`. Each entry is a normal SPT item tree, with the weapon as the first item.

## Building from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) and an SPT 4.1.x install.

```
dotnet build -c Release -p:SptRuntime="D:\SPT\SPT_Runtime" -o out
```

Copy everything in `out` to `SPT_Runtime\user\mods\GunsmithTrader\`.

## Changelog

- **1.0.2** — Added the T-5000M and PP-19-01 builds for Old Friend's Request (it needs three guns, not just the Glock). The M700 is now listed as Part 21 (M700) instead of a bonus build, since Part 21 needs it alongside the M1911A1. Removed the `includeBonusBuilds` setting.
- **1.0.1** — Fixed the AS VAL (Part 15) showing red (missing handguard) and a grip/stock conflict on the Special Order M4A1.
- **1.0.0** — Initial release.

## Credits

- Inspired by the original [Gunsmith](https://sp-mod.com/mod/761/gunsmith) trader mod by **alex** and **TEOA**, last updated for SPT 3.10. Most of the Parts 1–25 builds are based on that mod's presets (public community task builds), updated and fixed for 4.1.x. This is a full rewrite for the SPT 4.x C# server, with no code from the original.
- Built with the SPT server mod examples from the SP-Tushonka team.

## License

MIT. See `LICENSE`.
