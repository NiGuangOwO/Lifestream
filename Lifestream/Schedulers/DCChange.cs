using Dalamud.Utility;
using ECommons.Automation;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lifestream.Tasks.Login;
using Lumina.Excel.Sheets;
using Callback = ECommons.Automation.Callback;

namespace Lifestream.Schedulers;

/// <summary>
/// 跨区通用调度器（国服精简版）：仅保留登出、标题界面操作与 World Visit 返回主世界所需方法。
/// 跨大区（超域旅行）由 DCTravelerX 通过 IPC 完成，不在此处操作 LobbyDKT 系列界面。
/// </summary>
internal static unsafe class DCChange
{
    internal static bool DCThrottle => FrameThrottler.Throttle("DCOperation", 10);
    internal static bool DCRethrottle() => FrameThrottler.Throttle("DCOperation", 10, true);

    internal static bool? WaitUntilNotBusy()
    {
        if(!Player.Available) return false;
        return Player.Object.CastInfo.ActionId == 0 && !IsOccupied() && Player.Object.IsTargetable;
    }

    internal static bool? Logout()
    {
        if(DCThrottle)
        {
            DCRethrottle();
            PluginLog.Debug($"[DCChange] Sending logout command");
            Chat.SendMessage("/logout");
            return true;
        }
        return false;
    }

    internal static bool? SelectYesLogout()
    {
        if(!Svc.ClientState.IsLoggedIn || Svc.Condition[ConditionFlag.LoggingOut])
        {
            return true;
        }

        var addon = Utils.GetLogOutYesno();
        if(addon == null || !IsAddonReady(addon))
        {
            DCRethrottle();
            return false;
        }
        if(DCThrottle)
        {
            if(EzThrottler.Throttle("Logout", 1000))
            {
                PluginLog.Debug($"[DCChange] Confirming logout");
                new AddonMaster.SelectYesno(addon).Yes();
            }
            return false;
        }
        else
        {
            return false;
        }
    }

    internal static bool? WaitUntilCanAutoLogin()
    {
        return Utils.CanAutoLogin();
    }

    internal static bool? TitleScreenClickStart()
    {
        if(!Utils.CanAutoLogin())
        {
            DCRethrottle();
            return true;
        }
        if(Utils.CanAutoLogin() && TryGetAddonByName<AtkUnitBase>("_TitleMenu", out var title) && IsAddonReady(title) && DCThrottle && EzThrottler.Throttle("TitleScreenClickStart"))
        {
            PluginLog.Debug($"[DCChange] Clicking start");
            Callback.Fire(title, true, (int)4);
            DCRethrottle();
            return false;
        }
        else
        {
            DCRethrottle();
        }
        return false;
    }

    internal static bool? OpenContextMenuForChara(string name, uint homeWorld, uint currentLoginWorld)
    {
        if(TryGetAddonByName<AddonContextMenu>("ContextMenu", out var m) && IsAddonReady(&m->AtkUnitBase))
        {
            DCRethrottle();
            return true;
        }
        if(TryGetAddonByName<AtkUnitBase>("_CharaSelectListMenu", out var addon) && IsAddonReady(addon))
        {
            TaskChangeCharacter.SelectCharacter(name, ExcelWorldHelper.GetName(homeWorld), ExcelWorldHelper.GetName(currentLoginWorld), true);
        }
        else
        {
            DCRethrottle();
        }
        return false;
    }

    internal static bool? SelectReturnToHomeWorld()
    {
        if(TryGetAddonMaster<AddonMaster.ContextMenu>(out var m) && m.IsAddonReady)
        {
            if(m.Entries.TryGetFirst(x => x.Enabled && x.Text == Svc.Data.GetExcelSheet<Lobby>().GetRow(1117).Text.GetText(), out var entry) && DCThrottle && EzThrottler.Throttle("SelectReturnToHomeWorld"))
            {
                PluginLog.Debug($"[DCChange] Selecting return to home world");
                entry.Select();
                return true;
            }
        }
        else
        {
            DCRethrottle();
        }
        return false;
    }

    internal static bool? SelectOk()
    {
        if(TryGetAddonByName<AtkUnitBase>("SelectOk", out var addon) && IsAddonReady(addon))
        {
            if(DCThrottle && EzThrottler.Throttle("SelectOk", 500))
            {
                PluginLog.Debug($"[DCChange] Selecting OK");
                Callback.Fire(addon, true, (int)0);
                return true;
            }
        }
        else
        {
            DCRethrottle();
        }
        return false;
    }
}
