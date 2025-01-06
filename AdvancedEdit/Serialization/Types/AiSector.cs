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