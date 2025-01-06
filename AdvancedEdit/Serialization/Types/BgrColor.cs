using Microsoft.Xna.Framework;

namespace AdvancedEdit.Serialization.Types;

public struct BgrColor(ushort raw)
{
    private ushort _raw = raw;

    public int B
    {
        get => (_raw >> 10) & 0x1F;
        set => _raw = (ushort)((_raw & ~0b0_00000_11111_11111) | ((value & 0x1f) << 10));
    }

    public int G
    {
        get => (_raw >> 5) & 0x1F;
        set => _raw = (ushort)((_raw & ~0b0_11111_00000_11111) | ((value & 0x1f) << 5));
    }

    public int R
    {
        get => _raw & 0x1F;
        set => _raw = (ushort)((_raw & ~0b0_11111_11111_00000) | (value & 0x1f));
    }

    public Color ToColor()
    {
        return new Color(R << 3, G << 3, B << 3);
    }
}