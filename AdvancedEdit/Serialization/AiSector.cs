using System;
using System.Drawing;
using Microsoft.Xna.Framework;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace AdvancedEdit.Serialization;

public enum ZoneShape
{
    Rectangle,
    TopLeft,
    TopRight,
    BottomRight,
    BottomLeft,
}

public enum ResizeHandle
{
    None,
    TopLeft,
    Top,
    TopRight,
    Right,
    BottomRight,
    Bottom,
    BottomLeft,
    Left
}

[Flags]
public enum SectorFlags
{
    Intersection = 1 << 7,
}

public enum HoverPart
{
    None,
    Zone,
    Target,
}

public class AiSector : IEquatable<AiSector>
{
    #region Properties
    /// <summary>
    /// The precision for AI elements: 2 tiles
    /// </summary>
    public const int Precision = 2;

    private ZoneShape _shape;

    /// <summary>
    /// Gets or sets the zone shape
    /// </summary>
    public ZoneShape Shape
    {
        get => _shape;
        set
        {
            // Scale triangle zones to smallest side of rectangle
            if (_shape == ZoneShape.Rectangle && value != ZoneShape.Rectangle)
            {
                if (_zone.Width > _zone.Height)
                    _zone.Width = _zone.Height;
                else
                    _zone.Height = _zone.Width;
            }
            _shape = value;
        }
    }

    private Rectangle _zone;
    /// <summary>
    /// Gets the area in single tile persision
    /// </summary>
    public Rectangle Zone
    {
        get => _zone;
        set => _zone = value;
    }

    /// <summary>
    /// Gets or sets the target positions
    /// </summary>
    public Point[] Targets;
    /// <summary>
    /// Sets the speed values of the targets. Ranges from 0-3
    /// </summary>
    public int[] Speeds;

    /// <summary>
    /// Determines if the target is treated as an intersection
    /// </summary>
    public bool[] Intersections;

    /// <summary>
    /// Gets and sets the position of the zone
    /// </summary>
    public Point Position
    {
        get => Zone.Location;
        set => _zone.Location = value;
    }

    /// <summary>
    /// Returns the center point of the zone
    /// </summary>
    public Vector2 Center
    {
        get{
            switch (Shape) {
                default:
                case ZoneShape.Rectangle:
                    return Position.ToVector2() + Zone.Size.ToVector2()*(1/2f);
                case ZoneShape.TopLeft:
                    return Position.ToVector2() + Zone.Size.ToVector2()*(1/4f);
                case ZoneShape.BottomRight:
                    return Position.ToVector2() + Zone.Size.ToVector2()*(3/4f);
                case ZoneShape.TopRight:
                    return Position.ToVector2() + new Vector2(Zone.Size.X*(3/4f), Zone.Size.Y*(1/4f));
                case ZoneShape.BottomLeft:
                    return Position.ToVector2() + new Vector2(Zone.Size.X*(1/4f), Zone.Size.Y*(3/4f));
            }
        }
    }
    #endregion

    public AiSector(Point[] targets, ZoneShape shape, Rectangle zone, int[] speeds, bool[] intersections)
    {
        Targets = targets;
        var zoneX = zone.X * Precision;
        var zoneY = zone.Y * Precision;
        
        Speeds = speeds;
        _shape = shape;
        Intersections = intersections;

        if (shape == ZoneShape.Rectangle)
        {
            var zoneWidth = zone.Width * Precision;
            var zoneHeight = zone.Height * Precision;

            _zone = new Rectangle(zoneX, zoneY, zoneWidth + Precision, zoneHeight + Precision);
        }
        else
        {
            var zoneSize = (zone.Width + 1) * Precision;
            switch (Shape)
            {
                case ZoneShape.TopRight:
                    zoneX -= zoneSize - Precision;
                    break;
                case ZoneShape.BottomRight:
                    zoneX -= zoneSize - Precision;
                    zoneY -= zoneSize - Precision;
                    break;
                case ZoneShape.BottomLeft:
                    zoneY -= zoneSize - Precision;
                    break;
            }

            _zone = new Rectangle(zoneX, zoneY, zoneSize, zoneSize);
        }
    }
    public AiSector(Point position)
    {
        const int size = 16;

        var zoneX = Math.Clamp((position.X - size / 2) / Precision * Precision, 0, UInt16.MaxValue);
        var zoneY = Math.Clamp((position.Y - size / 2) / Precision * Precision, 0, UInt16.MaxValue);

        var zone = new Rectangle(zoneX, zoneY, size, size);
        _zone = zone;
        var x = zone.X + zone.Width / Precision;
        var y = zone.Y + zone.Height / Precision;
        var pos = new Point(x, y);
        Targets = [pos,pos,pos];
        Speeds = [0,0,0];
        Intersections = [false, false, false];
    }

