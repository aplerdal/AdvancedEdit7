using Microsoft.Xna.Framework;

namespace AdvancedEdit.Serialization.Types;

public class GameObject
{
    public byte Id; // Maybe rename to "Type"?
    public GameGfx Gfx;
    public Vector2 Position;
    public byte Zone;
}