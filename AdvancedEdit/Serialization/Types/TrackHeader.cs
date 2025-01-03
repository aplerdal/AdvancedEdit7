using System;
using System.IO;
using ImGuiNET;

namespace AdvancedEdit.Serialization.Types;

public struct TrackHeader : ISerializable, IDataView
{
    public byte Magic;
    public byte IsTilesetCompressed;
    public byte TrackFlags;
    public byte Width; // These could be the wrong way around
    public byte Height;
    public uint ReusedTileset;
    public uint LayoutOffset;
    public uint TilesetOffset;
    public uint PaletteOffset;
    public uint BehaviorsOffset;
    public uint ObjectsOffset;
    public uint OverlayOffset;
    public uint ItemBoxesOffset;
    public uint FinishLineOffset;
    public uint TrackRoutine;
    public uint MinimapOffset;
    public uint AiOffset;
    public uint ObjectGfxOffset;
    public uint ObjectPalOffset;
    public uint ReusedObjects;
    public void Serialize(BinaryWriter writer)
    {
        throw new NotImplementedException();
    }

    public void Deserialize(BinaryReader reader)
    {
        Magic = reader.ReadByte();
        IsTilesetCompressed = reader.ReadByte();
        reader.BaseStream.Seek(1, SeekOrigin.Current);
        TrackFlags = reader.ReadByte();
        Width = reader.ReadByte();
        Height = reader.ReadByte();
        reader.BaseStream.Seek(42, SeekOrigin.Current);
        ReusedTileset = reader.ReadUInt32();
        reader.BaseStream.Seek(12, SeekOrigin.Current);
        LayoutOffset = reader.ReadUInt32();
        reader.BaseStream.Seek(60, SeekOrigin.Current);
        TilesetOffset = reader.ReadUInt32();
        PaletteOffset = reader.ReadUInt32();
        BehaviorsOffset = reader.ReadUInt32();
        ObjectsOffset = reader.ReadUInt32();
        OverlayOffset = reader.ReadUInt32();
        ItemBoxesOffset = reader.ReadUInt32();
        FinishLineOffset = reader.ReadUInt32();
        reader.BaseStream.Seek(36, SeekOrigin.Current);
        TrackRoutine = reader.ReadUInt32();
        MinimapOffset = reader.ReadUInt32();
        reader.BaseStream.Seek(4, SeekOrigin.Current);
        AiOffset = reader.ReadUInt32();
        reader.BaseStream.Seek(60, SeekOrigin.Current);
        ObjectGfxOffset = reader.ReadUInt32();
        ObjectPalOffset = reader.ReadUInt32();
        ReusedObjects = reader.ReadUInt32();
        reader.BaseStream.Seek(16, SeekOrigin.Current);
    }

    public void DrawData()
    {   
        ImGui.SeparatorText("Track Header:");
        
        int magic = Magic;
        ImGui.InputInt("Magic", ref magic);
        Magic = (byte)(magic);
        
        bool isTilesetCompressed = IsTilesetCompressed != 0;
        ImGui.Checkbox("Tileset Compressed?", ref isTilesetCompressed);
        IsTilesetCompressed = (byte)(isTilesetCompressed?1:0);
        
        int trackFlags = TrackFlags;
        ImGui.CheckboxFlags("Track Flags", ref trackFlags, 0b111);
        TrackFlags = (byte)trackFlags;

        int[] size = new int[] { Width, Height };
        ImGui.InputInt2("Track Size", ref size[0]);
        Width = (byte)size[0];
        Height = (byte)size[1];
        
        int reusedTileset = (int)ReusedTileset;
        ImGui.InputInt2("Reused Tileset", ref reusedTileset);
        ReusedTileset = (uint)reusedTileset;
        
        int layoutOffset = (int)LayoutOffset;
        ImGui.InputInt("Layout Offset", ref layoutOffset);
        LayoutOffset = (uint)layoutOffset;
        
        int tilesetOffset = (int)TilesetOffset;
        ImGui.InputInt("Tileset Offset", ref tilesetOffset);
        TilesetOffset = (uint)tilesetOffset;
        
        int paletteOffset = (int)PaletteOffset;
        ImGui.InputInt("Palette Offset", ref paletteOffset);
        PaletteOffset = (uint)paletteOffset;
        
        int behaviorsOffset = (int)BehaviorsOffset;
        ImGui.InputInt("Behaviors Offset", ref behaviorsOffset);
        BehaviorsOffset = (uint)behaviorsOffset;
        
        int objectsOffset = (int)ObjectsOffset;
        ImGui.InputInt("Objects Offset", ref objectsOffset);
        ObjectsOffset = (uint)objectsOffset;
        
        int overlayOffset = (int)OverlayOffset;
        ImGui.InputInt("Overlay Offset", ref overlayOffset);
        OverlayOffset = (uint)overlayOffset;
        
        int itemBoxesOffset = (int)ItemBoxesOffset;
        ImGui.InputInt("Item Boxes Offset", ref itemBoxesOffset);
        ItemBoxesOffset = (uint)itemBoxesOffset;
        
        int finishLineOffset = (int)FinishLineOffset;
        ImGui.InputInt("Finish Line Offset", ref finishLineOffset);
        FinishLineOffset = (uint)finishLineOffset;
        
        int trackRoutine = (int)TrackRoutine;
        ImGui.InputInt("Track Routine", ref trackRoutine);
        TrackRoutine = (uint)trackRoutine;
        
        int minimapOffset = (int)MinimapOffset;
        ImGui.InputInt("Minimap Offset", ref minimapOffset);
        MinimapOffset = (uint)minimapOffset;
        
        int aiOffset = (int)AiOffset;
        ImGui.InputInt("AI Offset", ref aiOffset);
        AiOffset = (uint)aiOffset;
        
        int objectGfxOffset = (int)ObjectGfxOffset;
        ImGui.InputInt("Object Gfx Offset", ref objectGfxOffset);
        ObjectGfxOffset = (uint)objectGfxOffset;
        
        int objectPalOffset = (int)ObjectPalOffset;
        ImGui.InputInt("Object Pal Offset", ref objectPalOffset);
        ObjectPalOffset = (uint)objectPalOffset;
        
        int reusedObjectsOffset = (int)ReusedObjects;
        ImGui.InputInt("Reused Objects", ref reusedObjectsOffset);
        ReusedObjects = (uint)reusedObjectsOffset;
        
    }
}