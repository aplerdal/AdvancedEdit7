using System.Collections.Generic;
using System.Linq;
using AdvancedEdit.UI.Elements;
using ImGuiNET;

namespace AdvancedEdit.UI;

public class UiManager
{
    private int _windowId = 0;
    LinkedList<UiWindow> _windows = new();

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

    public void AddWindow(UiWindow window)
    {
        window.Id = _windowId++;
        _windows.AddLast(window);
    }
}