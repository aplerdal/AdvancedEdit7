using System.Collections.Generic;
using AdvancedEdit.Serialization;
using AdvancedEdit.UI.Editors;
using AdvancedEdit.UI.Undo;
using AdvancedEdit.UI.Windows;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;

namespace AdvancedEdit.UI.Tools;

public class Eyedropper : TilemapEditorTool, ISelectableTool
{
    public string Icon => "eyedropper";
    public ImGuiKey? Shortcut => ImGuiKey.V;
    
    public override void Update(TilemapEditor editor)
    {
        Point hoveredTile = editor.View.HoveredTile;
        var track = editor.View.Track;
        if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && ImGui.IsWindowHovered())
        {
            if (!(hoveredTile.X >= 0 && hoveredTile.Y >= 0 && hoveredTile.X < track.Size.X && hoveredTile.Y < track.Size.Y))
                return;
            editor.ActiveTile = track.Tilemap.Layout[hoveredTile.X, hoveredTile.Y];
        }

        var min = editor.View.TileToWindow(hoveredTile);
        var max = editor.View.TileToWindow(hoveredTile + new Point(1));
        ImGui.GetWindowDrawList().AddRect(min, max, Color.WhiteSmoke.PackedValue, 0, 0, 2.0f);
    }
}