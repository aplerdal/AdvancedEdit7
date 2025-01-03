using ImGuiNET;

namespace AdvancedEdit.UI.Elements;

public static class MenuBar
{
    private static bool _debug;

    public static void Draw(AdvancedEdit ae)
    {
        if (_debug) ImGui.ShowMetricsWindow();
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Open ROM", "ctrl+o"))
                {
                }

                ImGui.MenuItem("Save ROM", "ctrl+s");
                ImGui.Separator();
                ImGui.MenuItem("Open Project", "ctrl+shift+o");
                ImGui.MenuItem("Save Project", "ctrl+shift+s");
                ImGui.Separator();
                if (ImGui.MenuItem("Exit", "alt+f4")) ae.Exit();

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit"))
            {
                ImGui.MenuItem("Undo", "ctrl+z");
                ImGui.MenuItem("Redo", "ctrl+y");
                ImGui.Separator();
                ImGui.MenuItem("Copy", "ctrl+c");
                ImGui.MenuItem("Paste", "ctrl+v");
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Window"))
            {
                ImGui.MenuItem("Debug Window", null, ref _debug);
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Run")) ImGui.EndMenu();

            ImGui.EndMainMenuBar();
        }
    }
}