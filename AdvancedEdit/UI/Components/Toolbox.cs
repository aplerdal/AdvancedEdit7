using System.Collections.Generic;
using AdvancedEdit.UI.Tools;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;

namespace AdvancedEdit.UI.Components;

public class Toolbox(Tool[] tools)
{
    public Tool ActiveTool => _tools[_activeTool];
    private Tool[] _tools = tools;
    private int _activeTool = 0;

    public void DrawToolbox()
    {
        for (var i = 0; i < _tools.Length; i++)
        {
            if (_tools[i] is ISelectableTool tool)
            {
                if (i%4!=0) ImGui.SameLine();
                
                if (ImGui.ImageButton($"tool{i}", AdvancedEdit.Instance.Icons[tool.Icon], new Vector2(16, 16))
                    || (tool.Shortcut is not null && ImGui.Shortcut((int)tool.Shortcut.Value))
                    )
                {
                    _activeTool = i;
                }

                if (_activeTool == i)
                {
                    var min = ImGui.GetItemRectMin();
                    var max = ImGui.GetItemRectMax();
                    ImGui.GetWindowDrawList().AddRect(min, max, Color.White.PackedValue, 6.0f, ImDrawFlags.None, 3);
                }
            }
        }
    }
}