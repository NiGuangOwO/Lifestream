using ECommons.Configuration;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.Reflection;
using ECommons.SplatoonAPI;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lifestream.Data;
using Lifestream.Enums;
using Lifestream.Services;
using NightmareUI;
using NightmareUI.ImGuiElements;
using NightmareUI.PrimaryUI;

namespace Lifestream.GUI;
#nullable enable
public static unsafe class UIHouseReg
{
    public static ImGuiEx.RealtimeDragDrop<Vector3> PathDragDrop = new("UIHouseReg", (x) => x.ToString());

    public static void Draw()
    {
        if(Player.Available)
        {
            NuiTools.ButtonTabs([[new("个人房屋", DrawPrivate), new("部队房屋", DrawFC), new("自定义房屋", DrawCustom), new("总览", DrawOverview)]]);
        }
        else
        {
            ImGuiEx.TextWrapped("请登录才能使用此功能。");
            DrawOverview();
        }
    }

    private static ImGuiEx.RealtimeDragDrop<(ulong CID, HousePathData? Private, HousePathData? FC)> DragDropPathData = new("DragDropHPD", (x) => x.CID.ToString());
    private static string Search = "";
    private static int World = 0;
    private static WorldSelector WorldSelector = new()
    {
        DisplayCurrent = true,
        ShouldHideWorld = (x) => !C.HousePathDatas.Any(s => Utils.GetWorldFromCID(s.CID) == ExcelWorldHelper.GetName(x)),
        EmptyName = "All Worlds",
        DefaultAllOpen = true,
    };

