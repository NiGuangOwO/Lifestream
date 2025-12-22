using ECommons.Configuration;
using Lifestream.Data;
using NightmareUI.ImGuiElements;

namespace Lifestream.GUI;

public static class InputWardDetailDialog
{
    public static AddressBookEntry Entry = null;
    public static bool Open = false;
    public static void Draw()
    {
        if (Entry != null)
        {
            if (!ImGui.IsPopupOpen($"###ABEEditModal"))
            {
                Open = true;
                ImGui.OpenPopup($"###ABEEditModal");
            }
            if (ImGui.BeginPopupModal($"Editing {Entry.Name}###ABEEditModal", ref Open, ImGuiWindowFlags.AlwaysAutoResize))
            {
                if (ImGui.BeginTable($"ABEEditTable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("Edit1", ImGuiTableColumnFlags.WidthFixed, 150);
                    ImGui.TableSetupColumn("Edit2", ImGuiTableColumnFlags.WidthFixed, 250);

                    ImGui.TableNextRow();

                    ImGui.TableNextColumn();
                    ImGuiEx.TextV($"名称:");
                    ImGui.TableNextColumn();
                    ImGuiEx.SetNextItemFullWidth();
                    ImGui.InputTextWithHint($"##name", Entry.GetAutoName(), ref Entry.Name, 150);

                    ImGui.TableNextColumn();
                    ImGuiEx.TextV($"别名:");
                    ImGuiEx.HelpMarker($"如果启用并设置别名，您将能够在“li”命令中使用它：“/li 别名”。别名不区分大小写。");
                    ImGui.TableNextColumn();
                    ImGui.Checkbox($"##alias", ref Entry.AliasEnabled);
                    if (Entry.AliasEnabled)
                    {
                        ImGui.SameLine();
                        ImGuiEx.InputWithRightButtonsArea(() => ImGui.InputText($"##aliasname", ref Entry.Alias, 150), () =>
                        {
                            AddressBookEntry existing = null;
                            if (Entry.Alias != "" && C.AddressBookFolders.Any(b => b.Entries.TryGetFirst(a => a != Entry && a.AliasEnabled && a.Alias.EqualsIgnoreCase(Entry.Alias), out existing)))
                            {
                                ImGuiEx.HelpMarker($"发现别名冲突：此别名已被设置为 {existing?.Name.NullWhenEmpty() ?? existing?.GetAutoName()}", EColor.RedBright, FontAwesomeIcon.ExclamationTriangle.ToIconString());
                            }
                        });
                    }

                    ImGui.TableNextColumn();
                    ImGuiEx.TextV($"服务器:");
                    ImGui.TableNextColumn();
                    ImGuiEx.SetNextItemFullWidth();
                    WorldSelector.Instance.Draw(ref Entry.World);

                    ImGui.TableNextColumn();
                    ImGuiEx.TextV($"住宅区:");
                    ImGui.TableNextColumn();
                    if (Entry.City.RenderIcon())
                        ImGui.SameLine(0, 1);
                    ImGuiEx.SetNextItemFullWidth();
                    Utils.ResidentialAetheryteEnumSelector($"##resdis", ref Entry.City);

                    ImGui.TableNextColumn();
                    ImGuiEx.TextV($"区:");
                    ImGui.TableNextColumn();
                    ImGuiEx.SetNextItemFullWidth();
                    ImGui.InputInt($"##ward", ref Entry.Ward.ValidateRange(1, 30));

                    ImGui.TableNextColumn();
                    ImGuiEx.TextV($"房屋类型:");
                    ImGui.TableNextColumn();
                    ImGuiEx.SetNextItemFullWidth();
                    ImGuiEx.EnumRadio(ref Entry.PropertyType, true);

                    if (Entry.PropertyType == Enums.PropertyType.公寓)
                    {
                        ImGui.TableNextColumn();
                        ImGuiEx.TextV($"");
                        ImGui.TableNextColumn();
                        ImGui.Checkbox("扩建区", ref Entry.ApartmentSubdivision);

                        ImGui.TableNextColumn();
                        ImGuiEx.TextV($"房号:");
                        ImGui.TableNextColumn();
                        ImGuiEx.SetNextItemFullWidth();
                        ImGui.InputInt($"##room", ref Entry.Apartment.ValidateRange(1, 99999));
                    }

                    if (Entry.PropertyType == Enums.PropertyType.房屋)
                    {
                        ImGui.TableNextColumn();
                        ImGuiEx.TextV($"号:");
                        ImGui.TableNextColumn();
                        ImGuiEx.SetNextItemFullWidth();
                        ImGui.InputInt($"##plot", ref Entry.Plot.ValidateRange(1, 60));
                    }

                    ImGui.EndTable();
                }
                ImGuiEx.LineCentered(() =>
                {
                    if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Save, "保存并关闭"))
                    {
                        Open = false;
                        EzConfig.Save();
                    }
                });
                ImGui.EndPopup();
            }
        }
        if (!Open)
            Entry = null;
    }
}
