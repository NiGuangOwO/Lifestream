using AutoRetainerAPI;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using ECommons.Automation;
using ECommons.Configuration;
using ECommons.Events;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.MathHelpers;
using ECommons.SimpleGui;
using ECommons.StringHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lifestream.Enums;
using Lifestream.GUI;
using Lifestream.Schedulers;
using Lifestream.Tasks;
using Lumina.Excel.GeneratedSheets;
using NotificationMasterAPI;
using System;
using System.Security.Permissions;

namespace Lifestream
{
    public unsafe class Lifestream : IDalamudPlugin
    {
        public string Name => "Lifestream";
        internal static Lifestream P;
        internal Config Config;
        internal TaskManager TaskManager;
        internal DataStore DataStore;
        internal Memory Memory;
        internal Overlay Overlay;

        internal TinyAetheryte? ActiveAetheryte = null;
        internal AutoRetainerApi AutoRetainerApi;
        internal uint Territory => Svc.ClientState.TerritoryType;
        internal NotificationMasterApi NotificationMasterApi;
        private bool isDev = false;
        public Lifestream(DalamudPluginInterface pluginInterface)
        {
            P = this;
            ECommonsMain.Init(pluginInterface, this);
#if RELEASE
            if (Svc.PluginInterface.IsDev || !Svc.PluginInterface.SourceRepository.Contains("NiGuangOwO/DalamudPlugins/main/pluginmaster.json"))
            {
                isDev = true;
                Svc.Framework.Update += Dev;
            }
            else
#endif
            {
                new TickScheduler(delegate
                {
                    Config = EzConfig.Init<Config>();
                    EzConfigGui.Init(MainGui.Draw);
                    Overlay = new();
                    EzConfigGui.WindowSystem.AddWindow(Overlay);
                    EzConfigGui.WindowSystem.AddWindow(new ProgressOverlay());
                    EzCmd.Add("/lifestream", ProcessCommand, "打开插件配置");
                    EzCmd.Add("/li", ProcessCommand, "自动将跨服到指定的服务器（用第一个字母匹配），如果没有指定，则返回到原始服务器，如果靠近以太网，则传送到以太网目的地。以太网目的地也可以在目标服务器旁边指定。");
                    TaskManager = new()
                    {
                        AbortOnTimeout = true
                    };
                    DataStore = new();
                    ProperOnLogin.Register(() => P.DataStore.BuildWorlds());
                    Svc.Framework.Update += Framework_Update;
                    Memory = new();
                    //EqualStrings.RegisterEquality("Guilde des aventuriers (Guildes des armuriers & forgeron...", "Guilde des aventuriers (Guildes des armuriers & forgerons/Maelstrom)");
                    Svc.Toasts.ErrorToast += Toasts_ErrorToast;
                    AutoRetainerApi = new();
                    NotificationMasterApi = new(Svc.PluginInterface);
                });
            }
        }

        private bool showWarning = false;
        private void Dev(IFramework framework)
        {
            if (Svc.ClientState.IsLoggedIn && !showWarning)
            {
                showWarning = true;
                if (Svc.PluginInterface.IsDev)
                {
                    Svc.Chat.PrintError("[Lifestream] 禁止通过本地加载本汉化维护版插件！");
                }
                if (!Svc.PluginInterface.SourceRepository.Contains("NiGuangOwO/DalamudPlugins/main/pluginmaster.json"))
                {
                    Svc.Chat.PrintError($"[Lifestream] 当前安装来源 {Svc.PluginInterface.SourceRepository} 非本维护者仓库！");
                }
            }
        }

        private void Toasts_ErrorToast(ref Dalamud.Game.Text.SeStringHandling.SeString message, ref bool isHandled)
        {
            if (!Svc.ClientState.IsLoggedIn)
            {
                //430	60	8	0	False	Please wait and try logging in later.
                if (message.ExtractText().Trim() == Svc.Data.GetExcelSheet<LogMessage>().GetRow(430).Text.ExtractText().Trim())
                {
                    PluginLog.Warning($"CharaSelectListMenuError encountered");
                    EzThrottler.Throttle("CharaSelectListMenuError", 2.Minutes(), true);
                }
            }
        }

