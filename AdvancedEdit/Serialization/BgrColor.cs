using Microsoft.Xna.Framework;

namespace AdvancedEdit.Serialization;

public struct BgrColor
{
    public ushort Raw;

    public BgrColor(ushort raw)
    {
        Raw = raw;
    }

    public BgrColor(Color color)
    {
        Raw = 0;
        R = color.R/8;
        G = color.G/8;
        B = color.B/8;
    }

    public int B
    {
        get => (Raw >> 10) & 0x1F;
        set => Raw = (ushort)((Raw & 0b0_00000_11111_11111) | ((value & 0x1f) << 10));
    }

    public int G
    {
        get => (Raw >> 5) & 0x1F;
        set => Raw = (ushort)((Raw & 0b0_11111_00000_11111) | ((value & 0x1f) << 5));
    }

    public int R
    {
        get => Raw & 0x1F;
        set => Raw = (ushort)((Raw & 0b0_11111_11111_00000) | (value & 0x1f));
    }

    public Color ToColor()
    {
        return new Color(R << 3, G << 3, B << 3);
    }
}