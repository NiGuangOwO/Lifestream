using Dalamud.Utility;
using Lifestream.Enums;
using Lumina.Excel.Sheets;
using System.Text.RegularExpressions;

namespace Lifestream;

internal static class Lang
{
    public const string SymbolWard = "";
    public const string SymbolPlot = "";
    public const string SymbolApartment = "";
    public const string SymbolSubdivision = "";
    public static readonly (string Normal, string GameFont) Digits = ("0123456789", "");

    public static readonly string Help = $"""
    -- 主要旅行 --

    /li → 前往你的原始服务器
    /li <服务器> → 前往指定服务器
    /li <大区> → 前往指定大区的随机服务器
    /li <以太水晶> → 前往指定的以太水晶
    /li <服务器>, tp <位置> → 前往指定服务器的指定以太水晶
    /li <服务器>, tp <以太水晶> → 前往指定服务器的指定以太水晶

    -- 市场布告板 --

    /li mb → 前往市场布告板
    /li <服务器> mb → 前往指定服务器的市场布告板

    -- 房屋 --

    /li auto → 根据配置的偏好，前往你的个人房屋、共享房屋、部队房屋或公寓
    /li shared → 根据配置的偏好，前往你的共享房屋
    /li home → 前往你的个人房屋，别名：/li home|house|private
    /li fc → 前往你的部队房屋，别名：/li fc|free|company|free company
    /li apt → 前往你的公寓，别名：/li apt|apartment
    /li ws → 前往你部队房屋的部队工房，别名：/li ws|workshop

    /li <district> <ward> <plot> → 前往当前服务器的指定地块
    /li <world> <district> <ward> <plot> → 前往指定服务器的指定地块
    示例：/li lavender 1 30, /li goblet 1 30, /li mist 1 30

    -- 大国防联军 --

    /li gc → 前往你的大国防联军，别名：/li gc|hcc
    /li gc <grandcompany> → 前往指定的大国防联军，别名：/li gc|hcc <grandcompany>
    /li gcc → 前往你大国防联军所在城市的部队箱，别名：/li gc|hcc
    /li gcc <grandcompany> → 前往指定的大国防联军所在城市的部队箱，别名：/li gc|hcc <grandcompany>
    使用“hc”或“hcc”代替“gc”或“gcc”会优先将你带回你的原始服务器

    -- 其他 --

    /li cosmic → 前往 宇宙探索, 别名: /li cosmic|moon|ardorum
    /li island → 前往 无人岛
    /li firmament → - 前往 天穹街
    /li w → 打开跨服窗口，别名： /li w|world|open|select
    /lifestream → 打开插件配置 
    """;
    internal static string[] AdditionalChambersEntrance =>
    [
        Svc.Data.GetExcelSheet<EObjName>().GetRow(2004353).Singular.GetText(),
        Regex.Replace(Svc.Data.GetExcelSheet<EObjName>().GetRow(2004353).Singular.GetText(), @"\[.*?\]", "")
    ];

    internal static string[] EnterWorkshop => [GameText.Dialogue("custom/001/CmnDefHousingPersonalRoomEntrance_00178", 11)];

    internal static Dictionary<WorldChangeAetheryte, string> WorldChangeAetherytes = new()
    {
        [WorldChangeAetheryte.Gridania] = "格里达尼亚新街",
        [WorldChangeAetheryte.Uldah] = "乌尔达哈来生回廊",
        [WorldChangeAetheryte.Limsa] = "利姆萨·罗敏萨下层甲板 (不推荐)"
    };

    internal static class Symbols
    {
        internal const string HomeWorld = "";
        internal const string HighQuality = "";
    }

    internal static GameTextPattern LogInPartialText => new(
        Svc.Data.GetExcelSheet<Lobby>().GetRow(25).Text,
        Svc.Data.GetExcelSheet<Lobby>().GetRow(95).Text,
        Svc.Data.GetExcelSheet<Lobby>().GetRow(96).Text,
        Svc.Data.GetExcelSheet<Lobby>().GetRow(629).Text);

    internal static string[] Aethernet => [GameText.Dialogue("transport/Aetheryte", 1)];
    internal static string[] VisitAnotherWorld => [GameText.Dialogue("transport/Aetheryte", 3)];
    internal static GameTextPattern ConfirmWorldVisit => new(
        Svc.Data.GetExcelSheet<Addon>().GetRow(12519).Text,
        Svc.Data.GetExcelSheet<Addon>().GetRow(12624).Text);

    internal static string AethernetShard => Svc.Data.GetExcelSheet<EObjName>().GetRow(2000151).Singular.ToDalamudString().GetText();
    internal static string[] TravelToFirmament => [GameText.Dialogue("transport/AetheryteIshgard", 0)];
    public static string[] ResidentialDistrict => [GameText.Dialogue("transport/Aetheryte", 2)];
    public static string[] GoToWard => [Svc.Data.GetExcelSheet<Addon>().GetRow(6349).Text.GetText().Trim()];
    public static GameTextPattern TravelTo => new(Svc.Data.GetExcelSheet<Addon>().GetRow(6355).Text);

    public static string[] GoToSpecifiedApartment => [GameText.Dialogue("custom/003/HouFixMansionEntrance_00359", 2)];
    public static GameTextPattern EnterApartmenr => new(
        Svc.Data.GetExcelSheet<Addon>().GetRow(6782).Text,
        Svc.Data.GetExcelSheet<Addon>().GetRow(6784).Text);
    public static string[] GoToMyApartment => [GameText.Dialogue("custom/003/HouFixMansionEntrance_00359", 0)];

    public static string[] TravelToInstancedArea => [GameText.Dialogue("transport/Aetheryte", 16)];
    public static string ToReduceCongestion => Svc.Data.GetExcelSheet<Addon>().GetRow(2090).Text.GetText();
    public static string[] TravelToYourIsland => [GameText.Dialogue("custom/007/CtsMjiEntrance_00798", 4)];
    public static string[] TravelToMyIsland => [GameText.Dialogue("custom/007/CtsMjiEntrance_00798", 7)];
    public static string[] Entrance =>
    [
        Svc.Data.GetExcelSheet<EObjName>().GetRow(2002737).Singular.GetText(),
        Svc.Data.GetExcelSheet<EObjName>().GetRow(2002739).Singular.GetText()
    ];
    public static string[] ConfirmHouseEntrance => [Svc.Data.GetExcelSheet<Warp>().GetRow(131148).Question.GetText()];

    public static GameTextPattern UnableToSelectWorldForDcv => new(Svc.Data.GetExcelSheet<Lobby>().GetRow(1218).Text);

    public static readonly string[] RemainingSubTime = ["sqex.to/Msp"];

    public static readonly string AdjoiningArea = Svc.Data.GetExcelSheet<PlaceName>().GetRow(1252).Name.GetText();
    public static readonly string ToUpperLevel = Svc.Data.GetExcelSheet<PlaceName>().GetRow(1250).Name.GetText();
    public static readonly string ToLowerLevel = Svc.Data.GetExcelSheet<PlaceName>().GetRow(1251).Name.GetText();
    public static readonly string AethernetShardTooltip = Svc.Data.GetExcelSheet<PlaceName>().GetRow(1300).Name.GetText();
}
