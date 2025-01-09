using System;
using System.Collections.Generic;
using System.IO;
using AdvancedEdit.Compression;
using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;

namespace AdvancedEdit.Serialization.Types;

[Flags]
public enum TrackFlags
{
    SplitTileset = 1,
    SplitLayout = 2,
    SplitObjects = 4,
}
public class Track
{
    public int Id;
    // From track header
    public Tilemap Tilemap;
    public GameGfx Tileset; // TODO: Refactor with getters and setters to automatically fetch the correct tileset using the lookback stuff
    public List<AiSector> AiSectors = new List<AiSector>(); // Linked list so rearranging and removing is faster
    /// <summary>
    /// Track size in tiles
    /// </summary>
    public Point Size;
    public GameGfx Minimap;
    public TrackFlags Flags;
    public bool IsTilesetCompressed;
    public uint ReusedTileset;
    public uint ReusedGameObject;

    // From track definition
    public GameGfx Name;
    public GameGfx Cover;
    public uint Laps; // Artificially clamp from 1-5 (beyond that it likely breaks). Auto apply patch to all ROMs.

    // Store data we do not keep track of that cannot be determined.
    private byte _magic; // Might be okay to change to const?
    private uint _trackRoutine;

    private uint _trackId;
    private uint _backgroundId; // Might want to allow changing this
    private uint _backgroundBehavior;
    private uint _animation;
    private uint _material;
    private uint _turnInstructions;
    private uint _musicId; // and this as well
    private uint _targetSetTable;
    private uint _tdUnknown; // The "unk" value from the track definition

