using System;
using Microsoft.Xna.Framework;

namespace AdvancedEdit.Serialization;

public class GameObject(byte id, Point position, byte zone) : ICloneable
{
    public byte Id = id; // Maybe rename to "Type"?
    public Point Position = position;
    public byte Zone = zone;
    
    public object Clone() => new GameObject(Id, Position, Zone);
}