using System.Collections.Generic;
using AdvancedEdit.Serialization;
using AdvancedEdit.UI.Editors;
using AdvancedEdit.UI.Undo;
using AdvancedEdit.UI.Windows;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace AdvancedEdit.UI.Tools;

public class Eyedropper : TilemapEditorTool
{
    
    public override void Update(TilemapEditor editor)
    {
        Point hoveredTile = editor.Window.HoveredTile;
        var track = editor.Window.Track;
        if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && ImGui.IsWindowHovered())
        {
            if (!(hoveredTile.X >= 0 && hoveredTile.Y >= 0 && hoveredTile.X < track.Size.X && hoveredTile.Y < track.Size.Y))
                return;
            editor.ActiveTile = track.Tilemap.Layout[hoveredTile.X, hoveredTile.Y];
            var min = editor.Window.TileToWindow(hoveredTile);
            var max = editor.Window.TileToWindow(hoveredTile+new Point(1));
            ImGui.GetWindowDrawList().AddRect(min,max, Color.WhiteSmoke.PackedValue, 0, 0, 2.0f);
        }
    }
}