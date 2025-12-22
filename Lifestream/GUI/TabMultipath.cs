using ECommons.Configuration;
using ECommons.ExcelServices;
using Lifestream.Data;
using Lifestream.Tasks.Utility;

namespace Lifestream.GUI;

public static class TabMultipath
{
    private static MultiPath Selected = null;
    private static int Cursor = -1;
    private static bool EditMode = false;

    public static void Draw()
    {
        if (IsKeyPressed((int)System.Windows.Forms.Keys.LButton))
            Cursor = -1;
        if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Plus, "添加"))
        {
            var x = new MultiPath();
            C.MultiPathes.Add(x);
            Selected = x;
            x.Name = x.GUID.ToString();
        }
        ImGui.SameLine();
        if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Paste, "粘贴"))
        {
            Safe(() =>
            {
                var mp = EzConfig.DefaultSerializationFactory.Deserialize<MultiPath>(Paste());
                mp.Name += " - 复制";
                mp.GUID = Guid.NewGuid();
                C.MultiPathes.Add(mp);
                Selected = mp;
            });
        }
        ImGui.SameLine();
        ImGuiEx.SetNextItemFullWidth();
        if (ImGui.BeginCombo($"##select", $"{Selected?.Name ?? "..."}"))
        {
            foreach (var x in C.MultiPathes)
            {
                if (ImGui.Selectable($"{x.Name}##{x.GUID}"))
                {
                    Selected = x;
                }
            }
            ImGui.EndCombo();
        }

        if (Selected != null)
        {
            ImGui.SetNextItemWidth(200f.Scale());
            ImGui.InputText($"##name", ref Selected.Name, 100);
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.FastForward, "执行", !P.TaskManager.IsBusy && Player.Interactable))
            {
                TaskMultipathExecute.Enqueue(Selected);
            }
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Trash, "删除", ImGuiEx.Ctrl))
            {
                new TickScheduler(() => C.MultiPathes.Remove(Selected));
                Selected = null;
            }
            ImGuiEx.Tooltip("按住 CTRL 键并单击");
            ImGui.SameLine();
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Copy, "复制"))
            {
                Copy(EzConfig.DefaultSerializationFactory.Serialize(Selected, false));
            }
            var currentPath = Selected?.Entries.FirstOrDefault(x => x.Territory == P.Territory);
            if (currentPath == null)
            {
                if (ImGui.Button($"创建 {ExcelTerritoryHelper.GetName(P.Territory)}"))
                {
                    Selected.Entries.Add(new() { Territory = P.Territory });
                }
            }
            else
            {
                if (!P.TaskManager.IsBusy)
                    S.Ipc.SplatoonManager.RenderPath(currentPath.Points, false);
                if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Plus, "在当前位置添加", EditMode))
                {
                    currentPath.Points.Add(Player.Object.Position);
                }
                if (EditMode && ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    currentPath.Points.Insert(0, Player.Object.Position);
                }
                ImGui.SameLine();
                if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.MousePointer, "在光标处添加", EditMode))
                {
                    currentPath.Points.Add(Player.Object.Position);
                    Cursor = currentPath.Points.Count - 1;
                }
                if (EditMode && ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    currentPath.Points.Insert(0, Player.Object.Position);
                    Cursor = 0;
                }
                ImGui.SameLine();
                ImGui.Checkbox("冲刺", ref currentPath.Sprint);
                ImGui.SameLine();
                ImGui.Checkbox("编辑", ref EditMode);
                if (ImGui.BeginTable("Multipath", 3, ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("种类");
                    ImGui.TableSetupColumn("点", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("控制");
                    for (var i = 0; i < currentPath.Points.Count; i++)
                    {
                        var x = currentPath.Points[i];
                        ImGui.PushID(i);
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        //TODO: delete this

                        ImGui.TableNextColumn();

                        ImGuiEx.Text($"{x}");

                        ImGui.TableNextColumn();

                        if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.MapPin, "到我的位置", EditMode))
                        {
                            currentPath.Points[i] = Player.Object.Position;
                        }
                        ImGui.SameLine(0, 1);
                        if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.MousePointer, "到光标处", EditMode))
                        {
                            Cursor = i;
                        }
                        ImGui.SameLine(0, 1);
                        if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Trash, "删除", ImGuiEx.Ctrl && EditMode))
                        {
                            var idx = i;
                            new TickScheduler(() => currentPath.Points.RemoveAt(idx));
                        }

                        if (Cursor == i)
                        {
                            if (Svc.GameGui.ScreenToWorld(ImGui.GetMousePos(), out var pos))
                            {
                                currentPath.Points[i] = pos;
                            }
                        }

                        ImGui.PopID();
                    }
                    ImGui.EndTable();
                }
            }
        }

    }
}