        private void ProcessCommand(string command, string arguments)
        {
            if (arguments == "stop")
            {
                Notify.Info($"放弃 {TaskManager.NumQueuedTasks + (TaskManager.IsBusy?1:0)} 个任务");
                TaskManager.Abort();
            }
            else
            {
                if (command.EqualsIgnoreCase("/lifestream") && arguments == "")
                {
                    EzConfigGui.Open();
                }
                else
                {
                    var primary = arguments.Split(' ').GetOrDefault(0);
                    var secondary = arguments.Split(' ').GetOrDefault(1);
                    if (DataStore.Worlds.TryGetFirst(x => x.StartsWith(primary == "" ? Player.HomeWorld : primary, StringComparison.OrdinalIgnoreCase), out var w))
                    {
                        TPAndChangeWorld(w, false, secondary);
                    }
                    else if(DataStore.DCWorlds.TryGetFirst(x => x.StartsWith(primary == "" ? Player.HomeWorld : primary, StringComparison.OrdinalIgnoreCase), out var dcw))
                    {
                        TPAndChangeWorld(dcw, true, secondary);
                    }
                    else
                    {
                        TaskTryTpToAethernetDestination.Enqueue(primary);
                    }
                }
            }
        }

        private void TPAndChangeWorld(string w, bool isDcTransfer = false, string secondaryTeleport = null)
        {
            if(secondaryTeleport == null && P.Config.WorldVisitTPToAethernet && !P.Config.WorldVisitTPTarget.IsNullOrEmpty())
            {
                secondaryTeleport = P.Config.WorldVisitTPTarget;
            }
            if(isDcTransfer && !P.Config.AllowDcTransfer)
            {
                Notify.Error($"配置中未启用跨大区传送。");
                return;
            }
            if (TaskManager.IsBusy)
            {
                Notify.Error("另一项任务正在进行中");
                return;
            }
            if (!Player.Available)
            {
                Notify.Error("没有玩家");
                return;
            }
            if(w == Player.CurrentWorld)
            {
                Notify.Error("已经在这个服务器了");
                return;
            }
            /*if(ActionManager.Instance()->GetActionStatus(ActionType.Spell, 5) != 0)
            {
                Notify.Error("You are unable to teleport at this time");
                return;
            }*/
            if (Svc.Party.Length > 1 && !P.Config.LeavePartyBeforeWorldChange && !P.Config.LeavePartyBeforeWorldChange)
            {
                Notify.Warning("你必须解散小队才能跨服传送");
            }
            if (!Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "TeleporterPlugin" && x.IsLoaded))
            {
                Notify.Warning("Teleporter插件未安装");
            }
            Notify.Info($"目的地： {w}");
            if (isDcTransfer)
            {
                var type = DCVType.Unknown;
                var homeDC = Player.Object.HomeWorld.GameData.DataCenter.Value.Name.ToString();
                var currentDC = Player.Object.CurrentWorld.GameData.DataCenter.Value.Name.ToString();
                var targetDC = Util.GetDataCenter(w);
                if(currentDC == homeDC)
                {
                    type = DCVType.HomeToGuest;
                }
                else
                {
                    if(targetDC == homeDC)
                    {
                        type = DCVType.GuestToHome;
                    }
                    else
                    {
                        type = DCVType.GuestToGuest;
                    }
                }
                TaskRemoveAfkStatus.Enqueue();
                if(type != DCVType.Unknown)
                {
                    if (Config.TeleportToGatewayBeforeLogout && !(TerritoryInfo.Instance()->IsInSanctuary() || ExcelTerritoryHelper.IsSanctuary(Svc.ClientState.TerritoryType)) && !(currentDC == homeDC && Player.HomeWorld != Player.CurrentWorld))
                    {
                        TaskTpToGateway.Enqueue();
                    }
                    if(Config.LeavePartyBeforeLogout && (Svc.Party.Length > 1 || Svc.Condition[ConditionFlag.ParticipatingInCrossWorldPartyOrAlliance]))
                    {
                        P.TaskManager.Enqueue(WorldChange.LeaveAnyParty);
                    }
                }
                if(type == DCVType.HomeToGuest)
                {
                    if (!Player.IsInHomeWorld) TaskTPAndChangeWorld.Enqueue(Player.HomeWorld);
                    TaskWaitUntilInHomeWorld.Enqueue();
                    TaskLogoutAndRelog.Enqueue(Player.NameWithWorld);
                    TaskChangeDatacenter.Enqueue(w, Player.Name, Player.Object.HomeWorld.Id);
                    TaskSelectChara.Enqueue(Player.Name, Player.Object.HomeWorld.Id);
                    TaskWaitUntilInWorld.Enqueue(w);

                    if (P.Config.DCReturnToGateway) TaskReturnToGateway.Enqueue();
                    TaskDesktopNotification.Enqueue($"Arrived to {w}");
                    EnqueueSecondary();
                }
                else if(type == DCVType.GuestToHome)
                {
                    TaskLogoutAndRelog.Enqueue(Player.NameWithWorld);
                    TaskReturnToHomeDC.Enqueue(Player.Name, Player.Object.HomeWorld.Id);
                    TaskSelectChara.Enqueue(Player.Name, Player.Object.HomeWorld.Id);
                    if (Player.HomeWorld != w)
                    {
                        P.TaskManager.Enqueue(WorldChange.WaitUntilNotBusy, 60.Minutes());
                        P.TaskManager.DelayNext(1000);
                        P.TaskManager.Enqueue(() => TaskTPAndChangeWorld.Enqueue(w));
                    }
                    else
                    {
                        TaskWaitUntilInWorld.Enqueue(w);
                    }
                    if (P.Config.DCReturnToGateway) TaskReturnToGateway.Enqueue();
                    TaskDesktopNotification.Enqueue($"Arrived to {w}");
                    EnqueueSecondary();
                }
                else if(type == DCVType.GuestToGuest)
                {
                    TaskLogoutAndRelog.Enqueue(Player.NameWithWorld);
                    TaskReturnToHomeDC.Enqueue(Player.Name, Player.Object.HomeWorld.Id);
                    TaskChangeDatacenter.Enqueue(w, Player.Name, Player.Object.HomeWorld.Id);
                    TaskSelectChara.Enqueue(Player.Name, Player.Object.HomeWorld.Id);
                    TaskWaitUntilInWorld.Enqueue(w);
                    if (P.Config.DCReturnToGateway) TaskReturnToGateway.Enqueue();
                    TaskDesktopNotification.Enqueue($"Arrived to {w}");
                    EnqueueSecondary();
                }
                else
                {
                    DuoLog.Error($"Error - unknown data center visit type");
                }
                Notify.Info($"Data center visit: {type}");
            }
            else
            {
                TaskRemoveAfkStatus.Enqueue();
                TaskTPAndChangeWorld.Enqueue(w);
                TaskDesktopNotification.Enqueue($"Arrived to {w}");
                EnqueueSecondary();
            }