    public AiSector(AiSector oldSector)
    {
        Targets = oldSector.Targets;
        _zone = oldSector.Zone;
        Speeds = oldSector.Speeds;
        _shape = oldSector.Shape;
        Intersections = oldSector.Intersections;
    } 

    public HoverPart GetHover(Point point, int targetSet)
    {
        if (point.X > Targets[targetSet].X - 2 && point.X < Targets[targetSet].X + 1 && point.Y > Targets[targetSet].Y - 2 && point.Y < Targets[targetSet].Y + 1)
            return HoverPart.Target;
        if (Shape == ZoneShape.Rectangle)
        {

            return IntersectsWithRectangle(point) ? HoverPart.Zone : HoverPart.None;
        }

        return IntersectsWithTriangle(point)? HoverPart.Zone : HoverPart.None;
    }

    private bool IntersectsWithRectangle(Point point)
    {
        return
            point.X >= _zone.Left &&
            point.X < _zone.Right &&
            point.Y >= _zone.Top &&
            point.Y < _zone.Bottom;
    }

    private bool IntersectsWithTriangle(Point point)
    {
        if (!IntersectsWithRectangle(point))
        {
            return false;
        }

        // Divide precision by 2
        point = new Point((point.X / Precision) * Precision, (point.Y / Precision) * Precision);
        var x = point.X - _zone.X; // X coordinate relative to the triangle top-left corner
        var y = point.Y - _zone.Y; // Y coordinate relative to the triangle top-left corner

        switch (Shape)
        {
            case ZoneShape.TopLeft:
                return x + (y - Precision) <= _zone.Width - Precision;
            case ZoneShape.TopRight:
                return x >= y;
            case ZoneShape.BottomRight:
                return (x + Precision) + y >= _zone.Width - Precision;
            case ZoneShape.BottomLeft:
                return x <= y;
            default:
                throw new InvalidOperationException();
        }
    }

    public ResizeHandle GetResizeHandle(Point point)
    {
        if (Shape == ZoneShape.Rectangle)
        {
            return GetResizeHandleRectangle(point);
        }
        return GetResizeHandleTriangle(point);
    }
    
    private ResizeHandle GetResizeHandleRectangle(Point point)
        {
            ResizeHandle resizeHandle;

            if (point.X > _zone.Left &&
                point.X < _zone.Right - 1 &&
                point.Y > _zone.Top &&
                point.Y < _zone.Bottom - 1)
            {
                resizeHandle = ResizeHandle.None;
            }
            else
            {
                if (point.X == _zone.Left)
                {
                    if (point.Y == _zone.Top)
                    {
                        resizeHandle = ResizeHandle.TopLeft;
                    }
                    else if (point.Y == _zone.Bottom - 1)
                    {
                        resizeHandle = ResizeHandle.BottomLeft;
                    }
                    else
                    {
                        resizeHandle = ResizeHandle.Left;
                    }
                }
                else if (point.X == _zone.Right - 1)
                {
                    if (point.Y == _zone.Top)
                    {
                        resizeHandle = ResizeHandle.TopRight;
                    }
                    else if (point.Y == _zone.Bottom - 1)
                    {
                        resizeHandle = ResizeHandle.BottomRight;
                    }
                    else
                    {
                        resizeHandle = ResizeHandle.Right;
                    }
                }
                else
                {
                    if (point.Y == _zone.Top)
                    {
                        resizeHandle = ResizeHandle.Top;
                    }
                    else
                    {
                        resizeHandle = ResizeHandle.Bottom;
                    }
                }
            }

            return resizeHandle;
        }

