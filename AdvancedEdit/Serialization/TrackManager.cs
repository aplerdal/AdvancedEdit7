using System.IO;

namespace AdvancedEdit.Serialization;

public class TrackManager
{
    public string RomPath;
    public const int TrackCount = 49;
    public const uint TrackTable = 0x258000u;
    public const uint DefintionTable = 0xe7534u;
    public static Track[] Tracks = new Track[TrackCount];
    public TrackManager(BinaryReader reader){
        for (int i = 0; i < TrackCount; i++) {
            uint definitionAddress = (uint)(DefintionTable + 4 * i);
            reader.BaseStream.Seek(TrackTable + 4*i, SeekOrigin.Begin);
            var trackAddress = reader.ReadUInt32() + TrackTable;
            Tracks[i] = new Track(reader, i, definitionAddress, trackAddress);
        }
    }

    public void Save(BinaryWriter writer)
    {
        uint pos = 4*TrackCount + 0x10;
        for (int i = 0; i < TrackCount; i++)
        {
            uint definitionAddress = (uint)(DefintionTable + 4 * i);
            var last = writer.BaseStream.Position;
            writer.BaseStream.Seek(TrackTable + 4*i, SeekOrigin.Begin);
            writer.Write(pos);
            writer.BaseStream.Seek(last, SeekOrigin.Begin);
            pos += Tracks[i].Write(writer, definitionAddress, TrackTable + pos);
        }
    }
    
}