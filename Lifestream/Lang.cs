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

    internal static readonly string[] EnterWorkshop = ["Move to the company workshop", "地下工房に移動する", "移动到部队工房", "移動到部隊工房", "Die Gesellschaftswerkstätte betreten", "Aller dans l'atelier de compagnie", "지하공방으로 이동"];

    internal static Dictionary<WorldChangeAetheryte, string> WorldChangeAetherytes = new()
    {
        [WorldChangeAetheryte.Gridania] = "格里达尼亚新街",
        [WorldChangeAetheryte.Uldah] = "乌尔达哈来生回廊",
        [WorldChangeAetheryte.Limsa] = "利姆萨·罗敏萨下层甲板 (不推荐)"
    };

    internal static class Symbols
    {
        internal const string HomeWorld = "";
    }

    internal static readonly string[] LogInPartialText = ["Logging in with", "Log in with", "でログインします。", "einloggen?", "eingeloggt.", "Se connecter avec", "Vous allez vous connecter avec", "Souhaitez-vous vous connecter avec", "登录吗？", "登入嗎？", "접속하시겠습니까?"];

    /*
    1	TEXT_AETHERYTE_TOWN_WARP<Gui(69)/> Aethernet.
    1	TEXT_AETHERYTE_TOWN_WARP<Gui(69)/> 都市転送網
    1	TEXT_AETHERYTE_TOWN_WARP<Gui(69)/> Ätheryten<SoftHyphen/> netz
    1	TEXT_AETHERYTE_TOWN_WARP<Gui(69)/> Réseau de transport urbain éthéré
    */
    internal static readonly string[] Aethernet = ["Aethernet.", "都市転送網", "Ätherytennetz", "Réseau de transport urbain éthéré", "都市传送网", "都市傳送網", "도시 내 이동"];

    /*
     * 3	TEXT_AETHERYTE_MENU_WKT	<Gui(69)/> Visit Another World Server.
        3	TEXT_AETHERYTE_MENU_WKT	<Gui(69)/> 他のワールドへ遊びにいく
        3	TEXT_AETHERYTE_MENU_WKT	<Gui(69)/> Weltenreise
        3	TEXT_AETHERYTE_MENU_WKT	<Gui(69)/> Voyager vers un autre Monde
     * */
    internal static readonly string[] VisitAnotherWorld = ["Visit Another World Server.", "他のワールドへ遊びにいく", "Weltenreise", "Voyager vers un autre Monde", "跨界传送", "跨界傳送", "다른 서버 방문"];

    internal static readonly string[] ConfirmWorldVisit = ["Travel to", "へ移動します、よろしいですか？", "reisen?", "Voulez-vous vraiment visiter", "确定要移动到", "確定要移動到", "방문하시겠습니까?"];

    //2000151	Aethernet shard	0	Aethernet shards	0	1	1	0	0
    internal static string AethernetShard => Svc.Data.GetExcelSheet<EObjName>().GetRow(2000151).Singular.ToDalamudString().GetText();

    //0	TEXT_AETHERYTEISHGARD_HWD_WARP	<Gui(69)/> Travel to the Firmament.
    //0	TEXT_AETHERYTEISHGARD_HWD_WARP	<Gui(69)/> 蒼天街転送
    //0	TEXT_AETHERYTEISHGARD_HWD_WARP<Gui(69)/> Himmelsstadt
    //0	TEXT_AETHERYTEISHGARD_HWD_WARP	<Gui(69)/> Azurée
    internal static readonly string[] TravelToFirmament = ["Travel to the Firmament.", "蒼天街転送", "Himmelsstadt", "Azurée", "传送到天穹街", "傳送到蒼天街", "창천 거리 이동"];

    //2	TEXT_AETHERYTE_HOUSING_WARP	<Gui(69)/> Residential District Aethernet.
    //2	TEXT_AETHERYTE_HOUSING_WARP	<Gui(69)/> 冒険者居住区転送
    //2	TEXT_AETHERYTE_HOUSING_WARP	<Gui(69)/> Wohngebiet
    //2	TEXT_AETHERYTE_HOUSING_WARP	<Gui(69)/> Quartier résidentiel
    public static readonly string[] ResidentialDistrict = ["Residential District Aethernet.", "冒険者居住区転送", "Wohngebiet", "Quartier résidentiel", "冒险者住宅区传送", "冒險者住宅區傳送", "모험가 거주구 이동"];
    public static readonly string[] GoToWard = ["Go to specified ward. (Review Tabs)", "区を指定して移動（ハウスアピール確認）", "Zum angegebenen Bezirk (Zweck der Unterkunft einsehen)", "Spécifier le secteur où aller (Voir les attraits)", "移动到指定小区（查看房屋宣传标签）", "移動到指定小區（查看房屋宣傳標籤）", "구역을 지정하여 이동(주택 정보 확인)"];

    //6355	<Sheet(PlaceName,IntegerParameter(1),0)/>第<Value>IntegerParameter(2)</Value>区に移動します。
    //よろしいですか？
    //6355	Zu Wohnbezirk <Value>IntegerParameter(2)</Value> <Sheet(PlaceName,IntegerParameter(1),8,2)/> gehen?
    //6355	Vous allez vous rendre à “<Sheet(PlaceName,IntegerParameter(1),0)/>” dans le secteur <Value>IntegerParameter(2)</Value>. Confirmer<Indent/>?
    public static readonly string[] TravelTo = ["Travel to", "よろしいですか？", "Zu Wohnbezirk", "Vous allez vous rendre à", "要移动到", "要移動到", "이동하시겠습니까?"];

    public static readonly string[] GoToSpecifiedApartment = ["Go to specified apartment", "Go to speciz`fied apartment", "部屋番号を指定して移動（ハウスアピール確認）", "Eine bestimmte Wohnung betreten (Zweck der Unterkunft einsehen)", "移动到指定号码房间（查看房屋宣传标签）", "移動到指定號碼房間（查看房屋宣傳標籤）", "방 번호를 지정하여 이동(주택 정보 확인)", "Spécifier l'appartement où aller (Voir les attraits)"];
    public static readonly string[] EnterApartmenr = ["Enter", "よろしいですか？", "betreten?", "Aller dans l'appartement", "要移动到", "要移動到", "이동하시겠습니까?"];
    public static readonly string[] GoToMyApartment = ["Go to your apartment", "移动到自己的房间", "移動到自己的房間", "自分の部屋に移動する", "자신의 방으로 이동", "Aller dans votre appartement"];

    //12	TEXT_AETHERYTE_MOVE_INSTANCE	<Gui(69)/> Travel to Instanced Area.
    //12	TEXT_AETHERYTE_MOVE_INSTANCE	<Gui(69)/> インスタンスエリアへ移動
    //12	TEXT_AETHERYTE_MOVE_INSTANCE	<Gui(69)/> In ein instanziiertes Areal wechseln
    //12	TEXT_AETHERYTE_MOVE_INSTANCE	<Gui(69)/> Changer d'instance
    public static readonly string[] TravelToInstancedArea = ["Travel to Instanced Area.", "インスタンスエリアへ移動", "In ein instanziiertes Areal wechseln", "Changer d'instance", "切换副本区", "切換副本區", "인스턴스 지역으로 이동"];
    public static string ToReduceCongestion => Svc.Data.GetExcelSheet<Addon>().GetRow(2090).Text.GetText();
    public static string[] TravelToYourIsland = ["Travel to your island?", "あなたの島へ向かいますか？", "Zu deiner Insel fahren?", "Voulez-vous aller sur votre île?", "나의 섬으로 가시겠습니까?", "要前往你的小岛吗？"]; // row 4
    public static string[] TravelToMyIsland = ["Travel to my island.", "「自分の島」に行く", "Zur eigenen Insel fahren", "Aller sur son île", "'나의 섬'으로 가기", "前往自己的小岛"]; // row 7
    public static readonly string[] Entrance =
    [
        "ハウスへ入る",
        "进入房屋",
        "進入房屋",
        "Eingang",
        "Entrée",
        "Entrance"
    ];
    public static readonly string[] ConfirmHouseEntrance =
    [
        "「ハウス」へ入りますか？",
        "要进入这间房屋吗？",
        "要進入這間房屋嗎？",
        "Das Gebäude betreten?",
        "Entrer dans la maison ?",
        "Enter the estate hall?"
    ];

    public static readonly string[] UnableToSelectWorldForDcv = [
        "Unable to select", "で選択したワールド", "Die für die", "pas être choisi comme destination pour"
        ];

    public static readonly string AethernetShardTooltipPlaceholder = Svc.Data.GetExcelSheet<PlaceName>().GetRow(1300).Name.GetText();
}
