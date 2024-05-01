using Lumina.Excel.GeneratedSheets;

namespace Lifestream.GUI;

internal static class UISettings
{
    internal static void Draw()
    {
        UtilsUI.DrawSection("Overlay Settings", null, () =>
        {
            ImGui.Checkbox("启用悬浮窗", ref P.Config.Enable);
            if (P.Config.Enable)
            {
                ImGui.Indent();
                ImGui.Checkbox($"显示城内以太之光菜单", ref P.Config.ShowAethernet);
                ImGui.Checkbox($"显示服务器菜单", ref P.Config.ShowWorldVisit);
                ImGui.Checkbox($"Display Housing Ward buttons", ref P.Config.ShowWards);

                UtilsUI.NextSection();

                ImGui.Checkbox($"如果游戏UI菜单处于打开状态时隐藏 Lifestream", ref P.Config.HideAddon);

                UtilsUI.NextSection();

                ImGui.Checkbox("固定 Lifestream 位置", ref P.Config.FixedPosition);
                if (P.Config.FixedPosition)
                {
                    ImGui.Indent();
                    ImGui.SetNextItemWidth(200f);
                    ImGuiEx.EnumCombo("水平基准位置", ref P.Config.PosHorizontal);
                    ImGui.SetNextItemWidth(200f);
                    ImGuiEx.EnumCombo("垂直基准位置", ref P.Config.PosVertical);
                    ImGui.SetNextItemWidth(200f);
                    ImGui.DragFloat2("偏移", ref P.Config.Offset);

                    UtilsUI.NextSection();

                    ImGui.SetNextItemWidth(100f);
                    ImGui.InputInt("按钮左/右内填充", ref P.Config.ButtonWidth);
                    ImGui.SetNextItemWidth(100f);
                    ImGui.InputInt("以太之光按钮上/下内填充", ref P.Config.ButtonHeightAetheryte);
                    ImGui.SetNextItemWidth(100f);
                    ImGui.InputInt("服务器按钮上/下内填充", ref P.Config.ButtonHeightWorld);

                    ImGui.Unindent();
                }
                ImGui.Unindent();
            }
        });

        UtilsUI.DrawSection("Map Integration", null, () =>
        {
            ImGui.Checkbox("Click Aethernet Shard on map for quick teleport", ref P.Config.UseMapTeleport);
        });

        UtilsUI.DrawSection("Teleport Configuration", null, () =>
        {

            ImGui.SetNextItemWidth(200f);
            ImGuiEx.EnumCombo($"跨服传送的主城", ref P.Config.WorldChangeAetheryte, Lang.WorldChangeAetherytes);
            ImGuiEx.HelpMarker($"Where would you like to teleport for world changes");
            ImGui.Checkbox($"在跨服/跨大区后传送到指定的以太之光", ref P.Config.WorldVisitTPToAethernet);
            if (P.Config.WorldVisitTPToAethernet)
            {
                ImGui.Indent();
                ImGui.SetNextItemWidth(250f);
                ImGui.InputText("Aethernet destination, as if you'd use in \"/li\" command", ref P.Config.WorldVisitTPTarget, 50);
                ImGui.Checkbox($"Only teleport from command but not from overlay", ref P.Config.WorldVisitTPOnlyCmd);
                ImGui.Unindent();
            }
            ImGui.Checkbox($"Add firmament location into Foundation aetheryte", ref P.Config.Firmament);
            ImGui.Checkbox($"Automatically leave non cross-world party upon changing world", ref P.Config.LeavePartyBeforeWorldChange);
        });

        UtilsUI.DrawSection("Cross-Datacenter", null, () =>
        {
            ImGui.Checkbox($"Allow travelling to another data center", ref P.Config.AllowDcTransfer);
            ImGui.Checkbox($"Leave party before switching data center", ref P.Config.LeavePartyBeforeLogout);
            ImGui.Checkbox($"Teleport to gateway aetheryte before switching data center if not in sanctuary", ref P.Config.TeleportToGatewayBeforeLogout);
            ImGui.Checkbox($"Teleport to gateway aetheryte after completing data center travel", ref P.Config.DCReturnToGateway);
        });

        UtilsUI.DrawSection("Address Book", null, () =>
				{
						ImGui.Checkbox($"Disable pathing to a plot", ref P.Config.AddressNoPathing);
						ImGuiEx.HelpMarker($"You will be left at a closest aetheryte to the ward");
						ImGui.Checkbox($"Disable entering an apartment", ref P.Config.AddressApartmentNoEntry);
						ImGuiEx.HelpMarker($"You will be left at an entry confirmation dialogue");
				});

        UtilsUI.DrawSection("Expert Settings", null, () =>
        {
            ImGui.Checkbox($"Slow down aetheryte teleporting", ref P.Config.SlowTeleport);
            ImGuiEx.HelpMarker($"Slows down aethernet teleportation by specified amount.");
            if (P.Config.SlowTeleport)
            {
                ImGui.Indent();
                ImGui.SetNextItemWidth(200f);
                ImGui.DragInt("Teleport delay (ms)", ref P.Config.SlowTeleportThrottle);
                ImGui.Unindent();
            }
            ImGuiEx.CheckboxInverted($"Skip waiting until game screen is ready", ref P.Config.WaitForScreenReady);
            ImGuiEx.HelpMarker($"Enable this option for faster teleports but be careful that you may get stuck.");
            ImGui.Checkbox($"Hide progress bar", ref P.Config.NoProgressBar);
            ImGuiEx.HelpMarker($"Hiding progress bar leaves you with no way to stop Lifestream from executing it's tasks.");
            ImGuiEx.CheckboxInverted($"Don't walk to nearby aetheryte on world change command from greater distance", ref P.Config.WalkToAetheryte);
        });

        if(P.Config.Hidden.Count > 0) UtilsUI.DrawSection("Hidden Aetherytes", null, () =>
        {
            uint toRem = 0;
            foreach (var x in P.Config.Hidden)
            {
                ImGuiEx.Text($"{Svc.Data.GetExcelSheet<Aetheryte>().GetRow(x)?.AethernetName.Value?.Name.ToString() ?? x.ToString()}");
                ImGui.SameLine();
                if (ImGui.SmallButton($"Delete##{x}"))
                {
                    toRem = x;
                }
            }
            if (toRem > 0)
            {
                P.Config.Hidden.Remove(toRem);
            }
        });
    }
}
