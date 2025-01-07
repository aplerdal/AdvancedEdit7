using System;
using Microsoft.Xna.Framework;

namespace AdvancedEdit.Serialization.Types;

public enum SectorShape
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

public class AiSector
{
    public Vector2 Target;
    public Tuple<Vector2, Vector2> Zone; // Uses 2 absolute points so that math is easier
    public SectorShape Shape;
    public byte Speed; // ranges from 0-3
    public SectorFlags Flags;
}

// Maybe there is a better way to do this. Haven't thought of it yet.
public enum HoverPart{
    None,
    Target,
    Zone,
    ScaleNW, ScaleNE, ScaleSE, ScaleSW,
    ScaleN, ScaleS, ScaleE, ScaleW,
    ScaleHypot,
}
public abstract class ZoneShape{
    protected bool dragging;

    public abstract void Draw(ImDrawList drawList);
    public abstract HoverPart GetHoveredPart(Vector2 mousePosition);
    public abstract void InitDrag(HoverPart part);
    public abstract void Drag(HoverPart part);
}
public class RectangleZone : ZoneShape{
    public Rectangle Rect;
    
    public Vector2 Min {
        get => new(Rect.X, Rect.Y);
        set => Rect = new Rectangle(value.X, value.Y, Rect.Width+(Rect.X-value.X), Rect.Height+(Rect.Y-value.Y));
    }
    public Vector2 Max {
        get => new(Rect.X+Rect.Width, Rect.Y+Rect.Height);
        set => Rect = new Rectangle(Rect.X, Rect.Y, value.X-Rect.X, value.Y-Rect.Y);
    }
    public override void Draw(TilemapWindow view) {
        drawList.AddRect(view.Scale*8*Min, view.Scale*8*Max,);
        drawList.AddRectFilled(view.Scale*8*Min, view.Scale*8*Max,);
    }
    public override HoverPart GetHoveredPart(Vector2 mousePosition) {
        if (
            mousePosition.X > Min.X && mousePosition.Y > Min.Y &&
            mousePosition.X < Max.X && mousePosition.Y < Max.Y
        ) return HoverPart.Zone;
        //TODO: Implement scale hovering
    }
    private Vector2 _lastPosition;
    public override void InitDrag(HoverPart part) {
        var mousePosition = ImGui.GetMousePos();
        switch (part){
            case HoverPart.Zone:
            {
                _lastPosition = mousePosition;
            } break;
        }
    }
    public override void Drag(HoverPart part) {
        var mousePosition = ImGui.GetMousePos();
        switch (part){
            case HoverPart.Zone:
            {
                var delta = mousePosition-_lastPosition;
                //TODO: fix zone shrinking when pushed against border
                Min=Vector2.Clamp(Min+detla, Vector2.Zero, Vector2(1024));
                Max=Vector2.Clamp(Max+delta, Vector2.Zero, Vector2(1024));
                
                _lastPosition = mousePosition
            } break;
        }
    }
}