    private ResizeHandle GetResizeHandleTriangle(Point point)
        {
            int diagonal;

            switch (Shape)
            {
                case ZoneShape.TopLeft:
                    #region
                    diagonal = (point.X - _zone.X) + (point.Y - _zone.Y);
                    if (diagonal >= _zone.Width - Precision && diagonal <= _zone.Width)
                    {
                        return ResizeHandle.BottomRight;
                    }

                    if (point.X == _zone.Left)
                    {
                        return ResizeHandle.Left;
                    }

                    if (point.Y == _zone.Top)
                    {
                        return ResizeHandle.Top;
                    }
                    #endregion
                    break;

                case ZoneShape.TopRight:
                    #region
                    diagonal = (point.X - _zone.X) - (point.Y - _zone.Y);
                    if (diagonal >= -Precision && diagonal <= 0)
                    {
                        return ResizeHandle.BottomLeft;
                    }

                    if (point.X == _zone.Right - 1)
                    {
                        return ResizeHandle.Right;
                    }

                    if (point.Y == _zone.Top)
                    {
                        return ResizeHandle.Top;
                    }
                    #endregion
                    break;

                case ZoneShape.BottomRight:
                    #region
                    diagonal = (point.X - _zone.X) + (point.Y - _zone.Y);
                    if (diagonal >= _zone.Width - Precision && diagonal <= _zone.Width)
                    {
                        return ResizeHandle.TopLeft;
                    }

                    if (point.X == _zone.Right - 1)
                    {
                        return ResizeHandle.Right;
                    }

                    if (point.Y == _zone.Bottom - 1)
                    {
                        return ResizeHandle.Bottom;
                    }
                    #endregion
                    break;

                case ZoneShape.BottomLeft:
                    #region
                    diagonal = (point.X - _zone.X) - (point.Y - _zone.Y);
                    if (diagonal >= 0 && diagonal <= Precision)
                    {
                        return ResizeHandle.TopRight;
                    }

                    if (point.X == _zone.Left)
                    {
                        return ResizeHandle.Left;
                    }

                    if (point.Y == _zone.Bottom - 1)
                    {
                        return ResizeHandle.Bottom;
                    }
                    #endregion
                    break;

                default:
                    throw new InvalidOperationException();
            }

            return ResizeHandle.None;
        }
    
    public void Resize(ResizeHandle resizeHandle, int x, int y)
        {
            // Halve precision, so that areas are positioned following a 2-tile (16-px) step
            x = (x / Precision) * Precision;
            y = (y / Precision) * Precision;

            if (Shape == ZoneShape.Rectangle)
            {
                ResizeRectangle(resizeHandle, x, y);
            }
            else
            {
                ResizeTriangle(resizeHandle, x, y);
            }
        }

