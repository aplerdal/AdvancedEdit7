using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdvancedEdit.UI.Tools;
using AdvancedEdit.UI.Windows;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;

namespace AdvancedEdit.UI.Components;

public enum SelectionMode
{
    Replace,
    Add,
    Subtract,
    Invert
}

public class SelectionManager
{
    public HashSet<Point> Selection => _selection;
    private HashSet<Point> _selection = new();
    private HashSet<(Point, Point)> _edges = new();
    private List<Rectangle> _fill = new();
    
    public void Update(TrackView view)
    {
        //var min = view.TileToWindow(view.HoveredTile);
        //var max = view.TileToWindow(view.HoveredTile + new Point(1));
        //ImGui.GetWindowDrawList().AddRect(min, max, Color.WhiteSmoke.PackedValue, 0, 0, 6.0f);
        if (_edges.Count > 0)
        {
            var color = Color.White;
            var dl = ImGui.GetWindowDrawList();
            var outline = 3f;
            
            foreach (var rect in _fill)
            {
                dl.AddRectFilled(
                    view.TileToWindow(rect.Location),
                    view.TileToWindow(rect.Location + rect.Size),
                    new Color(255, 255, 255, 64).PackedValue);
            }
            
            foreach (var segment in _edges)
            {
                dl.AddLine(view.TileToWindow(segment.Item1), view.TileToWindow(segment.Item2), color.PackedValue ,
                    outline);
            }
        }

        // Clear selection on escape
        if (ImGui.IsKeyPressed(ImGuiKey.Escape))
        {
            Clear();
        }
    }

    private static List<Rectangle> GetFillList(HashSet<Point> cells)
    {
        var rectangles = new List<Rectangle>();
        var remainingCells = new HashSet<Point>(cells);

        while (remainingCells.Count > 0)
        {
            // Get any remaining cell as the starting point
            Point start = default;
            foreach (var cell in remainingCells)
            {
                start = cell;
                break;
            }

            int x = start.X, y = start.Y, width = 1, height = 1;

            // Expand downward first (preserving row integrity)
            while (true)
            {
                bool canExpand = true;
                for (int dx = 0; dx < width; dx++)
                {
                    if (!remainingCells.Contains(new Point(x + dx, y + height)))
                    {
                        canExpand = false;
                        break;
                    }
                }

                if (!canExpand) break;
                height++;
            }

            // Expand right while maintaining full height
            while (true)
            {
                bool canExpand = true;
                for (int dy = 0; dy < height; dy++)
                {
                    if (!remainingCells.Contains(new Point(x + width, y + dy)))
                    {
                        canExpand = false;
                        break;
                    }
                }

                if (!canExpand) break;
                width++;
            }

            // Mark all cells within this rectangle as processed
            for (int dx = 0; dx < width; dx++)
            for (int dy = 0; dy < height; dy++)
                remainingCells.Remove(new Point(x + dx, y + dy));

            // Store the rectangle
            rectangles.Add(new Rectangle(x, y, width, height));
        }

        return rectangles;
    }
    
    public bool HasPoint(Point point) => _selection.Contains(point);
    private void RegenEdges()
    {
        _edges.Clear();
        void CheckSegment((Point, Point) segment)
        {
            if (_edges.Contains(segment))
                _edges.Remove(segment);
            else if (_edges.Contains((segment.Item2, segment.Item1)))
                _edges.Remove((segment.Item2, segment.Item1));
            else
                _edges.Add(segment);

        }
        
        foreach (var point in _selection)
        {
            CheckSegment((point, new Point(point.X+1,point.Y)));
            CheckSegment((point, new Point(point.X, point.Y+1)));
            CheckSegment((new Point(point.X + 1, point.Y), new Point(point.X + 1, point.Y + 1)));
            CheckSegment((new Point(point.X, point.Y + 1), new Point(point.X + 1, point.Y + 1)));
        }

        _fill = GetFillList(_selection);
    }

    /// <summary>
    /// Clear Selection
    /// </summary>
    public void Clear()
    {
        _selection.Clear();
        RegenEdges();
    }
    /// <summary>
    /// Select rectangle area
    /// </summary>
    /// <param name="area">Selection area</param>
    /// <param name="mode">Selection mode</param>
    public void SelectRect(Rectangle area, SelectionMode mode = SelectionMode.Replace)
    {
        Debug.Assert(area.Top < area.Bottom && area.Left < area.Right);
        if (mode == SelectionMode.Replace)
        {
            _selection.Clear();
            mode = SelectionMode.Add;
        }
        for (int y = area.Top; y < area.Bottom; y++)
        for (int x = area.Left; x < area.Right; x++)
            SelectTile(new(x, y), mode);

        RegenEdges();
    }
    private void SelectTile(Point tile, SelectionMode mode = SelectionMode.Replace)
    {
        switch (mode)
        {
            case SelectionMode.Replace:
                _selection.Clear();
                goto case SelectionMode.Add;
            case SelectionMode.Add:
                _selection.Add(tile);
                break;
            case SelectionMode.Invert:
                if (_selection.Contains(tile))
                    _selection.Remove(tile);
                else
                    _selection.Add(tile);
                break;
            case SelectionMode.Subtract:
                _selection.Remove(tile);
                break;
        }
    }
}