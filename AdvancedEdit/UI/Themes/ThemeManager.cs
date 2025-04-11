using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Hexa.NET.ImGui;
using Newtonsoft.Json;
using Formatting = System.Xml.Formatting;

namespace AdvancedEdit.UI.Themes;

public class ThemeManager
{
    public virtual string Name { get; set; }

    public static List<string> ThemeFilePaths = new List<string>();

    public static Theme Theme;

    public static void UpdateTheme(string filePath)
    {
        UpdateTheme(JsonConvert.DeserializeObject<Theme>(File.ReadAllText(filePath)));
    }

    /// <summary>
    /// Updates the current theme of the application.
    /// </summary>
    public static void UpdateTheme(Theme theme)
    {
        Theme = theme;

        ImGui.GetStyle().WindowPadding = new Vector2(2);
        ImGui.GetStyle().FrameRounding = 5;

        ImGui.GetStyle().Colors[(int)ImGuiCol.Text] = theme.Text;
        ImGui.GetStyle().Colors[(int)ImGuiCol.WindowBg] = theme.WindowBg;
        ImGui.GetStyle().Colors[(int)ImGuiCol.ChildBg] = theme.ChildBg;
        ImGui.GetStyle().Colors[(int)ImGuiCol.Border] = theme.Border;
        ImGui.GetStyle().Colors[(int)ImGuiCol.PopupBg] = theme.PopupBg;
        ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg] = theme.FrameBg;
        ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBgHovered] = theme.FrameBgHovered;
        ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBgActive] = theme.FrameBgActive;
        ImGui.GetStyle().Colors[(int)ImGuiCol.TitleBg] = theme.TitleBg;
        ImGui.GetStyle().Colors[(int)ImGuiCol.TitleBgActive] = theme.TitleBgActive;
        ImGui.GetStyle().Colors[(int)ImGuiCol.CheckMark] = theme.CheckMark;
        ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonActive] = theme.ButtonActive;
        ImGui.GetStyle().Colors[(int)ImGuiCol.Header] = theme.Header;
        ImGui.GetStyle().Colors[(int)ImGuiCol.HeaderHovered] = theme.HeaderHovered;
        ImGui.GetStyle().Colors[(int)ImGuiCol.HeaderActive] = theme.HeaderActive;
        ImGui.GetStyle().Colors[(int)ImGuiCol.SeparatorHovered] = theme.SeparatorHovered;
        ImGui.GetStyle().Colors[(int)ImGuiCol.SeparatorActive] = theme.SeparatorActive;
        ImGui.GetStyle().Colors[(int)ImGuiCol.Separator] = theme.Separator;
        ImGui.GetStyle().Colors[(int)ImGuiCol.Tab] = theme.Tab;
        ImGui.GetStyle().Colors[(int)ImGuiCol.TabHovered] = theme.TabHovered;
        ImGui.GetStyle().Colors[(int)ImGuiCol.TabSelected] = theme.TabActive;
        ImGui.GetStyle().Colors[(int)ImGuiCol.TabDimmed] = theme.TabDimmed;
        ImGui.GetStyle().Colors[(int)ImGuiCol.TabDimmedSelected] = theme.TabDimmedSelected;
        ImGui.GetStyle().Colors[(int)ImGuiCol.DockingPreview] = theme.DockingPreview;
        ImGui.GetStyle().Colors[(int)ImGuiCol.DockingEmptyBg] = theme.DockingEmptyBg;
        ImGui.GetStyle().Colors[(int)ImGuiCol.TextSelectedBg] = theme.TextSelectedBg;
        ImGui.GetStyle().Colors[(int)ImGuiCol.NavWindowingHighlight] = theme.NavWindowingHighlight;
        ImGui.GetStyle().Colors[(int)ImGuiCol.Button] = theme.Button;
        ImGui.GetStyle().Colors[(int)ImGuiCol.MenuBarBg] = theme.WindowBg;
    }

    public static void Load()
    {
        string folder = Path.Combine("Resources", "Themes");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        ThemeFilePaths.Clear();
        foreach (var theme in Directory.GetFiles(folder))
        {
            ThemeFilePaths.Add(theme);
        }
    }

    public void Export(string fileName)
    {
        var json = JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(fileName, json);
    }
}