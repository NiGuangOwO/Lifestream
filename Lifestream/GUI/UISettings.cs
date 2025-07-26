using ECommons.Configuration;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lifestream.Data;
using Lifestream.Tasks.Shortcuts;
using Lumina.Excel.Sheets;
using NightmareUI;
using NightmareUI.PrimaryUI;
using System.Globalization;
using Action = System.Action;

namespace Lifestream.GUI;

internal static unsafe class UISettings
{
    private static string AddNew = "";
    internal static void Draw()
    {
        NuiTools.ButtonTabs([[new("常规", () => Wrapper(DrawGeneral)), new("悬浮窗", () => Wrapper(DrawOverlay))], [new("专业", () => Wrapper(DrawExpert)), new("账号", () => Wrapper(UIServiceAccount.Draw)), new("传送屏蔽", TabTravelBan.Draw)]]);
    }

    private static void Wrapper(Action action)
    {
        ImGui.Dummy(new(5f));
        action();
    }

    private static void DrawGeneral()
    {
        new NuiBuilder()
        .Section("传送配置")
        .Widget(() =>
        {
            ImGui.SetNextItemWidth(200f.Scale());
            ImGuiEx.EnumCombo($"跨服传送水晶", ref C.WorldChangeAetheryte, Lang.WorldChangeAetherytes);
            ImGuiEx.HelpMarker($"你想传送到哪里来跨服");
            ImGui.Checkbox($"访问 服务器/大区 后传送到特定的以太网目的地", ref C.WorldVisitTPToAethernet);
            if(C.WorldVisitTPToAethernet)
            {
                ImGui.Indent();
                ImGui.SetNextItemWidth(250f.Scale());
                ImGui.InputText("以太网目的地，就像您在“/li”命令中使用的一样", ref C.WorldVisitTPTarget, 50);
                ImGui.Checkbox($"只作用于使用命令传送的情况，不作用于从悬浮窗传送的情况", ref C.WorldVisitTPOnlyCmd);
                ImGui.Unindent();
            }
            ImGui.Checkbox($"将天穹街添加到伊修加德基础层以太之光中", ref C.Firmament);
            ImGui.Checkbox($"跨服时自动离开非跨服队伍", ref C.LeavePartyBeforeWorldChange);
            ImGui.Checkbox($"在聊天中显示传送目的地", ref C.DisplayChatTeleport);
            ImGui.Checkbox($"在弹出通知中显示传送目的地", ref C.DisplayPopupNotifications);
            ImGui.Checkbox("重试同服务器失败的服务器访问", ref C.RetryWorldVisit);
            ImGui.Indent();
            ImGui.SetNextItemWidth(100f.Scale());
            ImGui.InputInt("重试间隔（秒）##2", ref C.RetryWorldVisitInterval.ValidateRange(1, 120));
            ImGui.SameLine();
            ImGuiEx.Text("+ 最多");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100f.Scale());
            ImGui.InputInt("秒##2", ref C.RetryWorldVisitIntervalDelta.ValidateRange(0, 120));
            ImGuiEx.HelpMarker("为了让它看起来不那么像机器人");
            ImGui.Unindent();
            //ImGui.Checkbox("Use Return instead of Teleport when possible", ref C.UseReturn);
            //ImGuiEx.HelpMarker("This includes any IPC calls");
            ImGui.Checkbox("传送完成后启用托盘通知", ref C.EnableNotifications);
            ImGuiEx.PluginAvailabilityIndicator([new("NotificationMaster")]);
        })