    private void ResizeRectangle(ResizeHandle resizeHandle, int x, int y)
        {
            int areaX;
            int areaY;
            int width;
            int height;

            switch (resizeHandle)
            {
                case ResizeHandle.TopLeft:
                    #region
                    if (x >= _zone.Right)
                    {
                        x = _zone.Right - Precision;
                    }

                    if (y >= _zone.Bottom)
                    {
                        y = _zone.Bottom - Precision;
                    }

                    areaX = x;
                    areaY = y;
                    width = _zone.Right - x;
                    height = _zone.Bottom - y;
                    #endregion
                    break;

                case ResizeHandle.Top:
                    #region
                    if (y >= _zone.Bottom)
                    {
                        y = _zone.Bottom - Precision;
                    }

                    areaX = _zone.X;
                    areaY = y;
                    width = _zone.Width;
                    height = _zone.Bottom - y;
                    #endregion
                    break;

                case ResizeHandle.TopRight:
                    #region
                    if (x < _zone.Left)
                    {
                        x = _zone.Left;
                    }

                    if (y >= _zone.Bottom)
                    {
                        y = _zone.Bottom - Precision;
                    }

                    areaX = _zone.X;
                    areaY = y;
                    width = x - _zone.Left + Precision;
                    height = _zone.Bottom - y;
                    #endregion
                    break;

                case ResizeHandle.Right:
                    #region
                    if (x < _zone.Left)
                    {
                        x = _zone.Left;
                    }

                    areaX = _zone.X;
                    areaY = _zone.Y;
                    width = x - _zone.Left + Precision;
                    height = _zone.Height;
                    #endregion
                    break;

                case ResizeHandle.BottomRight:
                    #region
                    if (x < _zone.Left)
                    {
                        x = _zone.Left;
                    }

                    if (y < _zone.Top)
                    {
                        y = _zone.Top;
                    }

                    areaX = _zone.X;
                    areaY = _zone.Y;
                    width = x - _zone.Left + Precision;
                    height = y - _zone.Top + Precision;
                    #endregion
                    break;

                case ResizeHandle.Bottom:
                    #region
                    if (y < _zone.Top)
                    {
                        y = _zone.Top;
                    }

                    areaX = _zone.X;
                    areaY = _zone.Y;
                    width = _zone.Width;
                    height = y - _zone.Top + Precision;
                    #endregion
                    break;

                case ResizeHandle.BottomLeft:
                    #region
                    if (x >= _zone.Right)
                    {
                        x = _zone.Right - Precision;
                    }

                    if (y < _zone.Top)
                    {
                        y = _zone.Top;
                    }

                    areaX = x;
                    areaY = _zone.Y;
                    width = _zone.Right - x;
                    height = y - _zone.Top + Precision;
                    #endregion
                    break;

                case ResizeHandle.Left:
                    #region
                    if (x >= _zone.Right)
                    {
                        x = _zone.Right - Precision;
                    }

                    areaX = x;
                    areaY = _zone.Y;
                    width = _zone.Right - x;
                    height = _zone.Height;
                    #endregion
                    break;

                default:
                    throw new InvalidOperationException();
            }

            _zone = new Rectangle(areaX, areaY, width, height);
        }

