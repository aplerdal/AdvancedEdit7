using System.Numerics;
using Hexa.NET.ImGui;

namespace AdvancedEdit.UI.Theme;

public class DarkTheme : Theme
{
    public override void UpdateStyle()
    {
        var style = ImGui.GetStyle();
        var colors = style.Colors;

        style.WindowRounding = 8.0f;
        style.ChildRounding = 8.0f;
        style.FrameRounding = 6.0f;
        style.PopupRounding = 6.0f;
        style.ScrollbarRounding = 6.0f;
        style.GrabRounding = 6.0f;
        style.TabRounding = 6.0f;
        
        colors[(int)ImGuiCol.Text] =                    new(0.95f, 0.96f, 0.98f, 1.00f);
        colors[(int)ImGuiCol.TextDisabled] =            new(0.36f, 0.42f, 0.47f, 1.00f);
        colors[(int)ImGuiCol.WindowBg] =                new(0.11f, 0.15f, 0.17f, 1.00f);
        colors[(int)ImGuiCol.ChildBg] =                 new(0.15f, 0.18f, 0.22f, 1.00f);
        colors[(int)ImGuiCol.PopupBg] =                 new(0.08f, 0.08f, 0.08f, 0.94f);
        colors[(int)ImGuiCol.Border] =                  new(0.43f, 0.50f, 0.56f, 0.50f);
        colors[(int)ImGuiCol.BorderShadow] =            new(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int)ImGuiCol.FrameBg] =                 new(0.20f, 0.25f, 0.29f, 1.00f);
        colors[(int)ImGuiCol.FrameBgHovered] =          new(0.12f, 0.20f, 0.28f, 1.00f);
        colors[(int)ImGuiCol.FrameBgActive] =           new(0.09f, 0.12f, 0.14f, 1.00f);
        colors[(int)ImGuiCol.TitleBg] =                 new(0.09f, 0.12f, 0.14f, 1.00f);
        colors[(int)ImGuiCol.TitleBgActive] =           new(0.12f, 0.20f, 0.28f, 1.00f);
        colors[(int)ImGuiCol.TitleBgCollapsed] =        new(0.00f, 0.00f, 0.00f, 0.51f);
        colors[(int)ImGuiCol.MenuBarBg] =               new(0.15f, 0.18f, 0.22f, 1.00f);
        colors[(int)ImGuiCol.ScrollbarBg] =             new(0.02f, 0.02f, 0.02f, 0.53f);
        colors[(int)ImGuiCol.ScrollbarGrab] =           new(0.31f, 0.31f, 0.31f, 1.00f);
        colors[(int)ImGuiCol.ScrollbarGrabHovered] =    new(0.41f, 0.41f, 0.41f, 1.00f);
        colors[(int)ImGuiCol.ScrollbarGrabActive] =     new(0.51f, 0.51f, 0.51f, 1.00f);
        colors[(int)ImGuiCol.CheckMark] =               new(0.28f, 0.56f, 1.00f, 1.00f);
        colors[(int)ImGuiCol.SliderGrab] =              new(0.28f, 0.56f, 1.00f, 1.00f);
        colors[(int)ImGuiCol.SliderGrabActive] =        new(0.37f, 0.61f, 1.00f, 1.00f);
        colors[(int)ImGuiCol.Button] =                  new(0.20f, 0.25f, 0.29f, 1.00f);
        colors[(int)ImGuiCol.ButtonHovered] =           new(0.28f, 0.56f, 1.00f, 1.00f);
        colors[(int)ImGuiCol.ButtonActive] =            new(0.37f, 0.61f, 1.00f, 1.00f);
        colors[(int)ImGuiCol.Header] =                  new(0.20f, 0.25f, 0.29f, 0.55f);
        colors[(int)ImGuiCol.HeaderHovered] =           new(0.26f, 0.59f, 0.98f, 0.80f);
        colors[(int)ImGuiCol.HeaderActive] =            new(0.26f, 0.59f, 0.98f, 1.00f);
        colors[(int)ImGuiCol.Separator] =               new(0.43f, 0.50f, 0.56f, 0.50f);
        colors[(int)ImGuiCol.SeparatorHovered] =        new(0.26f, 0.59f, 0.98f, 0.78f);
        colors[(int)ImGuiCol.SeparatorActive] =         new(0.26f, 0.59f, 0.98f, 1.00f);
        colors[(int)ImGuiCol.ResizeGrip] =              new(0.26f, 0.59f, 0.98f, 0.25f);
        colors[(int)ImGuiCol.ResizeGripHovered] =       new(0.26f, 0.59f, 0.98f, 0.67f);
        colors[(int)ImGuiCol.ResizeGripActive] =        new(0.26f, 0.59f, 0.98f, 0.95f);
        colors[(int)ImGuiCol.Tab] =                     new(0.11f, 0.15f, 0.17f, 1.00f);
        colors[(int)ImGuiCol.TabHovered] =              new(0.28f, 0.56f, 1.00f, 0.80f);
        //colors[(int)ImGuiCol.TabActive] =               new(0.20f, 0.25f, 0.29f, 1.00f);
        //colors[(int)ImGuiCol.TabUnfocused] =            new(0.07f, 0.10f, 0.15f, 0.97f);
        //colors[(int)ImGuiCol.TabUnfocusedActive] =      new(0.14f, 0.22f, 0.36f, 1.00f);
        colors[(int)ImGuiCol.DockingPreview] =          new(0.26f, 0.59f, 0.98f, 0.70f);
        colors[(int)ImGuiCol.DockingEmptyBg] =          new(0.20f, 0.20f, 0.20f, 1.00f);
        colors[(int)ImGuiCol.PlotLines] =               new(0.61f, 0.61f, 0.61f, 1.00f);
        colors[(int)ImGuiCol.PlotLinesHovered] =        new(1.00f, 0.43f, 0.35f, 1.00f);
        colors[(int)ImGuiCol.PlotHistogram] =           new(0.90f, 0.70f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotHistogramHovered] =    new(1.00f, 0.60f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.TextSelectedBg] =          new(0.26f, 0.59f, 0.98f, 0.35f);
        colors[(int)ImGuiCol.DragDropTarget] =          new(1.00f, 0.00f, 0.00f, 0.90f);
        colors[(int)ImGuiCol.NavWindowingHighlight] =   new(1.00f, 1.00f, 1.00f, 0.70f);
        colors[(int)ImGuiCol.NavWindowingDimBg] =       new(0.80f, 0.80f, 0.80f, 0.20f);
        colors[(int)ImGuiCol.ModalWindowDimBg] =        new(0.80f, 0.80f, 0.80f, 0.35f);
    }
}