        .Section("捷径")
        .Widget(() =>
        {
            ImGui.SetNextItemWidth(200f.Scale());
            ImGuiEx.EnumCombo("/li 指令行为", ref C.LiCommandBehavior);
            ImGui.Checkbox("当传送到你自己的公寓时，进入里面", ref C.EnterMyApartment);
            ImGui.SetNextItemWidth(150f.Scale());
            ImGuiEx.EnumCombo("当传送到自己/部队房屋时，执行此操作", ref C.HouseEnterMode);
            ImGui.SetNextItemWidth(150f.Scale());
            if(ImGui.BeginCombo("首选旅馆", Utils.GetInnNameFromTerritory(C.PreferredInn), ImGuiComboFlags.HeightLarge))
            {
                foreach(var x in (uint[])[0, .. TaskPropertyShortcut.InnData.Keys])
                {
                    if(ImGui.Selectable(Utils.GetInnNameFromTerritory(x), x == C.PreferredInn)) C.PreferredInn = x;
                }
                ImGui.EndCombo();
            }
            if(Player.CID != 0)
            {
                ImGui.SetNextItemWidth(150f.Scale());
                var pref = C.PreferredSharedEstates.SafeSelect(Player.CID);
                var name = pref switch
                {
                    (0, 0, 0) => "第一个可用",
                    (-1, 0, 0) => "禁用",
                    _ => $"{ExcelTerritoryHelper.GetName((uint)pref.Territory)}, {pref.Ward}区, {pref.Plot}号"
                };
                if(ImGui.BeginCombo($"首选 {Player.NameWithWorld} 的共享房屋", name))
                {
                    foreach(var x in Svc.AetheryteList.Where(x => x.IsSharedHouse))
                    {
                        if(ImGui.RadioButton("第一个可用", pref == default))
                        {
                            C.PreferredSharedEstates.Remove(Player.CID);
                        }
                        if(ImGui.RadioButton("禁用", pref == (-1, 0, 0)))
                        {
                            C.PreferredSharedEstates[Player.CID] = (-1, 0, 0);
                        }
                        if(ImGui.RadioButton($"{ExcelTerritoryHelper.GetName(x.TerritoryId)}, {x.Ward}区, {x.Plot}号", pref == ((int)x.TerritoryId, x.Ward, x.Plot)))
                        {
                            C.PreferredSharedEstates[Player.CID] = ((int)x.TerritoryId, x.Ward, x.Plot);
                        }
                    }
                    ImGui.EndCombo();
                }
            }
            ImGui.Separator();
            ImGuiEx.Text("\"/li auto\" 命令优先级:");
            ImGui.SameLine();
            if(ImGui.SmallButton("Reset")) C.PropertyPrio.Clear();
            var dragDrop = Ref<ImGuiEx.RealtimeDragDrop<AutoPropertyData>>.Get(() => new("apddd", x => x.Type.ToString()));
            C.PropertyPrio.AddRange(Enum.GetValues<TaskPropertyShortcut.PropertyType>().Where(x => x != TaskPropertyShortcut.PropertyType.自动 && !C.PropertyPrio.Any(s => s.Type == x)).Select(x => new AutoPropertyData(false, x)));
            dragDrop.Begin();
            for(var i = 0; i < C.PropertyPrio.Count; i++)
            {
                var d = C.PropertyPrio[i];
                ImGui.PushID($"c{i}");
                dragDrop.NextRow();
                dragDrop.DrawButtonDummy(d, C.PropertyPrio, i);
                ImGui.SameLine();
                ImGui.Checkbox($"{d.Type}", ref d.Enabled);
                ImGui.PopID();
            }
            dragDrop.End();
            ImGui.Separator();
        })

        .Section("地图整合")
        .Widget(() =>
        {
            ImGui.Checkbox("单击地图上的城内以太水晶可快速传送", ref C.UseMapTeleport);
            ImGui.Checkbox("仅在同一地图中靠近以太水晶时处理", ref C.DisableMapClickOtherTerritory);
        })

        .Section("命令完成")
        .Widget(() =>
        {
            ImGuiEx.Text($"在聊天中输入 Lifestream 命令时建议自动完成");
            ImGui.Checkbox("启用", ref C.EnableAutoCompletion);
            ImGui.Checkbox("在固定位置显示弹出窗口", ref C.AutoCompletionFixedWindow);
            ImGui.Indent();
            ImGui.SetNextItemWidth(200f.Scale());
            ImGui.DragFloat2("位置", ref C.AutoCompletionWindowOffset, 1f);
            ImGuiEx.RadioButtonBool("从底部", "从顶部", ref C.AutoCompletionWindowBottom, sameLine: true, inverted: true);
            ImGuiEx.RadioButtonBool("从右侧", "从左侧", ref C.AutoCompletionWindowRight, sameLine: true, inverted: true);
            ImGui.Unindent();
        })

