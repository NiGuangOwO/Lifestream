using Lifestream.Data;
using NightmareUI.ImGuiElements;

namespace Lifestream.GUI;

public static class TabTravelBan
{
    public static void Draw()
    {
        WorldSelector.Instance.DisplayCurrent = true;
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(EColor.RedBright, FontAwesomeIcon.ExclamationTriangle.ToIconString());
        ImGui.PopFont();
        ImGui.SameLine();
        ImGuiEx.TextWrapped(EColor.RedBright, "请注意，此功能旨在避免造成无法挽回的错误。使用此功能可能会破坏其他依赖 Lifestream 的插件。阻止特定方向的移动只会通过 Lifestream 阻止，您仍然可以手动移动。");

        ImGuiEx.LineCentered(() =>
        {
            if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Plus, "添加新条目"))
            {
                var entry = new TravelBanInfo();
                if (Player.Available)
                {
                    entry.CharaName = Player.Name;
                    entry.CharaHomeWorld = (int)Player.Object.HomeWorld.RowId;
                }
                C.TravelBans.Add(entry);
            }
        });
        if (ImGui.BeginTable("Bantable", 5, ImGuiTableFlags.RowBg | ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
        {
            ImGui.TableSetupColumn("##enabled");
            ImGui.TableSetupColumn("角色名称和服务器", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("旅行出发地");
            ImGui.TableSetupColumn("旅行目的地");
            ImGui.TableSetupColumn("##control");

            ImGui.TableHeadersRow();
            for (var i = 0; i < C.TravelBans.Count; i++)
            {
                var entry = C.TravelBans[i];
                ImGui.PushID(entry.ID);
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Checkbox("##en", ref entry.IsEnabled);
                ImGui.TableNextColumn();
                ImGuiEx.InputWithRightButtonsArea(() =>
                {
                    ImGui.InputTextWithHint("##chara", "Character name", ref entry.CharaName, 30);
                }, () =>
                {
                    ImGuiEx.Text("@");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100f.Scale());
                    WorldSelector.Instance.Draw(ref entry.CharaHomeWorld);
                });
                ImGui.TableNextColumn();

                ImGui.SetNextItemWidth(100f.Scale());
                if (ImGui.BeginCombo("##from", $"{entry.BannedFrom.Count} worlds", ImGuiComboFlags.HeightLarge))
                {
                    Utils.DrawWorldSelector(entry.BannedFrom);
                    ImGui.EndCombo();
                }
                ImGui.TableNextColumn();

                ImGui.SetNextItemWidth(100f.Scale());
                if (ImGui.BeginCombo("##to", $"{entry.BannedTo.Count} worlds", ImGuiComboFlags.HeightLarge))
                {
                    Utils.DrawWorldSelector(entry.BannedTo);
                    ImGui.EndCombo();
                }
                ImGui.TableNextColumn();

                if (ImGuiEx.IconButton(FontAwesomeIcon.Trash))
                {
                    new TickScheduler(() => C.TravelBans.Remove(entry));
                }

                ImGui.PopID();
            }

            ImGui.EndTable();
        }
    }
}
