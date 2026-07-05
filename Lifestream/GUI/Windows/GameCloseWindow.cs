using NightmareUI.ImGuiElements;

namespace Lifestream.GUI.Windows;
public class GameCloseWindow : Window
{
    public int World = 0;
    private WorldSelector WorldSelector = new()
    {
        EmptyName = "已禁用",
    };
    public GameCloseWindow() : base("Lifestream 定时关闭", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.AlwaysAutoResize)
    {
        RespectCloseHotkey = false;
        ShowCloseButton = false;
    }

    public override void Draw()
    {
        if(World == 0)
        {
            ImGuiEx.Text("未激活，请选择目标服务器");
        }
        else
        {
            ImGuiEx.Text(EColor.RedBright, "已激活");
        }
        ImGuiEx.Text($"到达以下服务器后关闭游戏：");
        ImGui.SetNextItemWidth(200f.Scale());
        WorldSelector.Draw(ref World);
    }
}
