using System;
using AdvancedEdit.Serialization.Types;
using AdvancedEdit.UI.Tools;
using AdvancedEdit.UI.Undo;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdvancedEdit.UI.Windows;

public class MapEditor : TilemapWindow, IInspector
{
    public override string Name => "Map Editor";

    public override ImGuiWindowFlags Flags => ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
    
    public UndoManager UndoManager = new();
    private MapTool _activeTool = new Draw();
    private Texture2D _tilePalette;
    private IntPtr _palettePtr;

    public byte? ActiveTile = null;

    private const int TileDisplaySize = 16;
    public MapEditor(Track track) : base(track)
    {
        _tilePalette = new Texture2D(AdvancedEdit.Instance.GraphicsDevice, 16 * 8, 16 * 8);
        Color[] data = new Color[16*8 * 16*8];
        Color[] newData = new Color[16 * 8 * 16 * 8];
        Track.Tileset.Texture.GetData(data);
        for (int tileY = 0; tileY < 16; tileY++)
        for (int tileX = 0; tileX < 16; tileX++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
            newData[x + y * 8 * 16 + tileX * 8 + tileY * 64 * 16] = data[x+y*256*8+(tileX+tileY*16)*8];
        _tilePalette.SetData(newData);
        _palettePtr = AdvancedEdit.Instance.ImGuiRenderer.BindTexture(_tilePalette);
    }

    public override void Draw(bool hasFocus)
    {
        #region Menu Bar
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("View"))
            {
                if (ImGui.MenuItem("Reset View"))
                {
                    Translation = Vector2.Zero;
                    Scale = 1.0f;
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit"))
            {
                if (ImGui.MenuItem("Undo", "ctrl+z"))
                {
                    UndoManager.Undo();
                }

                if (ImGui.MenuItem("Redo", "ctrl+y"))
                {
                    UndoManager.Redo();
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
        #endregion
        
        // Basic map management
        base.Draw(hasFocus);

        if (hasFocus)
        {   
            _activeTool.Update(this);

            if (ImGui.IsKeyChordPressed(ImGuiKey.ModCtrl | ImGuiKey.Z))
                UndoManager.Undo();
            if (ImGui.IsKeyChordPressed(ImGuiKey.ModCtrl | ImGuiKey.Y))
                UndoManager.Redo();
        }

        if (hasFocus) View.Update(this);
    }

    public void DrawInspector()
    {
        ImGui.Text("Tilemap Inspector");
        ImGui.Separator();
        Vector2 mousePosition = ImGui.GetMousePos();
        Vector2 cursorPosition = ImGui.GetCursorScreenPos();
        ImGui.Image(_palettePtr, new(TileDisplaySize * 16));
        if (ImGui.IsItemHovered())
        {
            Point hoverTile = ((mousePosition - cursorPosition) / TileDisplaySize).ToPoint();
            Vector2 absoluteHoverTile = hoverTile.ToVector2() * TileDisplaySize + cursorPosition;
            if (ImGui.IsItemClicked())
                ActiveTile = (byte?)((int)hoverTile.X + (int)hoverTile.Y * 16);
            ImGui.GetForegroundDrawList().AddRect(
                (absoluteHoverTile - new Vector2(2)).ToNumerics(),
                (absoluteHoverTile + new Vector2(TileDisplaySize+2)).ToNumerics(),
                ImGui.GetColorU32(ImGuiCol.ButtonHovered),
                0f,
                0,
                3.0f
                );
        }

        if (ActiveTile is not null)
        {
            ImGui.GetForegroundDrawList().AddRect(
                (cursorPosition + new Vector2(ActiveTile.Value%16, ActiveTile.Value/16) * TileDisplaySize - new Vector2(2)).ToNumerics(),
                (cursorPosition + new Vector2(ActiveTile.Value % 16, ActiveTile.Value/16) * TileDisplaySize + new Vector2(TileDisplaySize + 2)).ToNumerics(),
                ImGui.GetColorU32(ImGuiCol.ButtonActive),
                0f,
                0,
                3.0f
                );
        }

        ImGui.Separator();
    }

    ~MapEditor()
    {
        AdvancedEdit.Instance.ImGuiRenderer.UnbindTexture(_palettePtr);
    }
}