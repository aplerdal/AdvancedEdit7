using System;
using System.Diagnostics;
using System.IO;
using AdvancedEdit.Serialization;
using AdvancedEdit.UI.Components;
using AdvancedEdit.UI.Tools;
using AdvancedEdit.UI.Windows;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NativeFileDialogs.Net;
using Hjg.Pngcs;

namespace AdvancedEdit.UI.Editors;

public class TilemapEditor : TrackEditor
{
    public override string Name => "Tilemap Editor";
    public override string Id => "tileeditor";
    private Toolbox _toolbox;
    public SelectionManager SelectionManager;
    private ImTextureID _palettePtr;
    private Track _track;

    public byte? ActiveTile = null;

    public TilemapEditor(TrackView trackView) : base(trackView)
    {
        SelectionManager = new SelectionManager();
        _track = trackView.Track;
        _toolbox = new Toolbox([new Draw(), new Eyedropper(), new Bucket(), new RectFill(), new TileSelect()]);
        RegenAtlas();
    }

    private void RegenAtlas()
    {
        var tilePalette = new Texture2D(AdvancedEdit.Instance.GraphicsDevice, 16 * 8, 16 * 8);
        Color[] data = new Color[16 * 8 * 16 * 8];
        Color[] newData = new Color[16 * 8 * 16 * 8];
        _track.Tileset.Texture.GetData(data);
        for (int tileY = 0; tileY < 16; tileY++)
        for (int tileX = 0; tileX < 16; tileX++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
            newData[x + y * 8 * 16 + tileX * 8 + tileY * 64 * 16] = data[x + y * 256 * 8 + (tileX + tileY * 16) * 8];
        tilePalette.SetData(newData);
        if (_palettePtr == IntPtr.Zero)
        {
            _palettePtr = AdvancedEdit.Instance.ImGuiRenderer.BindTexture(tilePalette);
        }
        else
        {
            AdvancedEdit.Instance.ImGuiRenderer.UpdateTexture(_palettePtr, tilePalette);
        }
    }

    private const int TileDisplaySize = 16;
    public override void Update(bool hasFocus)
    {
        if (hasFocus)
        {
            _toolbox.ActiveTool.Update(this); 
        }

        SelectionManager.Update(View);
    }

    public override void DrawInspector()
    {
        ImGui.Text("Tilemap Inspector");
        ImGui.Separator();
        Vector2 mousePosition = ImGui.GetMousePos();
        Vector2 cursorPosition = ImGui.GetCursorScreenPos();
        Debug.Assert(_palettePtr != IntPtr.Zero);
        ImGui.Image(_palettePtr, new(TileDisplaySize * 16));
        if (ImGui.IsItemHovered())
        {
            Point hoverTile = ((mousePosition - cursorPosition) / TileDisplaySize).ToPoint();
            Vector2 absoluteHoverTile = hoverTile.ToVector2() * TileDisplaySize + cursorPosition;
            if (ImGui.IsItemClicked())
                ActiveTile = (byte)(hoverTile.X + hoverTile.Y * 16);
            ImGui.GetForegroundDrawList().AddRect(
                (absoluteHoverTile - new Vector2(2)).ToNumerics(),
                (absoluteHoverTile + new Vector2(TileDisplaySize + 2)).ToNumerics(),
                ImGui.GetColorU32(ImGuiCol.ButtonHovered),
                0f,
                0,
                3.0f
            );
        }
        if (ActiveTile is not null)
        {
            ImGui.GetForegroundDrawList().AddRect(
                (cursorPosition + new Vector2(ActiveTile.Value % 16, ActiveTile.Value / 16) * TileDisplaySize -
                 new Vector2(2)).ToNumerics(),
                (cursorPosition + new Vector2(ActiveTile.Value % 16, ActiveTile.Value / 16) * TileDisplaySize +
                 new Vector2(TileDisplaySize + 2)).ToNumerics(),
                ImGui.GetColorU32(ImGuiCol.ButtonActive),
                0f,
                0,
                3.0f
            );
        }

        ImGui.BeginDisabled(ActiveTile is null);
        int buf = (ActiveTile is null) ? 0 : _track.Behaviors[ActiveTile.Value];
        ImGui.InputInt("Behavior", ref buf);
        if (ActiveTile is not null)
        {
            _track.Behaviors[ActiveTile.Value] = (byte)buf;
        }
        ImGui.EndDisabled();
        HelpMarker("Changes the material you are driving on. For a full list visit TODO.");
        
        ImGui.SeparatorText("Tileset Options");
        if (ImGui.Button("Load"))
        {
            var status = Nfd.OpenDialog(out var path);
            if (status == NfdStatus.Ok && path is not null)
            {
                try
                {
                    _track.Tileset = Gfx8.FromPng(path);
                }
                catch (ArgumentException e)
                {
                    ErrorManager.ShowError("Error reading image:", e);
                }
                RegenAtlas();
                _track.Tilemap.Tileset = _track.Tileset.Texture;
                _track.Tilemap.RegenMap();
            }
        }
        ImGui.SameLine();
        if (ImGui.Button("Save"))
        {
            var status = Nfd.SaveDialog(out var path);
            if (status == NfdStatus.Ok && path is not null)
            {
                try
                {
                    _track.Tileset.ExportPng(path);
                }
                catch (ArgumentException e)
                {
                    ErrorManager.ShowError("Error saving image:", e);
                }
            }
        }
        
        ImGui.SeparatorText("Tools");

        _toolbox.DrawToolbox();
        
        ImGui.Separator();
    }

    ~TilemapEditor()
    {
        if (_palettePtr != IntPtr.Zero)
            AdvancedEdit.Instance.ImGuiRenderer.UnbindTexture(_palettePtr);
    }
}