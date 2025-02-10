using System;
using System.Collections.Generic;
using System.IO;
using AdvancedEdit.Compression;
using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;

namespace AdvancedEdit.Serialization;

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
    
    private GameGfx? _tileset;
    public GameGfx Tileset
    {
        get
        {
            if (_tileset is not null) return _tileset;
            if (ReusedTileset == 0) throw new Exception("Tileset null, not reused.");
            return TrackManager.Tracks[Math.Clamp(Id - (256 - ReusedTileset), 0, Id-1)].Tileset; //TODO: Proper reused tilset offsets
        }
        set => _tileset = value;
    }
    public List<AiSector> AiSectors = new List<AiSector>(); // Linked list so rearranging and removing is faster

    private GameGfx? _objectGfx;
    public GameGfx ObjectGfx
    {
        get
        {
            if (_objectGfx is not null) return _objectGfx;
            if (ReusedGameObjects == 0) throw new Exception("Object gfx null, not reused.");
            return TrackManager.Tracks[Math.Clamp(Id - (256 - ReusedTileset), 0, Id - 1)].ObjectGfx;
        }
        set => _objectGfx = value;
    }
    
    /// <summary>
    /// Track size in tiles
    /// </summary>
    public Point Size;
    public TrackFlags Flags;
    
    public bool IsTilesetCompressed;
    public uint ReusedTileset;
    public uint ReusedGameObjects;
    public byte[] Behaviors;
    public List<GameObject> Objects = new List<GameObject>();
    public List<GameObject> ItemBoxes = new List<GameObject>();
    public List<Point> Overlay = new List<Point>();

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
    /// <param name="headerAddress">The track header address</param>
    public Track(BinaryReader reader, int id, uint definition, uint headerAddress)
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
        reader.BaseStream.Seek(headerAddress, SeekOrigin.Begin);
        _magic = reader.ReadByte();
        IsTilesetCompressed = reader.ReadBoolean();
        reader.BaseStream.Seek(1, SeekOrigin.Current);
        Flags = (TrackFlags)reader.ReadByte();
        Size = new Point(reader.ReadByte()*128, reader.ReadByte()*128);
        reader.BaseStream.Seek(42, SeekOrigin.Current);
        ReusedTileset = reader.ReadUInt32();
        reader.BaseStream.Seek(12, SeekOrigin.Current);
        var layoutAddress = headerAddress+reader.ReadUInt32();
        reader.BaseStream.Seek(60, SeekOrigin.Current);
        var tilesetAddress = headerAddress + reader.ReadUInt32();
        var paletteAddress = headerAddress + reader.ReadUInt32();
        var behaviorAddress = headerAddress + reader.ReadUInt32();
        var objectsAddress = headerAddress + reader.ReadUInt32();
        var overlayAddress = headerAddress + reader.ReadUInt32();
        var itemBoxAddress = headerAddress + reader.ReadUInt32();
        var finishAddress = headerAddress + reader.ReadUInt32();
        var unk1Address = headerAddress + reader.ReadUInt32();
        reader.BaseStream.Seek(32, SeekOrigin.Current);
        _trackRoutine = reader.ReadUInt32();
        var minimapAddress = headerAddress + reader.ReadUInt32();
        reader.ReadUInt32(); // unk battle stuff
        var aiAddress = headerAddress + reader.ReadUInt32();
        reader.BaseStream.Seek(20, SeekOrigin.Current);
        var objectGfxAddress = headerAddress + reader.ReadUInt32();
        var objectPaletteAddress = headerAddress + reader.ReadUInt32();
        var reusedObject = reader.ReadUInt32();
        #endregion
        
        #region Load Tileset
        
        Color[] tilePalette = new Color[64];
        reader.BaseStream.Seek(paletteAddress, SeekOrigin.Begin);
        for (int i = 0; i < 64; i++)
            tilePalette[i] = new BgrColor(reader.ReadUInt16()).ToColor();
        
        if (ReusedTileset != 0)
        {
            _tileset = null;
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
                    byte[] data = Lz10.Decompress(reader).ToArray();
                    Array.Copy(data, 0, indicies, i*4096, 4096);
                }
            }
            Tileset = new GameGfx(indicies, tilePalette);
        }
        else
        {
            reader.BaseStream.Seek(tilesetAddress, SeekOrigin.Begin);
            byte[] data = Lz10.Decompress(reader).ToArray();
            Tileset = new GameGfx(data, tilePalette);
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
                    var data = Lz10.Decompress(reader).ToArray();
                    Array.Copy(data, 0, indicies, i*4096, 4096);
                }
            }
            
            Tilemap = new Tilemap(Size, Tileset.Texture, indicies);
        }
        else
        {
            reader.BaseStream.Seek(layoutAddress, SeekOrigin.Begin);
            var data = Lz10.Decompress(reader).ToArray();
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

        #region Load Object GFX

        Color[] objectPalette = new Color[24];
        if (paletteAddress == headerAddress)
        {
            reader.BaseStream.Seek(objectPaletteAddress, SeekOrigin.Begin);
            for (int i = 0; i < 24; i++)
                objectPalette[i] = new BgrColor(reader.ReadUInt16()).ToColor();
        }
        if (ReusedGameObjects != 0)
        {
            _objectGfx = null;
        }
        else
        {
            if (objectGfxAddress != headerAddress)
            {
                if (!Flags.HasFlag(TrackFlags.SplitObjects))
                {
                    reader.BaseStream.Seek(objectGfxAddress, SeekOrigin.Begin);
                    var data = Lz10.Decompress(reader).ToArray();
                    ObjectGfx = new GameGfx(data, objectPalette);
                }
                else
                {
                    long pos = objectGfxAddress;
                    byte[] indicies = new byte[1024 * 2];
                    for (int i = 0; i < 2; i++)
                    {
                        reader.BaseStream.Seek(pos, SeekOrigin.Begin);
                        var offset = reader.ReadUInt16();
                        pos += 2;
                        if (offset != 0)
                        {
                            var partAddress = objectGfxAddress + offset;
                            reader.BaseStream.Seek(partAddress, SeekOrigin.Begin);
                            byte[] data = Lz10.Decompress(reader).ToArray();
                            Array.Copy(data, 0, indicies, i * 1024, 1024);
                        }
                    }
                    ObjectGfx = new GameGfx(indicies, objectPalette);
                }
            }
        }

        #endregion

        #region Load Objects

        if (objectsAddress != headerAddress)
        {
            reader.BaseStream.Seek(objectsAddress, SeekOrigin.Begin);
            while (true)
            {
                var objId = reader.ReadByte();
                if (objId == 0) return;
                Objects.Add(new(objId, new(reader.ReadByte(), reader.ReadByte()), reader.ReadByte()));
            }
        }

        #endregion

        #region Load Item Boxes
        // Haven't checked how these work at all. I guess I will assume they work like objects until it crashes
        if (itemBoxAddress != headerAddress)
        {
            reader.BaseStream.Seek(itemBoxAddress, SeekOrigin.Begin);
            while (true)
            {
                var boxId = reader.ReadByte();
                if (boxId == 0) return;
                ItemBoxes.Add(new(boxId, new(reader.ReadByte(), reader.ReadByte()), reader.ReadByte()));
            }
        }
        #endregion

        #region Load Behaviors

        reader.BaseStream.Seek(itemBoxAddress, SeekOrigin.Begin);
        Behaviors = reader.ReadBytes(256);

        #endregion

        #region Load Overlay

        if (overlayAddress != headerAddress)
        {
            reader.BaseStream.Seek(overlayAddress, SeekOrigin.Begin);
            while (true)
            {
                if (reader.ReadByte() == 0) return;
                Overlay.Add(new Point(reader.ReadByte(), reader.ReadByte()));
                reader.ReadByte();
            }
        }

        #endregion
    }

    public void Write(BinaryWriter writer, uint definition, uint header) {
        writer.BaseStream.Seek(definition, SeekOrigin.Begin);
        writer.Write(_trackId);
        writer.Write(_backgroundId);
        writer.Write(_backgroundBehavior);
        writer.Write(_animation);
        writer.Write(_material);
        writer.Write(_turnInstructions);
        writer.Write(_musicId);
        writer.Write(_targetSetTable);
        writer.Write(_tdUnknown);
        // TODO: Read and write these correctly.
        writer.Write(0u);
        writer.Write(0u);
        writer.Write(0u);
        writer.Write(0u);
        
        const int headerSize = 0x100;
        uint pos = headerSize;

        #region Layout
        uint layoutAddress;
        byte[] tmp = new byte[Tilemap.Layout.GetLength(0) * Tilemap.Layout.GetLength(1)];
        Buffer.BlockCopy(Tilemap.Layout, 0, tmp, 0, tmp.Length * sizeof(byte));
        if (tmp.Length <= 4096) {
            Flags &= ~TrackFlags.SplitLayout;
            byte[] compressedLayout = Lz10.Compress(tmp);
            layoutAddress = pos;
            writer.BaseStream.Seek(definition + pos, SeekOrigin.Begin);
            writer.Write(compressedLayout);
            pos += (uint)compressedLayout.Length;
        } else {
            Flags |= TrackFlags.SplitLayout;
            byte[][] parts = new byte[tmp.Length/4096][];
            for (int i = 0; i < parts.Length; i++) {
                parts[i] = Lz10.Compress(tmp[(i*4096)..((i+1)*4096)]);
            }
            writer.BaseStream.Seek(definition + pos, SeekOrigin.Begin);
            ushort localPos = 0x20;
            for (int i = 0; i < 16; i++) {
                if (i < parts.Length) {
                    writer.Write(localPos);
                    localPos += (ushort)parts[i].Length;
                }
                else
                    writer.Write((ushort)0);
            }
            pos += 0x20;
            foreach(var part in parts) {
                writer.Write(part);
                pos+= (uint)part.Length;
            }
        }
        #endregion

        writer.BaseStream.Seek(header, SeekOrigin.Begin);
        writer.Write((byte)01);
        writer.Write((byte)01);
        writer.Seek(1, SeekOrigin.Current);
        writer.Write((byte)Flags);
        writer.Write((byte)(Size.X/128));
        writer.Write((byte)(Size.Y/128));
        writer.Seek(42, SeekOrigin.Current);
        writer.Write(ReusedTileset);
        writer.Seek(12, SeekOrigin.Current);
    }
}