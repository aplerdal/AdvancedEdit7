using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdvancedEdit.UI.Elements;
using AdvancedEdit.UI.Windows;
using ImGuiNET;

namespace AdvancedEdit.UI;

public class UiManager
{
    List<TrackView> _tracks = new();
    
    UiWindow _trackSelector = new TrackSelector();
    
    private Dictionary<string, int> Ids = new();
    
    private int _activeTrack;


    /// <summary>
    /// Render and handle input for all windows from the current window list;
    /// </summary>
    public void DrawWindows()
    {
        ImGui.DockSpaceOverViewport();
        MenuBar.Draw();

        ImGui.Begin("Track Selector", ref _trackSelector.IsOpen, _trackSelector.Flags);
        _trackSelector.Draw();
        ImGui.End();

        ImGui.Begin("Tracks", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        if (ImGui.BeginTabBar("track_bar", ImGuiTabBarFlags.Reorderable))
        {
            _activeTrack = -1;
            for (var i = 0; i < _tracks.Count; i++)
            {
                var track = _tracks[i];
                if (track.IsOpen && ImGui.BeginTabItem($"{track.Name}##{track.WindowId}", ref track.IsOpen, ImGuiTabItemFlags.None))
                {
                    Debug.Assert(_activeTrack == -1);
                    _activeTrack = i;
                    ImGui.Text("");
                    ImGui.PushClipRect(ImGui.GetCursorPos(), ImGui.GetWindowPos()+ImGui.GetWindowSize(), true);
                    track.Draw(true);
                    ImGui.PopClipRect();
                    ImGui.EndTabItem();
                }

                if (!track.IsOpen)
                {
                    _tracks.RemoveAt(i);
                    i -= 1;
                    _activeTrack = -1;
                }
            }

            ImGui.EndTabBar();
        }
        ImGui.End();

        ImGui.Begin("Inspector");
        if (_tracks.Count > 0 && _activeTrack >= 0)
            _tracks[_activeTrack].DrawInspector();
        ImGui.End();
        // if window is focused draw inspector and call update
    }

    /// <summary>
    /// Add track to current context
    /// </summary>
    /// <param name="track">Track to be added</param>
    public void AddTrack(TrackView track)
    {
        _tracks.Add(track);
    }
}