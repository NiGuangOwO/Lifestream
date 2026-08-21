using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lifestream.Data;
using Lifestream.Tasks.SameWorld;
using Lifestream.Tasks.Shortcuts;
using Lumina.Excel.Sheets;
using GrandCompany = ECommons.ExcelServices.GrandCompany;

namespace Lifestream.IPC;

public static class WotsitEntryGenerator
{
    private static readonly Dictionary<uint, uint> AetheryteToTownPlaceName = new()
    {
        { 2, 39 },     // Gridania
        { 8, 27 },     // Limsa Lominsa
        { 9, 51 },     // Ul'dah
        { 62, 1484 },  // The Gold Saucer
        { 70, 62 },    // Ishgard
        { 75, 2082 },  // Idyllshire
        { 104, 2403 }, // Rhalgr's Reach
        { 111, 513 },  // Kugane
        { 127, 2507 }, // The Doman Enclave
        { 133, 516 },  // The Crystarium
        { 134, 517 },  // Eulmore
        { 182, 3706 }, // Old Sharlayan
        { 183, 3707 }, // Radz-at-Han
        { 216, 4504 }, // Tuliyollal
        { 217, 4503 }, // Solution Nine
    };

    // Any invisible aethernet shards that should be added to wotsit. In the
    // future this should be replaced with a config option.
    private static readonly List<uint> InvisibleWhitelist = [
        91,  // Prologue Gate (Western Hinterlands)
        92,  // Epilogue Gate (Eastern Hinterlands)
        120, // The Ruby Price
        239, // Phantom Village
    ];

    // These values cannot be read sometimes during a territory change, so we
    // cache them permanently otherwise they will disappear.
    private static readonly HashSet<ulong> HasPrivateEstate = [];
    private static readonly HashSet<ulong> HasFreeCompanyEstate = [];
    private static readonly HashSet<ulong> HasApartment = [];

    public static IEnumerable<WotsitEntry> Generate()
    {
        var includes = C.WotsitIntegrationIncludes;

        foreach (var entry in Generic(includes))
        {
            yield return entry;
        }

        // TODO: eventually residential aethernet could be added too
        if (includes.AetheryteAethernet)
        {
            foreach (var entry in AetheryteAethernetTeleport())
            {
                yield return entry;
            }
        }

        if (includes.AddressBook)
        {
            foreach (var entry in AddressBook())
            {
                yield return entry;
            }
        }

        if (includes.CustomAlias)
        {
            foreach (var entry in CustomAlias())
            {
                yield return entry;
            }
        }
    }

    private static IEnumerable<WotsitEntry> Generic(WotsitIntegrationIncludedItems includes)
    {
        if (Player.CID != 0 && TaskPropertyShortcut.GetPrivateHouseAetheryteID() != 0)
        {
            HasPrivateEstate.Add(Player.CID);
        }
        if (Player.CID != 0 && TaskPropertyShortcut.GetFreeCompanyAetheryteID() != 0)
        {
            HasFreeCompanyEstate.Add(Player.CID);
        }
        if (Player.CID != 0 && TaskPropertyShortcut.GetApartmentAetheryteID().ID != 0)
        {
            HasApartment.Add(Player.CID);
        }

        if (includes.WorldSelect)
        {
            yield return new WotsitEntry
            {
                DisplayName = "打开服务器选择窗口",
                SearchString = "open world select window - open travel window - 打开服务器选择窗口 - 打开跨服窗口",
                IconId = 55,
                Callback = () => S.Gui.SelectWorldWindow.IsOpen = true,
            };
        }
        if (includes.PropertyAuto)
        {
            yield return new WotsitEntry
            {
                DisplayName = "自动传送到房屋",
                SearchString = "auto-teleport to property - 自动传送到房屋",
                IconId = 52,
                Callback = () => TaskPropertyShortcut.Enqueue(TaskPropertyShortcut.PropertyType.自动),
            };
        }
        if (includes.PropertyPrivate && HasPrivateEstate.Contains(Player.CID))
        {
            yield return new WotsitEntry
            {
                DisplayName = "传送到你的个人房屋",
                SearchString = "teleport to your private estate - teleport to home - 传送到个人房屋 - 回家",
                IconId = 52,
                Callback = () => TaskPropertyShortcut.Enqueue(TaskPropertyShortcut.PropertyType.个人房屋),
            };
        }
        if (includes.PropertyFreeCompany && HasFreeCompanyEstate.Contains(Player.CID))
        {
            yield return new WotsitEntry
            {
                DisplayName = "传送到你的部队房屋",
                SearchString = "teleport to your free company estate - teleport to fc - 传送到部队房屋",
                IconId = 52,
                Callback = () => TaskPropertyShortcut.Enqueue(TaskPropertyShortcut.PropertyType.部队房屋),
            };
        }
        if (includes.PropertyApartment && HasApartment.Contains(Player.CID))
        {
            yield return new WotsitEntry
            {
                DisplayName = "传送到你的公寓",
                SearchString = "teleport to your apartment - 传送到公寓",
                IconId = 52,
                Callback = () => TaskPropertyShortcut.Enqueue(TaskPropertyShortcut.PropertyType.公寓),
            };
        }
        if (includes.PropertyInn)
        {
            // TODO: each inn could be a separate entry in the future
            yield return new WotsitEntry
            {
                DisplayName = "传送到旅店房间",
                SearchString = "teleport to an inn room - 传送到旅店房间 - 旅馆",
                IconId = 88,
                Callback = () => TaskPropertyShortcut.Enqueue(TaskPropertyShortcut.PropertyType.旅馆),
            };
        }
        // TODO: each grand company could be a separate entry in the future
        if (includes.GrandCompany && Player.GrandCompany != GrandCompany.Unemployed)
        {
            var (name, icon) = Player.GrandCompany switch
            {
                GrandCompany.Maelstrom => ("黑涡团", 60313u),
                GrandCompany.TwinAdder => ("双蛇党", 60306u),
                GrandCompany.ImmortalFlames => ("恒辉队", 60305u),
                _ => throw new NotImplementedException(),
            };
            yield return new WotsitEntry
            {
                DisplayName = "传送到你的大国防联军 - " + name,
                SearchString = name + " - teleport to your grand company headquarters - teleport to gc hq - 传送到大国防联军 - 部队箱",
                IconId = icon,
                Callback = () => TaskGCShortcut.Enqueue(Player.GrandCompany),
            };
        }
        if (includes.MarketBoard)
        {
            yield return new WotsitEntry
            {
                DisplayName = "传送到市场布告板",
                SearchString = "teleport to a market board - teleport to mb - 传送到市场布告板 - 板子",
                IconId = 63,
                Callback = () => TaskMBShortcut.Enqueue(),
            };
        }
        // TODO: the NPCs on the island sanctuary could get their own entries
        if (includes.IslandSanctuary)
        {
            yield return new WotsitEntry
            {
                DisplayName = "传送到你的无人岛",
                SearchString = "teleport to your island sanctuary - teleport to is - 传送到无人岛",
                IconId = 23,
                Callback = () => TaskISShortcut.Enqueue(),
            };
        }
    }

