using ECommons.ExcelServices;
using Lifestream.IPC;
using Lifestream.Schedulers;

namespace Lifestream.Tasks.CrossDC;

/// <summary>
/// 国服跨大区任务编排：调用 DCTravelerX 的 Travel IPC 完成超域旅行，
/// 完成后 DCTravelerX 停在标题界面（已切换 SDO 大区），由本类接管进入角色选择。
/// </summary>
internal static unsafe class TaskTravelViaDCTraveler
{
    /// <summary>
    /// 入队国服跨大区任务链。
    /// </summary>
    /// <param name="destinationWorld">目标世界名；isBack=true 时忽略（目标为本服）</param>
    /// <param name="charaName">角色名</param>
    /// <param name="charaHomeWorld">角色本服 world id</param>
    /// <param name="currentLoginWorld">当前登录所在 world id</param>
    /// <param name="isBack">true = 返回原始大区（DCTravelerX 按 contentId 查原订单 TravelBack）</param>
    internal static void Enqueue(string destinationWorld, string charaName, uint charaHomeWorld, uint currentLoginWorld, bool isBack)
    {
        var contentId = GetContentId(charaName, charaHomeWorld);
        var targetWorldId = isBack
            ? (int)charaHomeWorld
            : (int)(ExcelWorldHelper.Get(destinationWorld)?.RowId ?? charaHomeWorld);
        var currentWorldId = (int)currentLoginWorld;

        Task<Exception?>? travelTask = null;
        void tasks()
        {
            P.TaskManager.Enqueue(() =>
            {
                travelTask = DCTravelerIpc.Travel(currentWorldId, targetWorldId, contentId, isBack, charaName);
                if (travelTask == null)
                {
                    PluginLog.Error("DCTravelerX IPC 未初始化");
                    return null;
                }
                PluginLog.Information($"DCTraveler 跨大区开始: {currentWorldId} -> {targetWorldId}, isBack={isBack}, {charaName}");
                return true;
            }, $"InitiateDCTravel_{charaName}", TaskSettings.Timeout1M);
            P.TaskManager.Enqueue(() => travelTask!.IsCompleted, $"WaitDCTravel_{charaName}", TaskSettings.Timeout60M);
            P.TaskManager.Enqueue(() => CheckResult(travelTask!), $"CheckDCTravel_{charaName}", TaskSettings.Timeout1M);
        }
        tasks();
        // DCTraveler 完成后停在标题界面（needLogin=false 不点开始），此处进入角色选择
        P.TaskManager.Enqueue(DCChange.TitleScreenClickStart, TaskSettings.Timeout1M);
    }

    /// <summary>
    /// 检查 Travel 任务结果：null = 成功，非 null = 失败。
    /// </summary>
    private static bool? CheckResult(Task<Exception?> travelTask)
    {
        if (travelTask.IsFaulted)
        {
            var e = travelTask.Exception?.GetBaseException();
            e?.Log();
            Notify.Error($"跨大区失败: {e?.Message ?? "未知错误"}");
            return null;
        }
        var result = travelTask.Result;
        if (result != null)
        {
            result.Log();
            Notify.Error($"跨大区失败: {result.Message}");
            return null;
        }
        PluginLog.Information("DCTraveler 跨大区完成");
        return true;
    }

    /// <summary>
    /// 获取角色 ContentId：优先当前登录角色，否则从 CharaMap 反查（角色选择界面场景）。
    /// </summary>
    private static ulong GetContentId(string charaName, uint charaHomeWorld)
    {
        if (Svc.ClientState.LocalContentId != 0) return Svc.ClientState.LocalContentId;
        var key = $"{charaName}@{ExcelWorldHelper.GetName(charaHomeWorld)}";
        return C.CharaMap.FirstOrDefault(x => x.Value == key).Key;
    }
}
