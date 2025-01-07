using System;
using System.ComponentModel;
using System.Linq;
using AdvancedEdit.UI.Tools;
using AdvancedEdit.UI.Windows;
using ImGuiNET;
using Microsoft.Xna.Framework;
using SVector2 = System.Numerics.Vector2;

namespace AdvancedEdit.Serialization.Types;

public enum ZoneShape
{
    Rectangle,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
}

[Flags]
public enum SectorFlags
{
    Intersection = 1 << 7,
}

public enum HoverPart
{
    None,
    Target,
    Zone,
    ScaleNW,ScaleNE, ScaleSE, ScaleSW,
    ScaleN, ScaleS, ScaleE, ScaleW,
    ScaleHypot,
}

public class AiSector
{
    public SVector2 Target;

    public ZoneShape Shape;
    public byte Speed; // ranges from 0-3
    public SectorFlags Flags;
    public Zone Zone;
}

public abstract class Zone
{
    protected bool Dragging;

    public abstract void Draw(TilemapWindow view);
    public abstract HoverPart GetHoveredPart(SVector2 mousePosition);
    public abstract void InitDrag(HoverPart part);
    protected static readonly uint FillColor = ImGui.GetColorU32(new System.Numerics.Vector4(0.8f, 0.1f, 0.7f, 0.3f));
    protected static readonly uint HoverColor = ImGui.GetColorU32(new System.Numerics.Vector4(0.8f, 0.1f, 0.7f, 0.5f));
    protected static readonly uint BorderColor = ImGui.GetColorU32(new System.Numerics.Vector4(0.8f, 0.1f, 0.7f, 1.0f));
    public abstract void Drag(HoverPart part);
}

public class RectangleZone(Rectangle rect) : Zone
{
    public Rectangle Rect = rect;
    public SVector2 Min
    {
        get => new(Rect.X, Rect.Y);
        set => Rect = new Rectangle((int)value.X, (int)value.Y,
            Rect.Width + (Rect.X - (int)value.X), Rect.Height + (Rect.Y - (int)value.Y));
    }
    public SVector2 Max
    {
        get => new(Rect.X + Rect.Width, Rect.Y + Rect.Height);
        set => Rect = new Rectangle(Rect.X, Rect.Y, (int)value.X - Rect.X, (int)value.Y - Rect.Y);
    }

    public override void Draw(TilemapWindow view)
    {
        ImGui.GetWindowDrawList().AddRectFilled(
            view.Scale * 8 * Min + view.CursorPosition,
            view.Scale * 8 * Max + view.CursorPosition, 
            FillColor
            );
        ImGui.GetWindowDrawList().AddRect(
            view.Scale * 8 * Min + view.CursorPosition,
            view.Scale * 8 * Max + view.CursorPosition,
            BorderColor,
            0,
            0,
            3f
            );
    }

    public override HoverPart GetHoveredPart(SVector2 mousePosition)
    {
        // TODO: implement scale hovers
        if (mousePosition.X > Min.X && mousePosition.Y > Min.Y && mousePosition.X < Max.X && mousePosition.Y < Max.Y) 
            return HoverPart.Zone;
        return HoverPart.None;
    }

    private SVector2 _lastPosition;

    public override void InitDrag(HoverPart part)
    {
        var mousePosition = ImGui.GetMousePos();
        switch (part)
        {
            case HoverPart.Zone:
                _lastPosition = mousePosition; 
                break;
        }
    }

    public override void Drag(HoverPart part)
    {
        SVector2 mousePosition = ImGui.GetMousePos();
        switch (part)
        {
            case HoverPart.Zone:
                SVector2 delta = mousePosition-_lastPosition;
                Rect.Location += new Point((int)delta.X, (int)delta.Y);
                _lastPosition = mousePosition; 
                break;
        }
    }
}

public class TriangleZone : Zone
{
    public ZoneShape Shape;
    public SVector2 Position;
    public int Size;

    public SVector2[] Points
    {
        get
        {
            SVector2 vertex, armX, armY;
            switch (Shape)
            {
                case ZoneShape.BottomLeft:
                    vertex = Position + new SVector2(2);
                    armX = vertex with { X = vertex.X - (Size + 2) };
                    armY = vertex with { Y = vertex.Y - (Size + 2) };
                    break;
                case ZoneShape.TopLeft:
                    vertex = Position;
                    armX = vertex with { X = vertex.X + (Size + 2) };
                    armY = vertex with { Y = vertex.Y + (Size + 2) };
                    break;
                case ZoneShape.TopRight:
                    vertex = Position + new SVector2(2, 0);
                    armX = vertex with { X = vertex.X - (Size + 2) };
                    armY = vertex with { Y = vertex.Y + (Size + 2) };
                    break;
                case ZoneShape.BottomRight:
                    vertex = Position + new SVector2(0, 2);
                    armX = vertex with { X = vertex.X + (Size + 2) };
                    armY = vertex with { Y = vertex.Y - (Size + 2) };
                    break;
                default: throw new InvalidEnumArgumentException("Invalid Zone Shape");
            }

            return [vertex, armX, armY];
        }
    }

    public TriangleZone(SVector2 position, int size, ZoneShape shape)
    {
        Shape = shape;
        Position = position;
        Size = size;
    }

    public override void Draw(TilemapWindow view)
    {
        var temp = Points.Select(o => o * view.Scale * 8 + view.CursorPosition);
        SVector2[] absPoints = temp.Append(temp.First()).ToArray();
        
        ImGui.GetWindowDrawList().AddConcavePolyFilled(ref absPoints[0], 4, FillColor);
        ImGui.GetWindowDrawList().AddPolyline(ref absPoints[0], 4, BorderColor, 0, 3f);
    }

    private static bool PointInTriangle(SVector2 point, SVector2[] points)
    {
        var min = new SVector2(points.Min(v => v.X), points.Min(v => v.Y));
        var max = new SVector2(points.Max(v => v.X), points.Max(v => v.Y));
        if (PointInRect(point, min, max))
        {
            return PointAboveLine(points[0], points[1], points[2]) == PointAboveLine(point, points[1], points[2]);
        }

        return false;
    }

    private static bool PointAboveLine(SVector2 point, SVector2 p1, SVector2 p2)
    {
        return (point.X - p1.X) * (p2.Y - p1.Y) - (point.Y - p1.Y) * (p2.X - p1.X) > 0;
    }

    private static bool PointInRect(SVector2 point, SVector2 min, SVector2 max)
    {
        return (point.X > min.X && point.X < max.X && point.Y > min.Y && point.Y < max.Y);
    }

    public override HoverPart GetHoveredPart(SVector2 mousePosition)
    {
        if (PointInTriangle(mousePosition, Points))
        {
            return HoverPart.Zone;
        }

        return HoverPart.None;
    }

    private SVector2 _lastPosition;
    public override void InitDrag(HoverPart part)
    {
        var mousePosition = ImGui.GetMousePos();
        switch (part)
        {
            case HoverPart.Zone:
                _lastPosition = mousePosition;
                break;
        }
    }

    public override void Drag(HoverPart part)
    {
        SVector2 mousePosition = ImGui.GetMousePos();
        switch (part)
        {
            case HoverPart.Zone:
                SVector2 delta = mousePosition - _lastPosition;
                Position += new SVector2((int)delta.X, (int)delta.Y);
                _lastPosition = mousePosition;
                break;
        }
    }
}