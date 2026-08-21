using Dalamud.Plugin.Ipc;

namespace Lifestream.IPC;

/// <summary>
/// 国服跨大区适配层：调用 DCTravelerX 插件的 IPC 实现游戏内跨大区。
/// DCTravelerX 通过 XIVLauncherCN 的本地 HTTP API 完成超域旅行订单，
/// IPC 调用时自动确认、完成后只切换 SDO 大区不登录，由 Lifestream 接管登录。
/// </summary>
public static unsafe class DCTravelerIpc
{
    private static ICallGateSubscriber<int, int, ulong, bool, string, Task<Exception?>>? TravelSubscriber;
    private static ICallGateSubscriber<bool>? IsValidSubscriber;
    private static ICallGateSubscriber<uint, int>? GetWaitTimeSubscriber;
    private static ICallGateSubscriber<Task>? QueryAllWaitTimeSubscriber;
    private static ICallGateSubscriber<string, Task<bool>>? GetOrderStatusSubscriber;
    private static ICallGateSubscriber<string, Task<bool>>? SelectDCAndLoginSubscriber;

    /// <summary>是否已订阅 DCTravelerX 的 IPC 通道（插件安装后调用 Init）。</summary>
    public static bool Initialized { get; private set; }

    public static void Init()
    {
        if (Initialized) return;
        var pi = Svc.PluginInterface;
        TravelSubscriber = pi.GetIpcSubscriber<int, int, ulong, bool, string, Task<Exception?>>("DCTravelerX.Travel");
        IsValidSubscriber = pi.GetIpcSubscriber<bool>("DCTravelerX.IsValid");
        GetWaitTimeSubscriber = pi.GetIpcSubscriber<uint, int>("DCTravelerX.GetWaitTime");
        QueryAllWaitTimeSubscriber = pi.GetIpcSubscriber<Task>("DCTravelerX.QueryAllWaitTime");
        GetOrderStatusSubscriber = pi.GetIpcSubscriber<string, Task<bool>>("DCTravelerX.GetOrderStatus");
        SelectDCAndLoginSubscriber = pi.GetIpcSubscriber<string, Task<bool>>("DCTravelerX.SelectDCAndLogin");
        Initialized = true;
    }

    /// <summary>
    /// 国服 + DCTravelerX 已连接超域旅行 API 时可用。
    /// </summary>
    public static bool IsAvailable()
    {
        if ((int)Svc.Data.Language < 4) return false; // 非中文客户端（国际服），不需要走此路径
        if (!Initialized) Init();
        try
        {
            return Initialized && (IsValidSubscriber?.InvokeFunc() == true);
        }
        catch (Exception e)
        {
            e.Log();
            return false;
        }
    }

    /// <summary>
    /// 获取目标世界排队分钟数（-1 = 繁忙/不可用）。
    /// </summary>
    public static int GetWaitTime(uint worldId)
    {
        try
        {
            return GetWaitTimeSubscriber?.InvokeFunc(worldId) ?? -1;
        }
        catch (Exception e)
        {
            e.Log();
            return -1;
        }
    }

    /// <summary>手动刷新全部等待时间。</summary>
    public static void QueryAllWaitTime()
    {
        try
        {
            QueryAllWaitTimeSubscriber?.InvokeFunc();
        }
        catch (Exception e)
        {
            e.Log();
        }
    }

    /// <summary>
    /// 核心跨大区调用。返回 Task 完成后，null = 成功；非 null = 失败（携带异常）。
    /// DCTravelerX 内部完成：下单 → 排队 → 自动确认 → 切换 SDO 大区 → 停在标题界面（不登录）。
    /// </summary>
    public static Task<Exception?>? Travel(int currentWorldId, int targetWorldId, ulong contentId, bool isBack, string currentCharacterName)
    {
        if (!Initialized) return null;
        try
        {
            return TravelSubscriber?.InvokeFunc(currentWorldId, targetWorldId, contentId, isBack, currentCharacterName);
        }
        catch (Exception e)
        {
            e.Log();
            return Task.FromResult<Exception?>(e);
        }
    }
}
