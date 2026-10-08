using System.Reflection;
using System.Text.Json.Serialization;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Utils.Cloners;
using Path = System.IO.Path;

namespace GunsmithTrader;

public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "com.landon.gunsmithtrader";
    public string Name { get; init; } = "Gunsmith Trader";
    public string Author { get; init; } = "Landon";
    public List<string>? Contributors { get; init; }
    public SemanticVersioning.Version Version { get; init; } = new("1.0.0");
    public SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.0");
    public List<string>? Incompatibilities { get; init; }
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public string? Url { get; init; }
    public string License { get; init; } = "MIT";
    public bool HasPrepatcher { get; init; } = false;
}

public class GunsmithConfig
{
    [JsonPropertyName("priceMultiplier")] public double PriceMultiplier { get; set; } = 1.0;
    [JsonPropertyName("fixedPriceRoubles")] public int FixedPriceRoubles { get; set; }
    [JsonPropertyName("loyaltyLevel")] public int LoyaltyLevel { get; set; } = 1;
    [JsonPropertyName("stockPerRestock")] public int StockPerRestock { get; set; } = 10;
    [JsonPropertyName("buyLimitPerRestock")] public int BuyLimitPerRestock { get; set; }
    [JsonPropertyName("restockMinutesMin")] public int RestockMinutesMin { get; set; } = 60;
    [JsonPropertyName("restockMinutesMax")] public int RestockMinutesMax { get; set; } = 120;
    [JsonPropertyName("includeBonusBuilds")] public bool IncludeBonusBuilds { get; set; } = true;
}

public class GunsmithPreset
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("quest")] public string? Quest { get; set; }
    [JsonPropertyName("items")] public List<Item> Items { get; set; } = [];
}

[Injectable(TypePriority = OnLoadOrder.TraderRegistration + 1)]
public class GunsmithTraderMod(
    ISptLogger<GunsmithTraderMod> logger,
    ModHelper modHelper,
    ImageRouter imageRouter,
    TraderConfig traderConfig,
    RagfairConfig ragfairConfig,
    TradersTable tradersTable,
    LocaleTable localeTable,
    HandbookHelper handbookHelper,
    ICloner cloner) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var config = modHelper.GetJsonDataFromFile<GunsmithConfig>(modPath, "config.json") ?? new GunsmithConfig();
        var traderBase = modHelper.GetJsonDataFromFile<TraderBase>(modPath, "db/base.json");
        var presets = modHelper.GetJsonDataFromFile<List<GunsmithPreset>>(modPath, "db/presets.json") ?? [];

        // Avatar + restock timer + flea visibility
        imageRouter.AddRoute(traderBase.Avatar!.Replace(".jpg", ""), Path.Combine(modPath, "db", "gunsmith.jpg"));
        traderConfig.UpdateTime.Add(new UpdateTime
        {
            TraderId = traderBase.Id,
            Seconds = new MinMax<int>(Math.Max(1, config.RestockMinutesMin) * 60, Math.Max(config.RestockMinutesMin, config.RestockMinutesMax) * 60)
        });
        ragfairConfig.Traders.TryAdd(traderBase.Id, true);

        // Build the assort from the presets
        var assort = new TraderAssort
        {
            Items = [],
            BarterScheme = new Dictionary<MongoId, List<List<BarterScheme>>>(),
            LoyalLevelItems = new Dictionary<MongoId, int>()
        };

        var added = 0;
        foreach (var preset in presets)
        {
            if (preset.Quest is null && !config.IncludeBonusBuilds) continue;
            if (preset.Items.Count == 0) continue;

            var items = cloner.Clone(preset.Items)!;
            var root = items[0];
            root.ParentId = "hideout";
            root.SlotId = "hideout";
            root.Upd ??= new Upd();
            root.Upd.UnlimitedCount = false;
            root.Upd.StackObjectsCount = Math.Max(1, config.StockPerRestock);
            if (config.BuyLimitPerRestock > 0)
            {
                root.Upd.BuyRestrictionMax = config.BuyLimitPerRestock;
                root.Upd.BuyRestrictionCurrent = 0;
            }

            var price = config.FixedPriceRoubles > 0
                ? config.FixedPriceRoubles
                : (int)Math.Max(1, Math.Round(handbookHelper.GetTemplatePriceForItems(items) * config.PriceMultiplier));

            assort.Items.AddRange(items);
            assort.BarterScheme[root.Id] = [[new BarterScheme { Template = Money.ROUBLES, Count = price }]];
            assort.LoyalLevelItems[root.Id] = Math.Max(1, config.LoyaltyLevel);
            added++;
        }

        var trader = new Trader
        {
            Assort = assort,
            Base = cloner.Clone(traderBase)!,
            QuestAssort = new()
            {
                { "Started", new() },
                { "Success", new() },
                { "Fail", new() }
            },
            Dialogue = []
        };

        if (!tradersTable.TryAdd(traderBase.Id, trader))
        {
            logger.Error($"[Gunsmith Trader] A trader with id {traderBase.Id} already exists, not adding.");
            return Task.CompletedTask;
        }

        var id = traderBase.Id;
        foreach (var (_, locale) in localeTable.Global)
        {
            locale.AddTransformer(data =>
            {
                data[$"{id} FullName"] = "Gunsmith";
                data[$"{id} FirstName"] = "Gunsmith";
                data[$"{id} Nickname"] = "Gunsmith";
                data[$"{id} Location"] = "Mechanic's workshop";
                data[$"{id} Description"] = "Sells a finished build for every Gunsmith task, assembled to spec and ready to hand in to Mechanic.";
                return data;
            });
        }

        logger.Success($"[Gunsmith Trader] Loaded with {added} Gunsmith builds.");
        return Task.CompletedTask;
    }
}
