using System;
using AdvancedEdit.Serialization;
using AdvancedEdit.UI.Editors;
using AdvancedEdit.UI.Undo;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace AdvancedEdit.UI.Tools;

public class RectAction : IUndoable
{
    private Rectangle _area;
    private readonly Track _track;
    private byte[,] _oldTiles;
    private byte _tile;

    public RectAction(Track track, Rectangle area, byte tile)
    {
        _track = track;
        _tile = tile;
        if (area.Left < 0 || area.Left >= track.Size.X || area.Top < 0 || area.Top >= track.Size.Y ||
            area.Right > track.Size.X || area.Bottom > track.Size.Y
            )
        {
            var x = Math.Clamp(area.X, 0, track.Size.X);
            var y = Math.Clamp(area.Y, 0, track.Size.Y);
            var width = Math.Clamp(area.Right, 0, track.Size.X)-x;
            var height = Math.Clamp(area.Bottom, 0, track.Size.Y)-y;
            area = new Rectangle(x, y, width, height);
        }
        _area = area;
        _oldTiles = new byte[area.Width,area.Height];
        for (int y = area.Top; y < area.Bottom; y++)
        for (int x = area.Left; x < area.Right; x++)
            _oldTiles[x - area.Left, y - area.Top] = track.Tilemap.Layout[x, y];
    }
    public void Do()
    {
        _track.Tilemap.SetTiles(_area, _tile);
    }

    public void Undo()
    {
        _track.Tilemap.SetTiles(_oldTiles, _area.Location);
    }
}

public class RectFill : TilemapEditorTool, ISelectableTool
{
    public string Icon => "select";
    public ImGuiKey? Shortcut => ImGuiKey.R;
    
    private bool _dragging;
    private Point _start;
    
    public override void Update(TilemapEditor editor)
    {
        if (editor.ActiveTile is null) return;
        if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            if (!_dragging)
            {
                _dragging = true;
                _start = editor.View.HoveredTile;
            }

            var p1 = _start;
            var p2 = editor.View.HoveredTile;
            var min = new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
            var max = new Point(Math.Max(p1.X, p2.X)+1, Math.Max(p1.Y, p2.Y)+1);
            ImGui.GetWindowDrawList().AddRect(editor.View.TileToWindow(min), editor.View.TileToWindow(max), Color.White.PackedValue, 0, 0, 3f);
        } else if (_dragging)
        {
            _dragging = false;
            var p1 = _start;
            var p2 = editor.View.HoveredTile;
            var min = new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
            var max = new Point(Math.Max(p1.X, p2.X)+1, Math.Max(p1.Y, p2.Y)+1);
            var rect = new Rectangle(min, max - min);
            editor.UndoManager.Do(new RectAction(editor.View.Track, rect, editor.ActiveTile.Value));
        }
    }
}