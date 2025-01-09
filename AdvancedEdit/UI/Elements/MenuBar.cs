using System.Collections.Generic;
using System.IO;
using AdvancedEdit.Serialization.Types;
using AdvancedEdit.UI.Windows;
using ImGuiNET;
using NativeFileDialogs.Net;

namespace AdvancedEdit.UI.Elements;

public static class MenuBar
{
    private static bool _debug;
    /// <summary>
    /// File filter for gba files
    /// </summary>
    public static readonly Dictionary<string, string> RomFilter = new Dictionary<string, string>() {{"MKSC Rom","gba"},{"All files","*"}};
    
    /// <summary>
    /// Render the menu bar
    /// </summary>
    public static void Draw()
    {
        if (_debug) ImGui.ShowMetricsWindow();
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Open ROM", "ctrl+o"))
                {
                    string? path;
                    var status = NativeFileDialogs.Net.Nfd.OpenDialog(out path, RomFilter, null);
                    if (status == NfdStatus.Ok)
                    {
                        AdvancedEdit.Instance.TrackManager = new TrackManager(new BinaryReader(File.OpenRead(path)));
                        var track = AdvancedEdit.Instance.TrackManager.Tracks[29];
                        AdvancedEdit.Instance.UiManager.AddWindow(new MapEditor(track));
                        AdvancedEdit.Instance.UiManager.AddWindow(new AiEditor(track));
                    }
                }

                ImGui.MenuItem("Save ROM", "ctrl+s");
                ImGui.Separator();
                ImGui.MenuItem("Open Project", "ctrl+shift+o");
                ImGui.MenuItem("Save Project", "ctrl+shift+s");
                ImGui.Separator();
                if (ImGui.MenuItem("Exit", "alt+f4")) AdvancedEdit.Instance.Exit();

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
                if (ImGui.MenuItem("Track Selector"))
                {
                    AdvancedEdit.Instance.UiManager.AddWindow(new TrackSelector());
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Run")) ImGui.EndMenu();

            ImGui.EndMainMenuBar();
        }
    }
}