        //.Section("跨大区")
        //.Widget(() =>
        //{
        //    ImGui.Checkbox($"允许前往另一个大区", ref C.AllowDcTransfer);
        //    ImGui.Checkbox($"切换大区前离开队伍", ref C.LeavePartyBeforeLogout);
        //    ImGui.Checkbox($"如果不在休息区，则在跨大区之前传送到以太之光", ref C.TeleportToGatewayBeforeLogout);
        //    ImGui.Checkbox($"完成跨大区后传送到以太之光", ref C.DCReturnToGateway);
        //    ImGui.Checkbox($"跨大区期间允许选择服务器", ref C.DcvUseAlternativeWorld);
        //    ImGuiEx.HelpMarker("如果目标服务器不可用，但目标大区上的其他服务器可用，则会选择该服务器。正常登录后会切换服务器。");
        //    ImGui.Checkbox($"如果目标服务器不可用，重试跨大区", ref C.EnableDvcRetry);
        //    ImGui.Indent();
        //    ImGui.SetNextItemWidth(150f.Scale());
        //    ImGui.InputInt("最大重试次数", ref C.MaxDcvRetries.ValidateRange(1, int.MaxValue));
        //    ImGui.SetNextItemWidth(150f.Scale());
        //    ImGui.InputInt("重试间隔，秒", ref C.DcvRetryInterval.ValidateRange(10, 1000));
        //    ImGui.Unindent();
        //})

        .Section("地址簿")
        .Widget(() =>
        {
            ImGui.Checkbox($"禁用寻路到地块", ref C.AddressNoPathing);
            ImGuiEx.HelpMarker($"您将被留在距离地块最近的以太水晶");
            ImGui.Checkbox($"禁止进入公寓", ref C.AddressApartmentNoEntry);
            ImGuiEx.HelpMarker($"您将看到进入确认对话框");
        })

        .Section("移动")
        .Checkbox("自动移动时使用坐骑", () => ref C.UseMount)
        .Widget(() =>
        {
            Dictionary<int, string> mounts = [new KeyValuePair<int, string>(0, "随机坐骑"), .. Svc.Data.GetExcelSheet<Mount>().Where(x => x.Singular != "").ToDictionary(x => (int)x.RowId, x => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(x.Singular.GetText()))];
            ImGui.SetNextItemWidth(200f);
            ImGuiEx.Combo("首选坐骑", ref C.Mount, mounts.Keys, names: mounts);
        })
        .Checkbox("自动移动时使用冲刺", () => ref C.UseSprintPeloton)
        .Checkbox("自动移动时使用速行", () => ref C.UsePeloton)

        .Section("角色选择菜单")
        .Checkbox("从角色选择菜单启用数据中心和服务器访问", () => ref C.AllowDCTravelFromCharaSelect)
        .Checkbox("在访客数据中心上前往同一个服务器时使用跨服传送而不是数据中心访问", () => ref C.UseGuestWorldTravel)

        .Section("Wotsit整合")
        .Widget(() =>
        {
            var anyChanged = ImGui.Checkbox("启用 Wotsit 集成以传送至以太网目的地", ref C.WotsitIntegrationEnabled);
            ImGuiEx.PluginAvailabilityIndicator([new("Dalamud.FindAnything", "Wotsit")]);

            if(C.WotsitIntegrationEnabled)
            {
                ImGui.Indent();
                if(ImGui.Checkbox("包括服务器选择窗口", ref C.WotsitIntegrationIncludes.WorldSelect))
                {
                    anyChanged = true;
                }
                if(ImGui.Checkbox("包括自动传送至房产", ref C.WotsitIntegrationIncludes.PropertyAuto))
                {
                    anyChanged = true;
                }
                if(ImGui.Checkbox("包括传送至个人房屋", ref C.WotsitIntegrationIncludes.PropertyPrivate))
                {
                    anyChanged = true;
                }
                if(ImGui.Checkbox("包括传送至部队房屋", ref C.WotsitIntegrationIncludes.PropertyFreeCompany))
                {
                    anyChanged = true;
                }
                if(ImGui.Checkbox("包括传送至公寓", ref C.WotsitIntegrationIncludes.PropertyApartment))
                {
                    anyChanged = true;
                }
                if(ImGui.Checkbox("包括传送至旅馆房间", ref C.WotsitIntegrationIncludes.PropertyInn))
                {
                    anyChanged = true;
                }
                if(ImGui.Checkbox("包括传送至大国防联军", ref C.WotsitIntegrationIncludes.GrandCompany))
                {
                    anyChanged = true;
                }
                if(ImGui.Checkbox("包括传送至市场板", ref C.WotsitIntegrationIncludes.MarketBoard))
                {
                    anyChanged = true;
                }
                if(ImGui.Checkbox("包括传送至无人岛", ref C.WotsitIntegrationIncludes.IslandSanctuary))
                {
                    anyChanged = true;
                }
                if(ImGui.Checkbox("包括自动传送至以太网目的地", ref C.WotsitIntegrationIncludes.AetheryteAethernet))
                {
                    anyChanged = true;
                }
                if(ImGui.Checkbox("包括地址簿条目", ref C.WotsitIntegrationIncludes.AddressBook))
                {
                    anyChanged = true;
                }
                if(ImGui.Checkbox("包括自定义别名", ref C.WotsitIntegrationIncludes.CustomAlias))
                {
                    anyChanged = true;
                }
                ImGui.Unindent();
            }

            if(anyChanged)
            {
                PluginLog.Debug("Wotsit 集成设置已更改，立即重新初始化");
                S.Ipc.WotsitManager.TryClearWotsit();
                S.Ipc.WotsitManager.MaybeTryInit(true);
            }
        })

