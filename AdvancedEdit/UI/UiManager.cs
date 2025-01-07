using System.Collections.Generic;
using System.Linq;
using AdvancedEdit.UI.Elements;
using ImGuiNET;

namespace AdvancedEdit.UI;

public class UiManager
{
    private int _windowId = 0;
    LinkedList<UiWindow> _windows = new();

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
            ImGui.Begin($"{window.Name}##{window.Id}", ref window.IsOpen, window.Flags);
            window.Draw(ImGui.IsWindowFocused());
            ImGui.End();


            if (window is IInspector inspector)
            {
                ImGui.Begin("Inspector");
                inspector.DrawInspector();   
                ImGui.End();
            }
        }
        // if window is focused draw inspector and call update
    }

    /// <summary>
    /// Add window to current context
    /// </summary>
    /// <param name="window">Window to be added</param>
    public void AddWindow(UiWindow window)
    {
        window.Id = _windowId++;
        _windows.AddLast(window);
    }
}