using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdvancedEdit.Serialization;
using AdvancedEdit.UI;
using AdvancedEdit.UI.Windows;
using Hexa.NET.ImGui;

namespace AdvancedEdit.UI;

public class UiManager
{
    List<TrackView> _tracks = new();
    
    UiWindow _trackSelector = new TrackSelector();
    
    private Dictionary<string, int> Ids = new();
    
    private int _activeTrack;


    /// <summary>
    /// Render and handle input for all windows
    /// </summary>
    public void DrawWindows()
    {
        ImGui.DockSpaceOverViewport();
        MenuBar.Draw();
        ImGui.SetNextWindowSize(new(256,256), ImGuiCond.FirstUseEver);
        ImGui.Begin("Track Selector", _trackSelector.Flags);
        _trackSelector.Draw();
        ImGui.End();
        
        ImGui.SetNextWindowSize(new(256, 256), ImGuiCond.FirstUseEver);
        ImGui.Begin("Tracks", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.MenuBar);
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

        ImGui.SetNextWindowSize(new(256, 256), ImGuiCond.FirstUseEver);
        ImGui.Begin("Inspector");
        if (_tracks.Count > 0 && _activeTrack >= 0)
            _tracks[_activeTrack].DrawInspector();
        ImGui.End();


        // Show any errors
        ErrorManager.Update();
    }

    /// <summary>
    /// Add track to current context
    /// </summary>
    /// <param name="t">Track to be added</param>
    public void AddTrack(Track t)
    {
        if (!_tracks.Exists(x=>x.Track.Id == t.Id))
            _tracks.Add(new TrackView(t));
    }
}