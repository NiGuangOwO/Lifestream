using Dalamud.Interface.Components;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifestream.GUI
{
    internal static class UISettings
    {
        internal static void Draw()
        {
            ImGui.Checkbox("开启悬浮窗", ref P.Config.Enable);
            if (P.Config.Enable)
            {
                ImGui.Checkbox($"显示城内以太之光菜单", ref P.Config.ShowAethernet);
                ImGui.Checkbox($"显示服务器菜单", ref P.Config.ShowWorldVisit);
                ImGui.Checkbox("点击地图上的以太之光进行传送", ref P.Config.UseMapTeleport);
                ImGui.Checkbox($"放慢以太之光传送", ref P.Config.SlowTeleport);
                if (P.Config.SlowTeleport)
                {
                    ImGui.SetNextItemWidth(200f);
                    ImGui.DragInt("传送延迟 (ms)", ref P.Config.SlowTeleportThrottle);
                }
                ImGui.Checkbox("固定Lifestream位置", ref P.Config.FixedPosition);
                if (P.Config.FixedPosition)
                {
                    ImGui.SetNextItemWidth(200f);
                    ImGuiEx.EnumCombo("水平基准位置", ref P.Config.PosHorizontal);
                    ImGui.SetNextItemWidth(200f);
                    ImGuiEx.EnumCombo("垂直基准位置", ref P.Config.PosVertical);
                    ImGui.SetNextItemWidth(200f);
                    ImGui.DragFloat2("偏移", ref P.Config.Offset);
                }
                ImGui.SetNextItemWidth(100f);
                ImGui.InputInt("按钮左/右内填充", ref P.Config.ButtonWidth);
                ImGui.SetNextItemWidth(100f);
                ImGui.InputInt("以太之光按钮上/下内填充", ref P.Config.ButtonHeightAetheryte);
                ImGui.SetNextItemWidth(100f);
                ImGui.InputInt("服务器按钮上/下内填充", ref P.Config.ButtonHeightWorld);
                //ImGui.Checkbox($"Allow closing Lifestream with ESC", ref P.Config.AllowClosingESC2);
                //ImGuiComponents.HelpMarker("To reopen, reenter the proximity of aetheryte");
                ImGui.Checkbox($"如果游戏UI菜单处于打开状态时隐藏Lifestream", ref P.Config.HideAddon);
                ImGui.SetNextItemWidth(200f);
                ImGuiEx.EnumCombo($"跨服传送的主城", ref P.Config.WorldChangeAetheryte, Lang.WorldChangeAetherytes);
                ImGui.Checkbox($"如果可能，尝试根据跨服命令步行到附近的以太之光", ref P.Config.WalkToAetheryte);
                ImGui.Checkbox($"添加天穹街至伊修加德基础层的以太之光", ref P.Config.Firmament);
                ImGui.Checkbox($"跨服时自动离开非跨服小队", ref P.Config.LeavePartyBeforeWorldChange);
                ImGui.Checkbox($"允许跨大区", ref P.Config.AllowDcTransfer);
                ImGui.Checkbox($"跨大区前离开小队", ref P.Config.LeavePartyBeforeLogout);
                ImGui.Checkbox($"如果不在休息区内，则在跨大区之前传送到以太之光", ref P.Config.TeleportToGatewayBeforeLogout);
                ImGui.Checkbox($"跨大区后传送到以太之光", ref P.Config.DCReturnToGateway);
                ImGui.Checkbox($"在跨服/跨大区后传送到指定的以太之光", ref P.Config.WorldVisitTPToAethernet);
                if (P.Config.WorldVisitTPToAethernet)
                {
                    ImGui.SetNextItemWidth(250f);
                    ImGui.InputText("目的以太之光，就好像你在“/li”命令中使用一样", ref P.Config.WorldVisitTPTarget, 50);
                    ImGui.Checkbox($"仅通过命令传送，不通过悬浮窗传送", ref P.Config.WorldVisitTPOnlyCmd);
                }
                ImGui.Checkbox($"隐藏进度条", ref P.Config.NoProgressBar);
            }
            if (ImGui.CollapsingHeader("隐藏城内以太之光"))
            {
                uint toRem = 0;
                foreach (var x in P.Config.Hidden)
                {
                    ImGuiEx.Text($"{Svc.Data.GetExcelSheet<Aetheryte>().GetRow(x)?.AethernetName.Value?.Name.ToString() ?? x.ToString()}");
                    ImGui.SameLine();
                    if (ImGui.SmallButton($"删除##{x}"))
                    {
                        toRem = x;
                    }
                }
                if (toRem > 0)
                {
                    P.Config.Hidden.Remove(toRem);
                }
            }
        }
    }
}