    /// <summary>
    /// Reads a track object from a file
    /// </summary>
    /// <param name="reader">a BinaryReader</param>
    /// <param name="id">The number of the track</param>
    /// <param name="definition">The track definition address</param>
    /// <param name="header">The track header address</param>
    public Track(BinaryReader reader, int id, uint definition, uint header)
    {
        Id = id;
        #region Definition
        reader.BaseStream.Seek(definition, SeekOrigin.Begin);
        _trackId = reader.ReadUInt32();
        _backgroundId = reader.ReadUInt32();
        _backgroundBehavior = reader.ReadUInt32();
        _animation = reader.ReadUInt32();
        _material = reader.ReadUInt32();
        _turnInstructions = reader.ReadUInt32();
        _musicId = reader.ReadUInt32();
        _targetSetTable = reader.ReadUInt32();
        _tdUnknown = reader.ReadUInt32();
        
        //TODO: Load track gfx
        reader.ReadUInt32();
        reader.ReadUInt32();
        reader.ReadUInt32();
        reader.ReadUInt32();
        
        Laps = reader.ReadUInt32();

        #endregion
        
        #region Header
        reader.BaseStream.Seek(header, SeekOrigin.Begin);
        _magic = reader.ReadByte();
        IsTilesetCompressed = reader.ReadBoolean();
        reader.BaseStream.Seek(1, SeekOrigin.Current);
        Flags = (TrackFlags)reader.ReadByte();
        Size = new Point(reader.ReadByte()*128, reader.ReadByte()*128);
        reader.BaseStream.Seek(42, SeekOrigin.Current);
        ReusedTileset = reader.ReadUInt32();
        reader.BaseStream.Seek(12, SeekOrigin.Current);
        var layoutAddress = header+reader.ReadUInt32();
        reader.BaseStream.Seek(60, SeekOrigin.Current);
        var tilesetAddress = header + reader.ReadUInt32();
        var paletteAddress = header + reader.ReadUInt32();
        var behaviorAddress = header + reader.ReadUInt32();
        var objectsAddress = header + reader.ReadUInt32();
        var overlayAddress = header + reader.ReadUInt32();
        var itemBoxAddress = header + reader.ReadUInt32();
        var finishAddress = header + reader.ReadUInt32();
        var unk1Address = header + reader.ReadUInt32();
        reader.BaseStream.Seek(32, SeekOrigin.Current);
        _trackRoutine = reader.ReadUInt32();
        var minimapAddress = header + reader.ReadUInt32();
        reader.ReadUInt32(); // unk battle stuff
        var aiAddress = header + reader.ReadUInt32();
        reader.BaseStream.Seek(20, SeekOrigin.Current);
        var objectGfxAddress = header + reader.ReadUInt32();
        var objectPaletteAddress = header + reader.ReadUInt32();
        var reusedObject = reader.ReadUInt32();
        #endregion
        
        #region Load Tileset
        
        Color[] tilePalette = new Color[64];
        reader.BaseStream.Seek(paletteAddress, SeekOrigin.Begin);
        for (int i = 0; i < 64; i++)
            tilePalette[i] = new BgrColor(reader.ReadUInt16()).ToColor();
        
        if (ReusedTileset != 0)
        {
            //TODO: tileset lookback
            Tileset = null;
        } else if (Flags.HasFlag(TrackFlags.SplitTileset))
        {
            long pos = tilesetAddress;
            byte[] indicies = new byte[4096*4];
            for (int i = 0; i < 4; i++)
            {
                reader.BaseStream.Seek(pos, SeekOrigin.Begin);
                var offset = reader.ReadUInt16();
                pos += 2;
                if (offset != 0)
                {
                    var partAddress = tilesetAddress + offset;
                    reader.BaseStream.Seek(partAddress, SeekOrigin.Begin);
                    byte[] data = Lz10.Decompress(reader, 0x4000000-partAddress).ToArray();
                    Array.Copy(data, 0, indicies, i*4096, 4096);
                }
            }
            Tileset = new GameGfx(new(256*8, 8), indicies, tilePalette);
        }
        else
        {
            reader.BaseStream.Seek(tilesetAddress, SeekOrigin.Begin);
            byte[] data = Lz10.Decompress(reader, 0x4000000 - tilesetAddress).ToArray();
            Tileset = new GameGfx(new(256 * 8, 8), data, tilePalette);
        }
        #endregion
        
        #region Load Layout
        if (Flags.HasFlag(TrackFlags.SplitLayout))
        {
            uint pos = layoutAddress;
            byte[] indicies = new byte[4096*16];
            for (int i = 0; i < 16; i++)
            {
                reader.BaseStream.Seek(pos, SeekOrigin.Begin);
                var offset = reader.ReadUInt16();
                pos += 2;
                if (offset != 0)
                {
                    var partAddress = layoutAddress + offset;
                    
                    reader.BaseStream.Seek(partAddress, SeekOrigin.Begin);
                    var data = Lz10.Decompress(reader, 0x4000000 - partAddress).ToArray();
                    Array.Copy(data, 0, indicies, i*4096, 4096);
                }
            }
            
            if (Tileset is not null)
                Tilemap = new Tilemap(Size, Tileset.Texture, indicies);
        }
        else
        {
            reader.BaseStream.Seek(layoutAddress, SeekOrigin.Begin);
            var data = Lz10.Decompress(reader, 0x4000000 - layoutAddress).ToArray();
            if (Tileset is not null)
                Tilemap = new Tilemap(Size, Tileset.Texture, data);
        }
        #endregion

        #region Load AI

        reader.BaseStream.Seek(aiAddress, SeekOrigin.Begin);
        byte sectorCount = reader.ReadByte();
        uint zonesAddress = aiAddress + reader.ReadUInt16();
        uint targetsAddress = aiAddress + reader.ReadUInt16();
        
        const int targetSize = 2 + 2 + 1 + 3;
        const int zoneSize = 1 + 2 + 2 + 2 + 2 + 3;
        for (int i = 0; i < sectorCount; i++)
        {
            reader.BaseStream.Seek(targetsAddress + i*targetSize, SeekOrigin.Begin);
            var target = new Point(reader.ReadUInt16(), reader.ReadUInt16());
            byte speedFlagUnion = reader.ReadByte();
            var speed = (speedFlagUnion & 0x3);
            var intersection = (speedFlagUnion & 0x80) == 0x80;

            reader.BaseStream.Seek(zonesAddress + i * zoneSize, SeekOrigin.Begin);
            var shape = (ZoneShape)reader.ReadByte();
            var rect = new Rectangle(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16());

            AiSectors.Add(new AiSector(target, shape, rect, speed, intersection));
        }
        #endregion
    }
}