using System.Collections.Generic;
using System.IO;
using AdvancedEdit.Serialization;
using AdvancedEdit.UI.Windows;
using ImGuiNET;
using NativeFileDialogs.Net;

namespace AdvancedEdit.UI;

public static class MenuBar
{
    /// <summary>
    /// File filter for gba files
    /// </summary>
    public static readonly Dictionary<string, string> RomFilter = new() {{"MKSC Rom","gba"},{"All files","*"}};
    
    /// <summary>
    /// Render the menu bar
    /// </summary>
    public static void Draw()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Open ROM", "ctrl+o"))
                {
                    string? path;
                    var status = Nfd.OpenDialog(out path, RomFilter, null);
                    if (status == NfdStatus.Ok && path is not null)
                    {
                        var file = File.OpenRead(path);
                        AdvancedEdit.Instance.TrackManager = new TrackManager(new BinaryReader(file), path);
                        if (TrackManager.Tracks != null) {
                            AdvancedEdit.Instance.TrackManager.RomPath = path;
                            AdvancedEdit.Instance.UiManager.AddTrack(new TrackView(TrackManager.Tracks[29]));
                        }
                        // TODO: Show user error.
                        file.Close();
                    }
                }

                if (ImGui.MenuItem("Save ROM", "ctrl+s"))
                {
                    string? path;
                    var status = Nfd.SaveDialog(out path, RomFilter, "mksc_modified.gba");
                    if (status == NfdStatus.Ok && path is not null)
                    {
                        File.Copy(AdvancedEdit.Instance.TrackManager!.RomPath, path, true);
                        var file = File.OpenWrite(path);
                        AdvancedEdit.Instance.TrackManager.Save(new BinaryWriter(file));
                        file.Close();
                        ImGui.OpenPopup("saved");
                    }
                }
                
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

            ImGui.EndMainMenuBar();

            if (ImGui.BeginPopupModal("saved")) {
                ImGui.Text("Sucessfully saved file.");
                if (ImGui.Button("Close"))
                    ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
            }
        }
    }
}