    private static void DrawOverview()
    {
        ImGuiEx.InputWithRightButtonsArea(() =>
        {
            ImGui.InputTextWithHint("##search", "搜索...", ref Search, 50);
        }, () =>
        {
            ImGui.SetNextItemWidth(200f.Scale());
            WorldSelector.Draw(ref World);
        });
        List<(ulong CID, HousePathData? Private, HousePathData? FC)> charaDatas = [];
        foreach(var x in C.HousePathDatas.Select(x => x.CID).Distinct())
        {
            charaDatas.Add((x, C.HousePathDatas.FirstOrDefault(z => z.IsPrivate && z.CID == x), C.HousePathDatas.FirstOrDefault(z => !z.IsPrivate && z.CID == x)));
        }
        DragDropPathData.Begin();
        if(ImGuiEx.BeginDefaultTable("##charaTable", ["##move", "~Name or CID", "个人房屋", "##privateCtl", "##privateCtl2", "##privateDlm", "部队", "##FCCtl", "工房", "##workshopCtl", "##fcCtl", "##fcCtl2"]))
        {
            for(var i = 0; i < charaDatas.Count; i++)
            {
                var charaData = charaDatas[i];
                var charaName = Utils.GetCharaName(charaData.CID);
                if(Search != "" && !charaName.Contains(Search, StringComparison.OrdinalIgnoreCase)) continue;
                if(World != 0 && Utils.GetWorldFromCID(charaData.CID) != ExcelWorldHelper.GetName(World)) continue;
                ImGui.PushID($"{charaData}");
                var priv = charaData.Private;
                var fc = charaData.FC;
                var entry = (priv ?? fc)!;
                ImGui.TableNextRow();
                DragDropPathData.SetRowColor(entry.CID.ToString());
                ImGui.TableNextColumn();
                DragDropPathData.NextRow();
                DragDropPathData.DrawButtonDummy(charaData.CID.ToString(), charaDatas, i);
                ImGui.TableNextColumn();
                ImGuiEx.TextV($"{charaName}");
                ImGui.TableNextColumn();
                if(priv != null)
                {
                    NuiTools.RenderResidentialIcon((uint)priv.ResidentialDistrict.GetResidentialTerritory());
                    ImGui.SameLine();
                    ImGuiEx.Text($"{priv.Ward + 1}区, {priv.Plot + 1}号{(priv.PathToEntrance.Count > 0 ? ", +路径" : "")}");
                    ImGui.TableNextColumn();
                    if(ImGuiEx.IconButton((FontAwesomeIcon)'\ue50b', "删除个人房屋", enabled: ImGuiEx.Ctrl))
                    {
                        new TickScheduler(() => C.HousePathDatas.RemoveAll(z => z.IsPrivate && z.CID == charaData.CID));
                    }
                    ImGuiEx.Tooltip("按住 CTRL + 单击删除注册私人房屋。");
                    if(priv.PathToEntrance.Count > 0)
                    {
                        ImGui.SameLine();
                        if(ImGuiEx.IconButton((FontAwesomeIcon)'\ue566', "删除个人房屋路径", enabled: ImGuiEx.Ctrl))
                        {
                            priv.PathToEntrance.Clear();
                        }
                        ImGuiEx.Tooltip("按住 CTRL + 单击删除到个人房屋的路径。");
                    }

                    ImGui.SameLine();
                    if(ImGuiEx.IconButton(FontAwesomeIcon.Copy, "复制个人房屋路径"))
                    {
                        Copy(EzConfig.DefaultSerializationFactory.Serialize(priv)!);
                    }
                    ImGuiEx.Tooltip("将个人房屋注册数据复制到剪贴板");
                    ImGui.SameLine();
                }
                else
                {
                    ImGuiEx.TextV(ImGuiColors.DalamudGrey3, "未注册");
                    ImGui.TableNextColumn();
                }

                ImGui.TableNextColumn();

                if(ImGuiEx.IconButton(FontAwesomeIcon.Paste, "粘贴个人房屋"))
                {
                    ImportFromClipboard(charaData.CID, true);
                }
                ImGuiEx.Tooltip("从剪贴板粘贴个人房屋注册数据");

                ImGui.TableNextColumn();
                //delimiter
                ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.GetStyle().Colors[(int)ImGuiCol.TableBorderLight].ToUint());

                ImGui.TableNextColumn();
                if(fc != null)
                {
                    NuiTools.RenderResidentialIcon((uint)fc.ResidentialDistrict.GetResidentialTerritory());
                    ImGui.SameLine();
                    ImGuiEx.Text($"{fc.Ward + 1}区, {fc.Plot + 1}号{(fc.PathToEntrance.Count > 0 ? ", +路径" : "")}");
                    ImGui.TableNextColumn();
                    if(ImGuiEx.IconButton((FontAwesomeIcon)'\ue50b', "删除部队房屋", enabled: ImGuiEx.Ctrl))
                    {
                        new TickScheduler(() => C.HousePathDatas.RemoveAll(z => !z.IsPrivate && z.CID == charaData.CID));
                    }
                    ImGuiEx.Tooltip("按住 CTRL + 单击取消注册部队房屋。");
                    if(fc.PathToEntrance.Count > 0)
                    {
                        ImGui.SameLine();
                        if(ImGuiEx.IconButton((FontAwesomeIcon)'\ue566', "删除部队房屋路径", enabled: ImGuiEx.Ctrl))
                        {
                            fc.PathToEntrance.Clear();
                        }
                        ImGuiEx.Tooltip("按住 CTRL + 单击删除到部队房屋的路径。");
                    }
                }
                else
                {
                    ImGuiEx.TextV(ImGuiColors.DalamudGrey3, "未注册");
                    ImGui.TableNextColumn();
                }

                ImGui.TableNextColumn();
                if(fc == null || fc.PathToWorkshop.Count == 0)
                {
                    ImGuiEx.TextV(ImGuiColors.DalamudGrey3, "未注册");
                    ImGui.TableNextColumn();
                }
                else
                {
                    ImGuiEx.TextV($"{fc.PathToWorkshop.Count} points");
                    ImGui.TableNextColumn();
                    if(ImGuiEx.IconButton((FontAwesomeIcon)'\ue566', "删除工房路径", enabled: ImGuiEx.Ctrl))
                    {
                        fc.PathToWorkshop.Clear();
                    }
                    ImGuiEx.Tooltip("按住 CTRL + 单击删除到工房的路径。");
                }

                ImGui.TableNextColumn();

                if(fc != null)
                {
                    if(ImGuiEx.IconButton(FontAwesomeIcon.Copy, "复制部队房屋路径"))
                    {
                        Copy(EzConfig.DefaultSerializationFactory.Serialize(fc)!);
                    }
                    ImGuiEx.Tooltip("将部队房屋注册数据复制到剪贴板");
                    ImGui.SameLine();
                }

                ImGui.TableNextColumn();
                if(ImGuiEx.IconButton(FontAwesomeIcon.Paste, "粘贴部队房屋"))
                {
                    ImportFromClipboard(charaData.CID, false);
                }
                ImGuiEx.Tooltip("从剪贴板粘贴部队房屋注册数据");
                ImGui.PopID();
            }

            ImGui.EndTable();
            DragDropPathData.End();
        }
        C.HousePathDatas.Clear();
        foreach(var x in charaDatas)
        {
            if(x.Private != null) C.HousePathDatas.Add(x.Private);
            if(x.FC != null) C.HousePathDatas.Add(x.FC);
        }
    }

    private static void ImportFromClipboard(ulong cid, bool isPrivate)
    {
        new TickScheduler(() =>
        {
            try
            {
                var data = EzConfig.DefaultSerializationFactory.Deserialize<HousePathData>(Paste()!) ?? throw new NullReferenceException("剪贴板中未找到合适的数据");
                if(!data.GetType().GetFieldPropertyUnions().All(x => x.GetValue(data) != null)) throw new NullReferenceException("剪贴板包含无效数据");
                var existingData = C.HousePathDatas.FirstOrDefault(x => x.CID == cid && x.IsPrivate == isPrivate);
                var same = existingData != null && existingData.Ward == data.Ward && existingData.Plot == data.Plot && existingData.ResidentialDistrict == data.ResidentialDistrict;
                if(same || ImGuiEx.Ctrl)
                {
                    data.CID = cid;
                    var index = C.HousePathDatas.IndexOf(s => s.CID == data.CID && s.IsPrivate == isPrivate);
                    if(index == -1)
                    {
                        C.HousePathDatas.Add(data);
                    }
                    else
                    {
                        C.HousePathDatas[index] = data;
                    }
                }
                else
                {
                    Notify.Error($"此角色已注册了另一个 {(isPrivate ? "私人房屋地块" : "部队房屋地块")}。如需覆盖，请按住 CTRL 键并单击粘贴按钮。");
                }
            }
            catch(Exception e)
            {
                Notify.Error(e.Message);
                e.Log();
            }
        });
    }

    private static void DrawFC()
    {
        var data = Utils.GetFCPathData();
        DrawHousingData(data, false);
    }

    private static void DrawPrivate()
    {
        var data = Utils.GetPrivatePathData();
        DrawHousingData(data, true);
    }

    private static void DrawCustom()
    {
        if(TryGetCurrentPlotInfo(out var kind, out var ward, out var plot))
        {
            if(C.HousePathDatas.TryGetFirst(x => x.ResidentialDistrict == kind && x.Ward == ward && x.Plot == plot, out var regData))
            {
                ImGuiEx.TextWrapped($"该房屋已经由角色 {Utils.GetCharaName(regData.CID)} 注册为 {(regData.IsPrivate ? "个人房屋" : "部队房屋")}，无法注册为自定义房屋。");
            }
            else
            {
                var data = C.CustomHousePathDatas.FirstOrDefault(x => x.Ward == ward && x.Plot == plot && x.ResidentialDistrict == kind);
                if(data == null)
                {
                    if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Plus, "将此房屋登记为自定义房屋"))
                    {
                        C.CustomHousePathDatas.Add(new()
                        {
                            ResidentialDistrict = kind,
                            Plot = plot,
                            Ward = ward
                        });
                    }
                }
                else
                {
                    if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Trash, "删除该房屋", ImGuiEx.Ctrl))
                    {
                        new TickScheduler(() => C.CustomHousePathDatas.Remove(data));
                    }
                    ImGuiEx.Tooltip("Hold CTRL and click");
                    DrawHousingData_DrawPath(data, false, kind, ward, plot);
                }
            }
        }
        else
        {
            ImGuiEx.TextWrapped($"请导航至地块，将其注册为自定义房屋。注册自定义房屋将允许其路径用于共享房屋传送和地址簿传送。");
        }
    }

    private static void DrawHousingData(HousePathData? data, bool isPrivate)
    {
        var plotDataAvailable = TryGetCurrentPlotInfo(out var kind, out var ward, out var plot);
        if(data == null)
        {
            ImGuiEx.Text($"没有找到数据。 ");
            if(plotDataAvailable && Player.IsInHomeWorld)
            {
                if(ImGui.Button($"注册 {kind.GetName()}, {ward + 1}区, {plot + 1}号 作为 {(isPrivate ? "个人" : "部队")} 房屋。"))
                {
                    var newData = new HousePathData()
                    {
                        CID = Player.CID,
                        Plot = plot,
                        Ward = ward,
                        ResidentialDistrict = kind,
                        IsPrivate = isPrivate
                    };
                    C.HousePathDatas.Add(newData);
                }
            }
            else
            {
                ImGuiEx.Text($"前往您的地块注册数据。");
            }
        }
        else
        {
            ImGuiEx.TextWrapped(ImGuiColors.ParsedGreen, $"{data.ResidentialDistrict.GetName()}, {data.Ward + 1}区, {data.Plot + 1}号 已被注册为 {(data.IsPrivate ? "个人" : "部队")} 房屋。");
            if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Trash, "取消注册", ImGuiEx.Ctrl))
            {
                C.HousePathDatas.Remove(data);
            }
            ImGuiEx.Tooltip("按住 CTRL 并单击");
            ImGui.Checkbox("传送后的行为", ref data.EnableHouseEnterModeOverride);
            if(data.EnableHouseEnterModeOverride)
            {
                ImGui.SameLine();
                ImGui.SetNextItemWidth(150f.Scale());
                ImGuiEx.EnumCombo("##override", ref data.EnterModeOverride);
            }
            DrawHousingData_DrawPath(data, isPrivate, kind, ward, plot);
        }
    }

    public static void DrawHousingData_DrawPath(HousePathData data, bool isPrivate, ResidentialAetheryteKind kind, int ward, int plot)
    {
        if(data.ResidentialDistrict == kind && data.Ward == ward && data.Plot == plot)
        {
            if(!Utils.IsInsideHouse())
            {
                var path = data.PathToEntrance;
                new NuiBuilder()
                    .Section("通往房子的路径")
                    .Widget(() =>
                    {
                        ImGuiEx.TextWrapped($"创建从地块入口到房屋入口的路径。一条路径的第一个点应该稍微在你的地块内，你可以在传送后直线跑到那里，最后一个点应该靠近房子入口，你可以从那里进入房子。");

                        ImGui.PushID($"path{isPrivate}");
                        DrawPathEditor(path, data);
                        ImGui.PopID();

                    }).Draw();
            }
            else if(!isPrivate)
            {
                var path = data.PathToWorkshop;
                new NuiBuilder()
                    .Section("通往工房的路径")
                    .Widget(() =>
                    {
                        ImGuiEx.TextWrapped($"创建从房屋入口到工房/私人房间入口的路径。");

                        ImGui.PushID($"workshop");
                        DrawPathEditor(path, data);
                        ImGui.PopID();

                    }).Draw();
            }
            else
            {
                ImGuiEx.TextWrapped("进入地块范围内来编辑路径");
            }
        }
        else
        {
            ImGuiEx.TextWrapped("前往已注册地块编辑路径");
        }
    }

    public static void DrawPathEditor(List<Vector3> path, HousePathData? data = null)
    {
        if(!TerritoryWatcher.IsDataReliable())
        {
            ImGuiEx.Text(EColor.RedBright, $"您现在无法编辑房屋路径。\n请退出并进入您的房屋。");
            return;
        }
        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Plus, "添加到列表末尾"))
        {
            path.Add(Player.Position);
        }
        ImGui.SameLine();
        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Plus, "添加到列表的开头"))
        {
            path.Insert(0, Player.Position);
        }
        if(data != null)
        {
            var entryPoint = Utils.GetPlotEntrance(data.ResidentialDistrict.GetResidentialTerritory(), data.Plot);
            if(entryPoint != null)
            {
                ImGui.SameLine();
                if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Play, "测试", data.ResidentialDistrict.GetResidentialTerritory() == P.Territory && Vector3.Distance(Player.Position, entryPoint.Value) < 10f))
                {
                    P.FollowPath.Move(data.PathToEntrance, true);
                }
                ImGui.SameLine();
                if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Play, "测试工房", data.PathToWorkshop.Count > 0 && Utils.IsInsideHouse()))
                {
                    P.FollowPath.Move(data.PathToWorkshop, true);
                }
                if(ImGui.IsItemHovered())
                {
                    ImGuiEx.Tooltip($"""
                        住宅区区域: {data.ResidentialDistrict.GetResidentialTerritory()}
                        玩家区域: {P.Territory}
                        到入口点的距离: {Vector3.Distance(Player.Position, entryPoint.Value)}
                        """);
                }
            }
        }
        PathDragDrop.Begin();
        if(ImGui.BeginTable($"pathtable", 4, ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("##num");
            ImGui.TableSetupColumn("##move");
            ImGui.TableSetupColumn("坐标", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("##control");
            ImGui.TableHeadersRow();

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            ImGuiEx.Text($"地块入口");

            for(var i = 0; i < path.Count; i++)
            {
                ImGui.PushID($"point{i}");
                var p = path[i];
                ImGui.TableNextRow();
                PathDragDrop.SetRowColor(p.ToString());
                ImGui.TableNextColumn();
                PathDragDrop.NextRow();
                ImGuiEx.TextV($"{i + 1}");
                ImGui.TableNextColumn();
                PathDragDrop.DrawButtonDummy(p, path, i);
                Visualise();
                ImGui.TableNextColumn();
                ImGuiEx.TextV($"{p:F1}");
                Visualise();

                ImGui.TableNextColumn();
                if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.MapPin, "到我的位置"))
                {
                    path[i] = Player.Position;
                }
                Visualise();
                ImGui.SameLine();
                if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Trash, "删除", ImGuiEx.Ctrl))
                {
                    var toRem = i;
                    new TickScheduler(() => path.RemoveAt(toRem));
                }
                ImGuiEx.Tooltip("Hold CTRL and click");
                Visualise();
                ImGui.PopID();

                void Visualise()
                {
                    if(ImGui.IsItemHovered() && Splatoon.IsConnected())
                    {
                        var e = new Element(ElementType.CircleAtFixedCoordinates);
                        e.SetRefCoord(p);
                        e.Filled = false;
                        e.thicc = 2f;
                        e.radius = (Environment.TickCount64 % 1000f / 1000f) * 2f;
                        Splatoon.DisplayOnce(e);
                    }
                }
            }

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            ImGuiEx.Text($"房子的入口");

            ImGui.EndTable();
        }
        PathDragDrop.End();

        S.Ipc.SplatoonManager.RenderPath(path, false, true);
    }

    private static bool IsOutside()
    {
        return S.Data.ResidentialAethernet.ZoneInfo.ContainsKey(P.Territory);
    }

    public static bool TryGetCurrentPlotInfo(out ResidentialAetheryteKind kind, out int ward, out int plot)
    {
        var h = HousingManager.Instance();
        if(h != null)
        {
            ward = h->GetCurrentWard();
            plot = h->GetCurrentPlot();
            if(ward < 0 || plot < 0)
            {
                kind = default;
                return false;
            }
            kind = Utils.GetResidentialAetheryteByTerritoryType(P.Territory) ?? 0;
            return kind != 0;
        }
        kind = default;
        ward = default;
        plot = default;
        return false;
    }
}
