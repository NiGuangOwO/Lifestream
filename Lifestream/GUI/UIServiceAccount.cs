namespace Lifestream.GUI;

internal static class UIServiceAccount
{
    internal static void Draw()
    {
        ImGuiEx.TextWrapped($"如果您拥有多个账号，则必须将每个角色分配给正确的账号。\n要使角色出现在此列表中，请登录。");
        ImGui.Checkbox($"从 AutoRetainer 获取账号数据", ref C.UseAutoRetainerAccounts);
        List<string> ManagedByAR = [];
        if (P.AutoRetainerApi?.Ready == true && C.UseAutoRetainerAccounts)
        {
            var chars = P.AutoRetainerApi.GetRegisteredCharacters();
            foreach (var c in chars)
            {
                var data = P.AutoRetainerApi.GetOfflineCharacterData(c);
                if (data != null)
                {
                    var name = $"{data.Name}@{data.World}";
                    ManagedByAR.Add(name);
                    ImGui.SetNextItemWidth(150f.Scale());
                    if (ImGui.BeginCombo($"{name}", data.ServiceAccount == -1 ? "未选择" : $"账号 {data.ServiceAccount + 1}"))
                    {
                        for (var i = 0; i < 10; i++)
                        {
                            if (ImGui.Selectable($"账号 {i + 1}"))
                            {
                                C.ServiceAccounts[name] = i;
                                data.ServiceAccount = i;
                                P.AutoRetainerApi.WriteOfflineCharacterData(data);
                                Notify.Info($"设置保存到 AutoRetainer");
                            }
                        }
                        ImGui.EndCombo();
                    }
                    ImGui.SameLine();
                    ImGuiEx.Text(ImGuiColors.DalamudRed, $"由 AutoRetainer 管理");
                }
            }
        }
        foreach (var x in C.ServiceAccounts)
        {
            if (ManagedByAR.Contains(x.Key))
                continue;
            ImGui.SetNextItemWidth(150f.Scale());
            if (ImGui.BeginCombo($"{x.Key}", x.Value == -1 ? "未选择" : $"账号 {x.Value + 1}"))
            {
                for (var i = 0; i < 10; i++)
                {
                    if (ImGui.Selectable($"账号 {i + 1}"))
                        C.ServiceAccounts[x.Key] = i;
                }
                ImGui.EndCombo();
            }
            ImGui.SameLine();
            if (ImGui.Button("删除"))
            {
                new TickScheduler(() => C.ServiceAccounts.Remove(x.Key));
            }
        }
    }
}
