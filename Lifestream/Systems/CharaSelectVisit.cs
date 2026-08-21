using ECommons.Automation.NeoTaskManager.Tasks;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using Lifestream.Enums;
using Lifestream.Schedulers;
using Lifestream.Tasks;
using Lifestream.Tasks.CrossDC;
using Lifestream.Tasks.CrossWorld;
using Lifestream.Tasks.SameWorld;

namespace Lifestream.Systems;
public static unsafe class CharaSelectVisit
{
    /// <summary>
    /// 返回主世界（同 DC World Visit，国服原生支持）。
    /// </summary>
    public static void HomeToHome(string destinationWorld, string charaName, uint homeWorld, uint currentLoginWorld, string secondaryTeleport = null, bool noSecondaryTeleport = false, WorldChangeAetheryte? gateway = null, bool? doNotify = null, bool? returnToGateway = null, bool noLogin = false)
    {
        ApplyDefaults(ref returnToGateway, ref gateway, ref doNotify);
        TaskReturnToHomeWorldCharaSelect.Enqueue(charaName, homeWorld, currentLoginWorld);
        if(!noLogin)
        {
            TaskSelectChara.Enqueue(charaName, homeWorld, currentLoginWorld);
            if(ExcelWorldHelper.Get(homeWorld)?.Name != destinationWorld)
            {
                P.TaskManager.EnqueueMulti([
                    new(DCChange.WaitUntilNotBusy, TaskSettings.TimeoutInfinite),
                        new DelayTask(1000),
                        new(() => TaskTPAndChangeWorld.Enqueue(destinationWorld, gateway.Value.AdjustGateway(), true), $"TpAndChangeWorld {destinationWorld} at {gateway.Value}"),
                        ]);
            }
            else
            {
                TaskWaitUntilInWorld.Enqueue(destinationWorld, false);
            }
            if(gateway != null && returnToGateway == true) TaskReturnToGateway.Enqueue(gateway.Value);
            if(doNotify == true) TaskDesktopNotification.Enqueue($"Arrived to {destinationWorld}");
            EnqueueSecondary(noSecondaryTeleport, secondaryTeleport);
        }
    }

    /// <summary>
    /// 从访客大区返回本服（DCTravelerX isBack=true 返回原始大区）。
    /// </summary>
    public static void GuestToHome(string destinationWorld, string charaName, uint homeWorld, uint currentLoginWorld, string secondaryTeleport = null, bool noSecondaryTeleport = false, WorldChangeAetheryte? gateway = null, bool? doNotify = null, bool? returnToGateway = null, bool skipReturn = false, bool noLogin = false)
    {
        ApplyDefaults(ref returnToGateway, ref gateway, ref doNotify);
        if (!skipReturn)
        {
            TaskTravelViaDCTraveler.Enqueue(destinationWorld, charaName, homeWorld, currentLoginWorld, true);
        }
        if(!noLogin)
        {
            TaskSelectChara.Enqueue(charaName, homeWorld, currentLoginWorld);
            if(ExcelWorldHelper.Get(homeWorld)?.Name != destinationWorld)
            {
                P.TaskManager.EnqueueMulti([
                    new(DCChange.WaitUntilNotBusy, TaskSettings.TimeoutInfinite),
                        new DelayTask(1000),
                        new(() => TaskTPAndChangeWorld.Enqueue(destinationWorld, gateway.Value.AdjustGateway(), true), $"TpAndChangeWorld {destinationWorld} at {gateway.Value}"),
                        ]);
            }
            else
            {
                TaskWaitUntilInWorld.Enqueue(destinationWorld, true);
            }
            if(gateway != null && returnToGateway == true) TaskReturnToGateway.Enqueue(gateway.Value);
            if(doNotify == true) TaskDesktopNotification.Enqueue($"Arrived to {destinationWorld}");
            EnqueueSecondary(noSecondaryTeleport, secondaryTeleport);
        }
    }