    private static IEnumerable<WotsitEntry> AetheryteAethernetTeleport()
    {
        // Avoid spoilers by not showing aetherytes that the player hasn't
        // attuned to.
        uint[] visibleAetheryteIds = [];
        if (Player.Available)
        {
            unsafe
            {
                var visibleAetherytes = Telepo.Instance()->UpdateAetheryteList();
                if (visibleAetherytes != null)
                {
                    visibleAetheryteIds = visibleAetherytes->Select(x => x.AetheryteId).ToArray();
                }
            }
        }

        foreach (var (rootAetheryte, aethernetShards) in S.Data.DataStore.Aetherytes)
        {
            if (visibleAetheryteIds.Length > 0 && !visibleAetheryteIds.Contains(rootAetheryte.ID))
            {
                PluginLog.Debug($"WotsitEntryGenerator.AetheryteAethernetTeleport: Skipping aetheryte {rootAetheryte.ID} ({rootAetheryte.Name}) because it is not visible");
                continue;
            }

            string townName = null;
            if (AetheryteToTownPlaceName.TryGetValue(rootAetheryte.ID, out var placeId))
            {
                townName = Svc.Data.GetExcelSheet<PlaceName>().GetRow(placeId).Name.ToDalamudString().GetText();
            }
            foreach (var aethernetShard in aethernetShards)
            {
                if (!C.Hidden.Contains(aethernetShard.ID) && (!aethernetShard.Invisible || InvisibleWhitelist.Contains(aethernetShard.ID)))
                {
                    var name = C.Renames.TryGetValue(aethernetShard.ID, out var value) ? value : aethernetShard.Name;
                    yield return WotsitEntry.AetheryteAethernetTeleport(townName, name, rootAetheryte.ID, aethernetShard.ID);
                }
            }

            // Special case for The Firmament
            if (C.Firmament && rootAetheryte.TerritoryType == 418)
            {
                var placeName = Svc.Data.GetExcelSheet<PlaceName>().GetRow(3435).Name.ToDalamudString().GetText();
                yield return WotsitEntry.AetheryteAethernetTeleport(townName, placeName, rootAetheryte.ID, TaskAetheryteAethernetTeleport.FirmamentAethernetId);
            }
        }
    }

    private static IEnumerable<WotsitEntry> AddressBook()
    {
        foreach (var entry in C.AddressBookFolders.SelectMany(folder => folder.Entries))
        {
            var searchStr = entry.Name + (!string.IsNullOrEmpty(entry.Alias) && entry.Alias != entry.Name ? " - " + entry.Alias : "");
            yield return new WotsitEntry
            {
                DisplayName = $"传送到地址 {entry.Name}",
                SearchString = searchStr,
                IconId = 52,
                Callback = entry.GoTo,
            };
        }
    }

    private static IEnumerable<WotsitEntry> CustomAlias()
    {
        foreach (var alias in C.CustomAliases.Where(a => a.Enabled && !string.IsNullOrEmpty(a.Alias)))
        {
            yield return new WotsitEntry
            {
                DisplayName = $"Run alias {alias.Alias}",
                SearchString = alias.Alias,
                IconId = 55,
                Callback = () => alias.Enqueue(),
            };
        }
    }
}
