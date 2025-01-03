using System.Collections.Generic;
using AdvancedEdit.UI.Elements;
using ImGuiNET;

namespace AdvancedEdit.UI;

public class UiManager
{
    private int _windowId = 0;
    LinkedList<UiWindow> _windows = new();

    public void DrawWindows(AdvancedEdit ae)
    {
        ImGui.DockSpaceOverViewport();
        MenuBar.Draw(ae);
        foreach (var window in _windows)
        {
            if (!window.IsOpen) _windows.Remove(window);
            ImGui.Begin($"{window.Name}##{window.Id}", ref window.IsOpen, ImGuiWindowFlags.None);
            window.Draw(ae);
            ImGui.End();
        }
        // if window is focused draw inspector and call update
    }

    public void AddWindow(UiWindow window)
    {
        window.Id = _windowId++;
        _windows.AddLast(window);
    }
}