    /// <summary>
    /// 从本服前往其他大区（DCTravelerX isBack=false 超域旅行）。
    /// </summary>
    public static void HomeToGuest(string destinationWorld, string charaName, uint homeWorld, uint currentLoginWorld, string secondaryTeleport = null, bool noSecondaryTeleport = false, WorldChangeAetheryte? gateway = null, bool? doNotify = null, bool? returnToGateway = null, bool noLogin = false)
    {
        ApplyDefaults(ref returnToGateway, ref gateway, ref doNotify);
        TaskTravelViaDCTraveler.Enqueue(destinationWorld, charaName, homeWorld, currentLoginWorld, false);
        if(!noLogin)
        {
            TaskSelectChara.Enqueue(charaName, homeWorld, currentLoginWorld);
            TaskWaitUntilInWorld.Enqueue(destinationWorld, true);
            TaskEnforceWorld.Enqueue(destinationWorld, gateway);
            if(gateway != null && returnToGateway == true) TaskReturnToGateway.Enqueue(gateway.Value);
            if(doNotify == true) TaskDesktopNotification.Enqueue($"Arrived to {destinationWorld}");
            EnqueueSecondary(noSecondaryTeleport, secondaryTeleport);
        }
    }

    /// <summary>
    /// 从访客大区前往第三个大区：先返回原始大区，再超域旅行至目标。
    /// </summary>
    public static void GuestToGuest(string destinationWorld, string charaName, uint homeWorld, uint currentLoginWorld, string secondaryTeleport = null, bool noSecondaryTeleport = false, WorldChangeAetheryte? gateway = null, bool? doNotify = null, bool? returnToGateway = null, bool noLogin = false, bool useSameWorldReturnHome = false)
    {
        ApplyDefaults(ref returnToGateway, ref gateway, ref doNotify);
        if (useSameWorldReturnHome)
        {
            // 玩家在本服 DC 的其他世界：先 World Visit 返回主世界，再超域旅行
            TaskReturnToHomeWorldCharaSelect.Enqueue(charaName, homeWorld, currentLoginWorld);
        }
        else
        {
            // 玩家在访客 DC：先 DCTraveler 返回原始大区，再超域旅行去目标
            TaskTravelViaDCTraveler.Enqueue(destinationWorld, charaName, homeWorld, currentLoginWorld, true);
        }
        TaskTravelViaDCTraveler.Enqueue(destinationWorld, charaName, homeWorld, homeWorld, false);
        if(!noLogin)
        {
            TaskSelectChara.Enqueue(charaName, homeWorld, currentLoginWorld);
            TaskWaitUntilInWorld.Enqueue(destinationWorld, true);
            TaskEnforceWorld.Enqueue(destinationWorld, gateway);
            if(gateway != null && returnToGateway == true) TaskReturnToGateway.Enqueue(gateway.Value);
            if(doNotify == true) TaskDesktopNotification.Enqueue($"Arrived to {destinationWorld}");
            EnqueueSecondary(noSecondaryTeleport, secondaryTeleport);
        }
    }

    public static void EnqueueSecondary(bool noSecondaryTeleport, string secondaryTeleport)
    {
        if(noSecondaryTeleport) return;
        if(secondaryTeleport == null && C.WorldVisitTPToAethernet && !C.WorldVisitTPTarget.IsNullOrEmpty())
        {
            secondaryTeleport = C.WorldVisitTPTarget;
        }
        if(!secondaryTeleport.IsNullOrEmpty())
        {
            P.TaskManager.EnqueueMulti([
                new(() => Player.Interactable),
                    new(() => TaskTryTpToAethernetDestination.Enqueue(secondaryTeleport))
                ]);
        }
    }

    public static void ApplyDefaults(ref bool? returnToGateway, ref WorldChangeAetheryte? gateway, ref bool? doNotify)
    {
        returnToGateway ??= C.DCReturnToGateway;
        gateway ??= C.WorldChangeAetheryte;
        doNotify ??= true;
    }
}
