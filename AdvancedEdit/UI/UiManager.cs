using System.Collections.Generic;
using System.Linq;
using AdvancedEdit.UI.Elements;
using ImGuiNET;

namespace AdvancedEdit.UI;

public class UiManager
{
    private int _windowId = 0;
    LinkedList<UiWindow> _windows = new();
    private Dictionary<string, int> Ids = new();
    private UiWindow _focused = null;

    /// <summary>
    /// Render and handle input for all windows from the current window list;
    /// </summary>
    public void DrawWindows()
    {
        ImGui.DockSpaceOverViewport();
        MenuBar.Draw();
        foreach (var window in _windows.ToArray())
        {
            if (!window.IsOpen) _windows.Remove(window);
            ImGui.Begin($"{window.Name}###{window.WindowId + window.Id}", ref window.IsOpen, window.Flags);
            if (ImGui.IsWindowFocused())
            {
                _focused = window;
            }
            window.Draw(_focused == window);
            ImGui.End();
        }

        if (_focused is IInspector inspector)
        {
            ImGui.Begin("Inspector");
            inspector.DrawInspector();
            ImGui.End();
        }
        // if window is focused draw inspector and call update
    }

    /// <summary>
    /// Add window to current context
    /// </summary>
    /// <param name="window">Window to be added</param>
    public void AddWindow(UiWindow window)
    {
        if (!Ids.ContainsKey(window.WindowId))
            Ids[window.WindowId] = 0;
        else 
            Ids[window.WindowId]++;
        window.Id = Ids[window.WindowId];
        _windows.AddLast(window);
    }
}