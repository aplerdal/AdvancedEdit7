using System.Collections;
using System.Collections.Generic;
using AdvancedEdit.Serialization;
using AdvancedEdit.UI.Editors;
using AdvancedEdit.UI.Undo;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;

namespace AdvancedEdit.UI.Tools;

public class TileFillAction(Track track, HashSet<Point> positions, byte newTile, byte oldTile) : IUndoable
{
    public void Do()
    {
        track.Tilemap.SetTiles(positions, newTile);
    }
    public void Undo()
    {
        track.Tilemap.SetTiles(positions, oldTile);
    }
}

public class SelectionFillAction(Track track, HashSet<Point> positions, byte newTile) : IUndoable
{
    private Dictionary<Point, byte>? _oldTiles;
    public void Do()
    {
        if (_oldTiles is null)
        {
            _oldTiles = new Dictionary<Point, byte>();
            foreach (var point in positions)
            {
                _oldTiles.Add(point, track.Tilemap.Layout[point.X, point.Y]);
            }
        }
        track.Tilemap.SetTiles(positions, newTile);
    }

    public void Undo()
    {
        track.Tilemap.SetTiles(_oldTiles!);
    }
}
public class Bucket : TilemapEditorTool, ISelectableTool
{
    public string Icon => "bucket";
    public ImGuiKey? Shortcut => ImGuiKey.B;
    public override void Update(TilemapEditor editor)
    {
        Point hoveredTile = editor.View.HoveredTile;
        var track = editor.View.Track;
        if (editor.ActiveTile is not null && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && ImGui.IsWindowHovered())
        {
            if (!(hoveredTile.X >= 0 && hoveredTile.Y >= 0 && hoveredTile.X < track.Size.X &&
                  hoveredTile.Y < track.Size.Y))
                return;
            if (editor.SelectionManager.HasPoint(editor.View.HoveredTile))
            {
                // Fill selection
                editor.UndoManager.Do(new SelectionFillAction(editor.View.Track, editor.SelectionManager.Selection, editor.ActiveTile.Value));
                
            }
            else
            {
                // Flood fill
                var action = FastFloodFill(editor, hoveredTile, editor.ActiveTile.Value);
                if (action is not null)
                    editor.UndoManager.Do(action);
            }
        }

        var min = editor.View.TileToWindow(hoveredTile);
        var max = editor.View.TileToWindow(hoveredTile + new Point(1));
        ImGui.GetWindowDrawList().AddRect(min, max, Color.WhiteSmoke.PackedValue, 0, 0, 2.0f);
    }
    private static TileFillAction? FastFloodFill(TilemapEditor editor, Point pos, byte replacement)
    {
        var map = editor.View.Track.Tilemap.Layout;
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        byte target = map[pos.X, pos.Y];

        // return if nothing to be done
        if (target == replacement) return null;

        HashSet<Point> changedPoints = new HashSet<Point>();
        Stack<Point> stack = new Stack<Point>();

        stack.Push(pos);

        while (stack.Count > 0)
        {
            var (x, y) = stack.Pop();

            // Find left boundary
            int left = x;
            while (left >= 0 && map[left, y] == target)
                left--;

            // Find right boundary
            int right = x;
            while (right < width && map[right, y] == target)
                right++;

            // Fill the scanline and collect changed points
            for (int i = left + 1; i < right; i++)
            {
                map[i, y] = replacement;
                changedPoints.Add(new(i, y));
            }

            // Check rows above and below for unprocessed sections
            void TryPushRow(int nx, int ny)
            {
                if (ny < 0 || ny >= height) return; // Out of bounds
                bool found = false;
                for (int i = left + 1; i < right; i++)
                {
                    if (map[i, ny] == target)
                    {
                        if (!found)
                        {
                            stack.Push(new (i, ny));
                            found = true;
                        }
                    }
                    else
                    {
                        found = false;
                    }
                }
            }

            TryPushRow(left + 1, y - 1); // Check row above
            TryPushRow(left + 1, y + 1); // Check row below
        }

        return new TileFillAction(editor.View.Track, changedPoints, replacement, target);
        
    }

}