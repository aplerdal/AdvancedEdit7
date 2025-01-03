using System.IO;

namespace AdvancedEdit.Serialization.Types;

public struct AiHeader : ISerializable
{
    public byte Count { get; set; }
    public ushort ZonesOffset { get; set; }
    public ushort TargetsOffset { get; set; }

    public void Serialize(BinaryWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public void Deserialize(BinaryReader reader)
    {
        Count = reader.ReadByte();
        ZonesOffset = reader.ReadUInt16();
        TargetsOffset = reader.ReadUInt16();
    }
}

public struct AiTarget : ISerializable
{
    public ushort X { get; set; }
    public ushort Y { get; set; }
    private byte _speedFlags;
    public int Speed => _speedFlags & 0x0F;
    public int Flags => _speedFlags >> 0x10;

    public void Serialize(BinaryWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public void Deserialize(BinaryReader reader)
    {
        X = reader.ReadUInt16();
        Y = reader.ReadUInt16();
        _speedFlags = reader.ReadByte();
        reader.BaseStream.Seek(3, SeekOrigin.Current);
    }
}

public struct AiZone : ISerializable
{
    public byte Shape { get; set; }
    private ushort _halfX;
    public uint X
    {
        get => (uint)(_halfX * 2);
        set => _halfX = (ushort)(value/2);
    }
    
    private ushort _halfY;
    public uint Y
    {
        get => (uint)(_halfY * 2);
        set => _halfY = (ushort)(value / 2);
    }
    
    private ushort _halfWidth;
    public uint Width
    {
        get => (uint)(_halfWidth * 2);
        set => _halfWidth = (ushort)(value / 2);
    }
    
    private ushort _halfHeight;

    public uint Height
    {
        get => (uint)(_halfHeight * 2);
        set => _halfHeight = (ushort)(value / 2);
    }
    public void Serialize(BinaryWriter writer)
    {
        throw new System.NotImplementedException();
    }

    public void Deserialize(BinaryReader reader)
    {
        Shape = reader.ReadByte();
        _halfX = reader.ReadUInt16();
        _halfY = reader.ReadUInt16();
        _halfWidth = reader.ReadUInt16();
        _halfHeight = reader.ReadUInt16();
    }
}

public enum ZoneShape : byte
{
    Rectangle = 0,
    TopLeft = 1,
    TopRight = 2,
    BottomRight = 3,
    BottomLeft = 4,
}