    private void ResizeTriangle(ResizeHandle resizeHandle, int x, int y)
        {
            int areaX;
            int areaY;
            int length;

            switch (resizeHandle)
            {
                case ResizeHandle.TopLeft:
                    #region
                    length = (_zone.Right - x) + (_zone.Bottom - y);

                    #region Validate area length
                    if (length < Precision)
                    {
                        length = Precision;
                    }
                    else
                    {
                        var offBounds = Math.Max(length - _zone.Right, length - _zone.Bottom);
                        if (offBounds > 0)
                        {
                            length -= offBounds;
                        }
                    }
                    #endregion Validate area length

                    areaX = _zone.Right - length;
                    areaY = _zone.Bottom - length;
                    #endregion
                    break;

                case ResizeHandle.Top:
                    #region
                    length = _zone.Bottom - y;

                    if (Shape == ZoneShape.TopLeft)
                    {
                        areaX = _zone.Left;

                        #region Validate area length
                        if (length < Precision)
                        {
                            length = Precision;
                        }
                        else
                        {
                            var offBounds = _zone.X + length - 256;
                            if (offBounds > 0)
                            {
                                length -= offBounds;
                            }
                        }
                        #endregion Validate area length

                        areaY = _zone.Bottom - length;
                    }
                    else //if (Shape == Shape.TriangleTopRight)
                    {
                        #region Validate area length
                        if (length < Precision)
                        {
                            length = Precision;
                        }
                        else
                        {
                            var offBounds = length - _zone.Right;
                            if (offBounds > 0)
                            {
                                length -= offBounds;
                            }
                        }
                        #endregion Validate area length

                        areaX = _zone.Right - length;
                        areaY = _zone.Bottom - length;
                    }
                    #endregion
                    break;

                case ResizeHandle.TopRight:
                    #region
                    length = (x - _zone.X) + (_zone.Bottom - y);
                    areaX = _zone.X;

                    #region Validate area length
                    if (length < Precision)
                    {
                        length = Precision;
                    }
                    else
                    {
                        var offBounds = Math.Max(areaX + length - 256, length - _zone.Bottom);
                        if (offBounds > 0)
                        {
                            length -= offBounds;
                        }
                    }
                    #endregion Validate area length

                    areaY = _zone.Bottom - length;
                    #endregion
                    break;

                case ResizeHandle.Right:
                    #region
                    length = x - _zone.X + Precision;
                    areaX = _zone.X;

                    if (Shape == ZoneShape.TopRight)
                    {
                        areaY = _zone.Y;

                        #region Validate area length
                        if (length < Precision)
                        {
                            length = Precision;
                        }
                        else
                        {
                            var offBounds = Math.Max(areaX + length - 256, areaY + length - 256);
                            if (offBounds > 0)
                            {
                                length -= offBounds;
                            }
                        }
                        #endregion Validate area length
                    }
                    else //if (Shape == Shape.TriangleBottomRight)
                    {
                        #region Validate area length
                        if (length < Precision)
                        {
                            length = Precision;
                        }
                        else
                        {
                            var offBounds = Math.Max(areaX + length - 256, length - _zone.Bottom);
                            if (offBounds > 0)
                            {
                                length -= offBounds;
                            }
                        }
                        #endregion Validate area length

                        areaY = _zone.Bottom - length;
                    }
                    #endregion
                    break;

                case ResizeHandle.BottomRight:
                    #region
                    length = (x - _zone.X) + (y - _zone.Y);
                    areaX = _zone.X;
                    areaY = _zone.Y;

                    #region Validate area length
                    if (length < Precision)
                    {
                        length = Precision;
                    }
                    else
                    {
                        var offBounds = Math.Max(areaX + length - 256, areaY + length - 256);
                        if (offBounds > 0)
                        {
                            length -= offBounds;
                        }
                    }
                    #endregion Validate area length
                    #endregion
                    break;

                case ResizeHandle.Bottom:
                    #region
                    length = y - _zone.Y + Precision;
                    areaY = _zone.Y;

                    if (Shape == ZoneShape.BottomRight)
                    {
                        #region Validate area length
                        if (length < Precision)
                        {
                            length = Precision;
                        }
                        else
                        {
                            var offBounds = Math.Max(length - _zone.Right, areaY + length - 256);
                            if (offBounds > 0)
                            {
                                length -= offBounds;
                            }
                        }
                        #endregion Validate area length

                        areaX = _zone.Right - length;
                    }
                    else //if (Shape == Shape.TriangleBottomLeft)
                    {
                        areaX = _zone.X;

                        #region Validate area length
                        if (length < Precision)
                        {
                            length = Precision;
                        }
                        else
                        {
                            var offBounds = Math.Max(areaX + length - 256, areaY + length - 256);
                            if (offBounds > 0)
                            {
                                length -= offBounds;
                            }
                        }
                        #endregion Validate area length
                    }
                    #endregion
                    break;

                case ResizeHandle.BottomLeft:
                    #region
                    length = (_zone.Right - x) + (y - _zone.Y);
                    areaY = _zone.Y;

                    #region Validate area length
                    if (length < Precision)
                    {
                        length = Precision;
                    }
                    else
                    {
                        var offBounds = Math.Max(length - _zone.Right, areaY + length - 256);
                        if (offBounds > 0)
                        {
                            length -= offBounds;
                        }
                    }
                    #endregion Validate area length

                    areaX = _zone.Right - length;
                    #endregion
                    break;

                case ResizeHandle.Left:
                    #region
                    length = _zone.Right - x;

                    if (Shape == ZoneShape.TopLeft)
                    {
                        areaY = _zone.Y;

                        #region Validate area length
                        if (length < Precision)
                        {
                            length = Precision;
                        }
                        else
                        {
                            var offBounds = Math.Max(length - _zone.Right, areaY + length - 256);
                            if (offBounds > 0)
                            {
                                length -= offBounds;
                            }
                        }
                        #endregion Validate area length
                    }
                    else //if (Shape == Shape.TriangleBottomLeft)
                    {
                        #region Validate area length
                        if (length < Precision)
                        {
                            length = Precision;
                        }
                        else
                        {
                            var offBounds = Math.Max(length - _zone.Right, length - _zone.Bottom);
                            if (offBounds > 0)
                            {
                                length -= offBounds;
                            }
                        }
                        #endregion Validate area length

                        areaY = _zone.Bottom - length;
                    }

                    areaX = _zone.Right - length;
                    #endregion
                    break;

                default:
                    throw new InvalidOperationException();
            }

            _zone = new Rectangle(areaX, areaY, length, length);
        }
    