            void EnqueueSecondary()
            {
                if (!secondaryTeleport.IsNullOrEmpty())
                {
                    P.TaskManager.Enqueue(() => Player.Interactable);
                    P.TaskManager.Enqueue(() => TaskTryTpToAethernetDestination.Enqueue(secondaryTeleport));
                }
            }
        }

        private void Framework_Update(object framework)
        {
            YesAlreadyManager.Tick();
            if(Svc.ClientState.LocalPlayer != null && DataStore.Territories.Contains(Svc.ClientState.TerritoryType))
            {
                UpdateActiveAetheryte();
            }
            else
            {
                ActiveAetheryte = null;
            }
        }

        public void Dispose()
        {
            if (isDev)
            {
                Svc.Framework.Update -= Dev;
            }
            else
            {
                Svc.Framework.Update -= Framework_Update;
                Svc.Toasts.ErrorToast -= Toasts_ErrorToast;
                Memory.Dispose();
            }
            ECommonsMain.Dispose();
            P = null;
        }

        void UpdateActiveAetheryte()
        {
            var a = Util.GetValidAetheryte();
            if (a != null)
            {
                var pos2 = a.Position.ToVector2();
                foreach (var x in DataStore.Aetherytes)
                {
                    if (x.Key.TerritoryType == Svc.ClientState.TerritoryType && Vector2.Distance(x.Key.Position, pos2) < 10)
                    {
                        if (ActiveAetheryte == null)
                        {
                            Overlay.IsOpen = true;
                        }
                        ActiveAetheryte = x.Key;
                        return;
                    }
                    foreach (var l in x.Value)
                    {
                        if (l.TerritoryType == Svc.ClientState.TerritoryType && Vector2.Distance(l.Position, pos2) < 10)
                        {
                            if (ActiveAetheryte == null)
                            {
                                Overlay.IsOpen = true;
                            }
                            ActiveAetheryte = l;
                            return;
                        }
                    }
                }
            }
            else
            {
                ActiveAetheryte = null;
            }
        }
    }
}