using System.IO;

namespace AdvancedEdit.Serialization;

public class TrackManager {
    public const int TrackCount = 49;
    public const uint TrackTable = 0x258000;
    public const uint DefintionTable = 0xe7ff0;
    public static Track[] Tracks = new Track[TrackCount];
    public TrackManager(BinaryReader reader){
        for (int i = 0; i < TrackCount; i++) {
            reader.BaseStream.Seek(DefintionTable + 4*i, SeekOrigin.Begin);
            var definitionAddress = reader.ReadUInt32()&0xffffff;
            reader.BaseStream.Seek(TrackTable + 4*i, SeekOrigin.Begin);
            var trackAddress = reader.ReadUInt32() + TrackTable;
            Tracks[i] = new Track(reader, i, definitionAddress, trackAddress);
        }
    }
    
}