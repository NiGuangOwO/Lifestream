using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifestream.GUI
{
    internal static class UIServiceAccount
    {
        internal static void Draw()
        {
            ImGuiEx.TextWrapped($"如果您拥有多个角色账户，则必须将每个角色分配给正确的账户。\n若要使角色出现在此列表中，请先登录。");
            ImGui.Checkbox($"从AutoRetainer获取账户数据", ref P.Config.UseAutoRetainerAccounts);
            List<string> ManagedByAR = [];
            if (P.AutoRetainerApi?.Ready == true && P.Config.UseAutoRetainerAccounts)
            {
                var chars = P.AutoRetainerApi.GetRegisteredCharacters();
                foreach (var c in chars)
                {
                    var data = P.AutoRetainerApi.GetOfflineCharacterData(c);
                    if (data != null)
                    {
                        var name = $"{data.Name}@{data.World}";
                        ManagedByAR.Add(name);
                        ImGui.SetNextItemWidth(150f);
                        if (ImGui.BeginCombo($"{name}", data.ServiceAccount == -1 ? "未选择" : $"角色 {data.ServiceAccount+1}"))
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                if (ImGui.Selectable($"角色 {i + 1}"))
                                {
                                    P.Config.ServiceAccounts[name] = i;
                                    data.ServiceAccount = i;
                                    P.AutoRetainerApi.WriteOfflineCharacterData(data);
                                    Notify.Info($"设置保存到AutoRetainer");
                                }
                            }
                            ImGui.EndCombo();
                        }
                        ImGui.SameLine();
                        ImGuiEx.Text(ImGuiColors.DalamudRed, $"由AutoRetainer管理");
                    }
                }
            }
            foreach (var x in P.Config.ServiceAccounts)
            {
                if (ManagedByAR.Contains(x.Key)) continue;
                ImGui.SetNextItemWidth(150f);
                if (ImGui.BeginCombo($"{x.Key}", x.Value==-1?"未选择":$"角色 {x.Value+1}"))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if(ImGui.Selectable($"角色 {i+1}")) P.Config.ServiceAccounts[x.Key] = i;
                    }
                    ImGui.EndCombo();
                }
                ImGui.SameLine();
                if (ImGui.Button("删除"))
                {
                    new TickScheduler(() => P.Config.ServiceAccounts.Remove(x.Key));
                }
            }
        }
    }
}
