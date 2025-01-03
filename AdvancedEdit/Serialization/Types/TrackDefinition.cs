using System;
using System.IO;

namespace AdvancedEdit.Serialization.Types;

public struct TrackDefinition : ISerializable
{
    public const uint Address = 0xE7534;
    public uint TrackId { get; set; }
    public uint BackgroundId { get; set; }
    public uint BackgroundBehavior { get; set; }
    public uint Animation { get; set; }
    public uint Material { get; set; }
    public uint TurnInstructions { get; set; }
    public uint MusicId { get; set; }
    public uint TargetSetTable { get; set; }
    public uint Unknown { get; set; }
    public uint TrackCoverGraphics { get; set; }
    public uint TrackCoverPalette { get; set; }
    public uint LockedPalette { get; set; }
    public uint TrackNameGfx { get; set; }
    public uint LapCount { get; set; }
    
    public void Serialize(BinaryWriter writer)
    {
        writer.Write(TrackId);
        writer.Write(BackgroundId);
        writer.Write(BackgroundBehavior);
        writer.Write(Animation);
        writer.Write(Material);
        writer.Write(TurnInstructions);
        writer.Write(MusicId);
        writer.Write(TargetSetTable);
        writer.Write(Unknown);
        writer.Write(TrackCoverGraphics);
        writer.Write(TrackCoverPalette);
        writer.Write(LockedPalette);
        writer.Write(TrackNameGfx);
        writer.Write(LapCount);
    }

    public void Deserialize(BinaryReader reader)
    {
        TrackId = reader.ReadUInt32();
        BackgroundId = reader.ReadUInt32();
        BackgroundBehavior = reader.ReadUInt32();
        Animation = reader.ReadUInt32();
        Material = reader.ReadUInt32();
        TurnInstructions = reader.ReadUInt32();
        MusicId = reader.ReadUInt32();
        TargetSetTable = reader.ReadUInt32();
        Unknown = reader.ReadUInt32();
        TrackCoverGraphics = reader.ReadUInt32();
        TrackCoverPalette = reader.ReadUInt32();
        LockedPalette = reader.ReadUInt32();
        TrackNameGfx = reader.ReadUInt32();
        LapCount = reader.ReadUInt32();
    }
}