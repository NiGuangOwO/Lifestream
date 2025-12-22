using Lifestream.Paissa;
using NightmareUI.ImGuiElements;
using NightmareUI.PrimaryUI;

namespace Lifestream.GUI;

public static class TabUtility
{
    public static int TargetWorldID = 0;
    private static WorldSelector WorldSelector = new()
    {
        DisplayCurrent = false,
        EmptyName = "禁用",
        ShouldHideWorld = (x) => x == Player.Object?.CurrentWorld.RowId
    };
    private static PaissaImporter PaissaImporter = new();

    public static void Draw()
    {
        new NuiBuilder()
            .Section("抵达服务器后关闭游戏")
            .Widget(() =>
            {
                ImGuiEx.SetNextItemFullWidth();
                WorldSelector.Draw(ref TargetWorldID);
            })
            .Section("从 PaissaDB 导入房屋清单")
            .Widget(() =>
            {
                ImGuiEx.SetNextItemFullWidth();
                PaissaImporter.Draw();
            })
            .Draw();
    }
}