    public Point[] GetTriangle()
        {
            var points = new Point[_zone.Width + 3];

            int x;
            int y;
            int xStep;
            int yStep;
            Point rightAngle;

            switch (Shape)
            {
                case ZoneShape.TopLeft:
                    x = _zone.X;
                    y = _zone.Y + _zone.Height;
                    xStep = Precision;
                    yStep = -Precision;
                    rightAngle = _zone.Location;
                    break;

                case ZoneShape.TopRight:
                    x = _zone.X + _zone.Width;
                    y = _zone.Y + _zone.Height;
                    xStep = -Precision;
                    yStep = -Precision;
                    rightAngle = new Point(x, _zone.Y);
                    break;

                case ZoneShape.BottomRight:
                    x = _zone.X + _zone.Width;
                    y = _zone.Y;
                    xStep = -Precision;
                    yStep = Precision;
                    rightAngle = new Point(x, _zone.Y + _zone.Height);
                    break;

                case ZoneShape.BottomLeft:
                    x = _zone.X;
                    y = _zone.Y;
                    xStep = Precision;
                    yStep = Precision;
                    rightAngle = new Point(x, _zone.Y + _zone.Height);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var i = 0;
            var even = true;
            while (i < points.Length - Precision)
            {
                points[i++] = new Point(x, y);
                if (even)
                {
                    x += xStep;
                }
                else
                {
                    y += yStep;
                }
                even = !even;
            }

            points[i++] = rightAngle;
            points[i] = points[0];
            
            return points;
        }


    public static bool operator ==(AiSector sector1, AiSector sector2)
    {
        return sector1.Equals(sector2);
    }

    public static bool operator !=(AiSector sector1, AiSector sector2)
    {
        return !sector1.Equals(sector2);
    }

    public bool Equals(AiSector? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return _shape == other._shape && _zone.Equals(other._zone) && Targets.Equals(other.Targets) && Speeds == other.Speeds && Intersections == other.Intersections;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((AiSector)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)_shape, _zone, Targets, Speeds, Intersections);
    }

    public void GetRawInputs(out Point[] targets, out ZoneShape shape, out Rectangle zone, out int[] speeds, out bool[] intersections)
    {
        targets = Targets;
        shape = _shape;
        speeds = Speeds;
        intersections = Intersections;

        var zoneX = _zone.X / Precision;
        var zoneY = _zone.Y / Precision;
        var zoneWidth = (_zone.Width - Precision) / Precision;
        var zoneHeight = (_zone.Height - Precision) / Precision;

        if (_shape == ZoneShape.Rectangle)
        {
            zone = new Rectangle(zoneX, zoneY, zoneWidth, zoneHeight);
        }
        else
        {
            var zoneSize = _zone.Width / Precision - 1;

            switch (_shape)
            {
                case ZoneShape.TopRight:
                    zoneX += zoneSize;
                    break;
                case ZoneShape.BottomRight:
                    zoneX += zoneSize;
                    zoneY += zoneSize;
                    break;
                case ZoneShape.BottomLeft:
                    zoneY += zoneSize;
                    break;
            }

            zone = new Rectangle(zoneX, zoneY, zoneSize, zoneSize);
        }
    }
}
