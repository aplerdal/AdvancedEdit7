using System;
using AdvancedEdit.UI.Components;
using AdvancedEdit.UI.Editors;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;

namespace AdvancedEdit.UI.Tools;

public class TileSelect : TilemapEditorTool, ISelectableTool
{
    public string Icon => "select";
    public ImGuiKey? Shortcut => ImGuiKey.S;

    private bool _dragging;
    private Point _start;
    
    public override void Update(TilemapEditor editor)
    {
        if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            if (!_dragging && ImGui.IsWindowHovered())
            {
                _dragging = true;
                _start = editor.View.HoveredTile;
            }

            if (_dragging)
            {
                var p1 = _start;
                var p2 = editor.View.HoveredTile;
                var min = new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
                var max = new Point(Math.Max(p1.X, p2.X) + 1, Math.Max(p1.Y, p2.Y) + 1);
                ImGui.GetWindowDrawList().AddRect(editor.View.TileToWindow(min), editor.View.TileToWindow(max),
                    Color.White.PackedValue, 0, 0, 3f);
            }
        }
        else if (_dragging)
        {
            _dragging = false;
            var p1 = _start;
            p1 = new Point(
                Math.Clamp(p1.X, 0, editor.View.Track.Size.X-1),
                Math.Clamp(p1.Y, 0, editor.View.Track.Size.Y-1)
                );
            var p2 = editor.View.HoveredTile;
            p2 = new Point(
                Math.Clamp(p2.X, 0, editor.View.Track.Size.X-1),
                Math.Clamp(p2.Y, 0, editor.View.Track.Size.Y-1)
            );
            if (p1 == p2)
            {
                // Clear selection, started and ended on same tile.
                editor.SelectionManager.Clear();
            }
            else
            {
                var min = new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
                var max = new Point(Math.Max(p1.X, p2.X) + 1, Math.Max(p1.Y, p2.Y) + 1);
                var mode = ImGui.IsKeyDown(ImGuiKey.ModShift) ? SelectionMode.Add : SelectionMode.Replace;
                editor.SelectionManager.SelectRect(new Rectangle(min, max - min), mode);
            }
        }
    }
}