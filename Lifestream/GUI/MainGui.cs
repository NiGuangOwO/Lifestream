using ECommons.Reflection;

namespace Lifestream.GUI;

internal unsafe static class MainGui
{
    internal static void Draw()
    {
        if(!Utils.IsTeleporterInstalled())
        {
            if(ImGui.BeginTable("Notify", 1, ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn("1", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableNextRow();
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, EColor.RedDark.ToUint());
                ImGui.TableNextColumn();
                ImGuiEx.TextWrapped(EColor.White, $"您没有安装或启用 Teleporter 插件。为了正确的 Lifestream 插件操作，您需要从官方仓库安装 Teleporter 插件。单击此处打开插件安装器。");
                if (ImGuiEx.HoveredAndClicked())
                {
                    Svc.PluginInterface.OpenPluginInstaller();
                    try
                    {
                        DalamudReflector.GetService("Dalamud.Interface.Internal.DalamudInterface").Call("SetPluginInstallerSearchText", ["TeleporterPlugin"]);
                    }
                    catch(Exception e) { e.LogInternal(); }
                }
                ImGui.EndTable();
            }
        }
        KoFiButton.DrawRight();
        ImGuiEx.EzTabBar("LifestreamTabs",
            ("地址簿", TabAddressBook.Draw, null, true),
            ("设置", UISettings.Draw, null, true),
            ("账户", UIServiceAccount.Draw, null, true),
            InternalLog.ImGuiTab(),
            ("Debug", UIDebug.Draw, ImGuiColors.DalamudGrey3, true)
            );
    }
}
