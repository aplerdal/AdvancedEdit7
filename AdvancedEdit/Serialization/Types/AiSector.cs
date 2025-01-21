using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using AdvancedEdit.UI.Tools;
using AdvancedEdit.UI.Windows;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace AdvancedEdit.Serialization.Types;

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

public class AiSector
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
    /// Gets the area
    /// </summary>
    public Rectangle Zone
    {
        get => _zone;
        set => _zone = value;
    }

    private Point _target;
    /// <summary>
    /// Gets or sets the target
    /// </summary>
    public Point Target
    {
        get => _target;
        set => _target = Vector2.Clamp(value.ToVector2(), Vector2.Zero, new Vector2(UInt16.MaxValue)).ToPoint();
    }

    private int _speed;
    /// <summary>
    /// Sets the speed value of the target. Ranges from 0-3
    /// </summary>
    public int Speed
    {
        get => _speed;
        set => _speed = Math.Clamp(value, 0, 3);
    }

    /// <summary>
    /// Determines if the zone is treated as an intersection
    /// </summary>
    public bool Intersection { get; set; }

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

    public AiSector(Point target, ZoneShape shape, Rectangle zone, int speed, bool intersection)
    {
        _target = target;
        var zoneX = zone.X * Precision;
        var zoneY = zone.Y * Precision;
        
        _speed = speed;
        _shape = shape;
        Intersection = intersection;

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
        _target = new Point(x, y);
        _speed = 0;
    }

    public AiSector(AiSector oldSector)
    {
        _target = oldSector.Target;
        _zone = oldSector.Zone;
        _speed = oldSector.Speed;
        _shape = oldSector.Shape;
        Intersection = oldSector.Intersection;
    } 

    public HoverPart GetHover(Point point)
    {
        if (point.X > _target.X - 2 && point.X < _target.X + 1 && point.Y > _target.Y - 2 && point.Y < _target.Y + 1)
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
        return (sector1.Shape == sector2.Shape) && 
               (sector1.Zone == sector2.Zone) &&
               (sector1.Target == sector2.Target) &&
               (sector1.Speed == sector2.Speed) && 
               (sector1.Intersection == sector2.Intersection);
    }

    public static bool operator !=(AiSector sector1, AiSector sector2)
    {
        return !((sector1.Shape == sector2.Shape) &&
               (sector1.Zone == sector2.Zone) &&
               (sector1.Target == sector2.Target) &&
               (sector1.Speed == sector2.Speed) &&
               (sector1.Intersection == sector2.Intersection));
    }
}
