using ECommons.Funding;

namespace Lifestream.GUI;

internal static unsafe class MainGui
{
    internal static void Draw()
    {
        PatreonBanner.DrawRight();
        ImGuiEx.EzTabBar("LifestreamTabs", PatreonBanner.Text,
            ("地址簿", TabAddressBook.Draw, null, true),
            ("房屋登记", UIHouseReg.Draw, null, true),
            ("自定义别名", TabCustomAlias.Draw, null, true),
            ("实用", TabUtility.Draw, null, true),
            ("设置", UISettings.Draw, null, true),
            ("帮助", DrawHelp, null, true),
            ("Debug", UIDebug.Draw, ImGuiColors.DalamudGrey3, true)
            );
    }

    private static void DrawHelp()
    {
        ImGuiEx.TextWrapped(Lang.Help);
    }
}