        .Draw();
    }

    private static void DrawOverlay()
    {
        new NuiBuilder()
        .Section("常规悬浮窗设置")
        .Widget(() =>
        {
            ImGui.Checkbox("启用悬浮窗", ref C.Enable);
            if(C.Enable)
            {
                ImGui.Indent();
                ImGui.Checkbox($"显示城内以太水晶菜单", ref C.ShowAethernet);
                ImGui.Checkbox($"显示服务器菜单", ref C.ShowWorldVisit);
                ImGui.Checkbox($"显示房区按钮", ref C.ShowWards);

                UtilsUI.NextSection();

                ImGui.Checkbox("固定Lifestream悬浮窗位置", ref C.FixedPosition);
                if(C.FixedPosition)
                {
                    ImGui.Indent();
                    ImGui.SetNextItemWidth(200f.Scale());
                    ImGuiEx.EnumCombo("水平位置", ref C.PosHorizontal);
                    ImGui.SetNextItemWidth(200f.Scale());
                    ImGuiEx.EnumCombo("垂直位置", ref C.PosVertical);
                    ImGui.SetNextItemWidth(200f.Scale());
                    ImGui.DragFloat2("偏移", ref C.Offset);

                    ImGui.Unindent();
                }

                UtilsUI.NextSection();

                ImGui.SetNextItemWidth(100f.Scale());
                ImGui.InputInt3("按钮左/右内边距", ref C.ButtonWidthArray[0]);
                ImGui.SetNextItemWidth(100f.Scale());
                ImGui.InputInt("以太水晶按钮顶部/底部填充", ref C.ButtonHeightAetheryte);
                ImGui.SetNextItemWidth(100f.Scale());
                ImGui.InputInt("服务器按钮顶部/底部填充", ref C.ButtonHeightWorld);
                ImGui.Unindent();

                ImGui.Checkbox("按钮上的文本左对齐", ref C.LeftAlignButtons);
                if(C.LeftAlignButtons)
                {
                    ImGui.SetNextItemWidth(100f);
                    ImGui.DragInt("左内边距，空格", ref C.LeftAlignPadding, 0.1f, 0, 20);
                }
            }
        })

        .Section("副本区切换")
        .Checkbox("启用", () => ref C.ShowInstanceSwitcher)
        .Checkbox("失败时重试", () => ref C.InstanceSwitcherRepeat)
        .Checkbox("切换副本区前飞行时返回地面", () => ref C.EnableFlydownInstance)
        .Widget("在服务器信息栏显示副本区编号", (x) =>
        {
            if(ImGui.Checkbox(x, ref C.EnableDtrBar))
            {
                S.DtrManager.Refresh();
            }
        })
        .SliderInt(150f, "额外按钮高度", () => ref C.InstanceButtonHeight, 0, 50)
        .Widget("重置副本区数据", (x) =>
        {
            if(ImGuiEx.Button(x, C.PublicInstances.Count > 0))
            {
                C.PublicInstances.Clear();
                EzConfig.Save();
            }
        })

        .Section("游戏窗口集成")
        .Checkbox($"如果打开以下游戏窗口，则隐藏 Lifestream", () => ref C.HideAddon)
        .If(() => C.HideAddon)
        .Widget(() =>
        {
            if(ImGui.BeginTable("HideAddonTable", 2, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("col1", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("col2");

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGuiEx.SetNextItemFullWidth();
                ImGui.InputTextWithHint("##addnew", "窗口名称... /xldata ai - 来查找", ref AddNew, 100);
                ImGui.TableNextColumn();
                if(ImGuiEx.IconButton(FontAwesomeIcon.Plus))
                {
                    C.HideAddonList.Add(AddNew);
                    AddNew = "";
                }

                List<string> focused = [];
                try
                {
                    foreach(var x in RaptureAtkUnitManager.Instance()->FocusedUnitsList.Entries)
                    {
                        if(x.Value == null) continue;
                        focused.Add(x.Value->NameString);
                    }
                }
                catch(Exception e) { e.Log(); }

                if(focused != null)
                {
                    foreach(var name in focused)
                    {
                        if(name == null) continue;
                        if(C.HideAddonList.Contains(name)) continue;
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGuiEx.TextV(EColor.Green, $"Focused: {name}");
                        ImGui.TableNextColumn();
                        ImGui.PushID(name);
                        if(ImGuiEx.IconButton(FontAwesomeIcon.Plus))
                        {
                            C.HideAddonList.Add(name);
                        }
                        ImGui.PopID();
                    }
                }

                ImGui.TableNextRow();
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, 0x88888888);
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, 0x88888888);
                ImGui.TableNextColumn();
                ImGui.Dummy(new Vector2(5f));

                foreach(var s in C.HideAddonList)
                {
                    ImGui.PushID(s);
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGuiEx.TextV(focused.Contains(s) ? EColor.Green : null, s);
                    ImGui.TableNextColumn();
                    if(ImGuiEx.IconButton(FontAwesomeIcon.Trash))
                    {
                        new TickScheduler(() => C.HideAddonList.Remove(s));
                    }
                    ImGui.PopID();
                }

                ImGui.EndTable();
            }
        })
        .EndIf()
        .Draw();

        if(C.Hidden.Count > 0)
        {
            new NuiBuilder()
            .Section("隐藏城内水晶")
            .Widget(() =>
            {
                uint toRem = 0;
                foreach(var x in C.Hidden)
                {
                    ImGuiEx.Text($"{Svc.Data.GetExcelSheet<Aetheryte>().GetRowOrDefault(x)?.AethernetName.ValueNullable?.Name.ToString() ?? x.ToString()}");
                    ImGui.SameLine();
                    if(ImGui.SmallButton($"删除##{x}"))
                    {
                        toRem = x;
                    }
                }
                if(toRem > 0)
                {
                    C.Hidden.Remove(toRem);
                }
            })
            .Draw();
        }
    }

    private static void DrawExpert()
    {
        new NuiBuilder()
        .Section("专家设置")
        .Widget(() =>
        {
            ImGui.Checkbox($"减慢城内以太水晶传送速度", ref C.SlowTeleport);
            ImGuiEx.HelpMarker($"将城内以太水晶传送速度减慢指定的量。");
            if(C.SlowTeleport)
            {
                ImGui.Indent();
                ImGui.SetNextItemWidth(200f.Scale());
                ImGui.DragInt("传送延迟（毫秒）", ref C.SlowTeleportThrottle);
                ImGui.Unindent();
            }
            ImGuiEx.CheckboxInverted($"跳过直到游戏屏幕准备好的等待", ref C.WaitForScreenReady);
            ImGuiEx.HelpMarker($"启用此选项可以加快传送速度，但要小心，您可能会被卡住。");
            ImGui.Checkbox($"隐藏进度条", ref C.NoProgressBar);
            ImGuiEx.HelpMarker($"隐藏进度条会让您无法阻止 Lifestream 执行其任务。");
            ImGuiEx.CheckboxInverted($"在更远的距离执行跨服命令时不要走到附近的以太之光", ref C.WalkToAetheryte);
            ImGui.Checkbox($"进度条在屏幕顶部", ref C.ProgressOverlayToTop);
            ImGui.Checkbox("允许自定义别名和房屋别名覆盖内置命令", ref C.AllowCustomOverrides);
            ImGui.Indent();
            ImGuiEx.TextWrapped(EColor.RedBright, "警告！其他插件可能依赖于内置命令。如果您决定启用此选项并覆盖命令，请确保情况并非如此。");
            ImGui.Unindent();
        })
        .Draw();
    }
}
