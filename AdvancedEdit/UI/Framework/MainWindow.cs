using System.Collections.Generic;
using System.ComponentModel;
using AdvancedEdit.UI.Framework.Windows;
using AdvancedEdit.UI.Framework.Menu;
using AdvancedEdit.UI.Renderer;
using AdvancedEdit.UI.Themes;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AdvancedEdit.UI.Framework;

public class MainWindow : DockSpaceWindow
{
    public List<Window> Windows = new List<Window>();
    public List<MenuItem> MenuItems = new List<MenuItem>();

    public static bool ForceFocus = true;

    protected static Game _game;
    float font_scale = 1.0f;
    bool fullscreen = true;
    bool p_open = true;
    ImGuiDockNodeFlags dockspace_flags = ImGuiDockNodeFlags.None;

    public MainWindow() : base("dock_main")
    {
    }

    internal void Init(Game game)
    {
        _game = game;
    }

    public void OnApplicationLoad()
    {
        Name = "WindowSpace";

        //Disable the docking buttons
        ImGui.GetStyle().WindowMenuButtonPosition = ImGuiDir.None;

        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        ImGui.GetIO().ConfigDragClickToInputText = true;
        ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;

        //Load theme files
        ThemeManager.Load();
        ThemeManager.UpdateTheme(new LightTheme());
        OnLoad();
    }

    public void OnRenderFrame()
    {
        var windowFlags = ImGuiWindowFlags.NoDocking;

        if (fullscreen)
        {
            ImGuiViewportPtr viewport = ImGui.GetMainViewport();
            ImGui.SetNextWindowPos(viewport.WorkPos);
            ImGui.SetNextWindowSize(viewport.WorkSize);
            ImGui.SetNextWindowViewport(viewport.ID);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            windowFlags |= ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize |
                           ImGuiWindowFlags.NoMove;
            windowFlags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;
        }

        if ((dockspace_flags & ImGuiDockNodeFlags.PassthruCentralNode) != 0)
            windowFlags |= ImGuiWindowFlags.NoBackground;

        ImGui.Begin("WindowSpace", ref p_open, windowFlags);

        if (fullscreen)
            ImGui.PopStyleVar(2);

        Render();

        ImGui.End();
    }

    public override void Render()
    {
        if (ImGui.BeginMainMenuBar())
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new System.Numerics.Vector2(8, 6));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2(8, 4));
            ImGui.PushStyleColor(ImGuiCol.Separator, new System.Numerics.Vector4(0.4f, 0.4f, 0.4f, 1.0f));

            foreach (var item in MenuItems)
                item.Render(false);

            MainMenuDraw();

            ImGui.PopStyleVar(2);
            ImGui.PopStyleColor(1);

            //Display FPS at right side of screen
            float width = ImGui.GetWindowWidth();
            float framerate = ImGui.GetIO().Framerate;

            ImGui.SetCursorPosX(width - 100);
            ImGui.Text($"({framerate:0.#} FPS)");

            ImGui.EndMainMenuBar();
        }
    }

    public virtual void MainMenuDraw()
    {
    }

    public virtual void OnResize(int width, int height)
    {
    }

    public virtual void OnFileDrop(string filename)
    {
    }

    public virtual void OnKeyDown(Keys key)
    {
    }

    public virtual void OnClosing(object? sender, ExitingEventArgs? e)
    {
    }
}