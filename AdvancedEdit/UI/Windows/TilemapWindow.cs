using System;
using System.Runtime.InteropServices;
using AdvancedEdit.Serialization;
using AdvancedEdit.UI.Tools;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AdvancedEdit.UI.Windows;

public abstract class TilemapWindow(Track track) : UiWindow
{
    public override ImGuiWindowFlags Flags => ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
    /// <summary>
    /// The current active track
    /// </summary>
    public Track Track = track;
    /// <summary>
    /// The window position
    /// </summary>
    public Vector2 WindowPosition;
    /// <summary>
    /// The size of the current window
    /// </summary>
    public Vector2 WindowSize;
    /// <summary>
    /// The absolute position of the map image
    /// </summary>
    public Vector2 MapPosition;
    /// <summary>
    /// The size of the map image
    /// </summary>
    public Vector2 MapSize;
    /// <summary>
    /// The relative position of the map
    /// </summary>
    public Vector2 Translation;
    /// <summary>
    /// The scale of the map
    /// </summary>
    public float Scale = 1.0f;
    /// <summary>
    /// True when the map image is hovered
    /// </summary>
    public bool Hovered = false;

    /// <summary>
    /// The position of the currently hovered tile
    /// </summary>
    public Point HoveredTile => ((ImGui.GetMousePos() - MapPosition) / (8 * Scale)).ToPoint();
    
    protected View View = new();

    private IntPtr _mapPtr;

    private ImDrawListPtr DrawList => _drawList ?? throw new NullReferenceException("No active draw list!");
    private ImDrawListPtr? _drawList;

    /// <summary>
    /// Convert from tilespace to window space
    /// </summary>
    /// <param name="tile">Position of tile</param>
    /// <returns>World space position</returns>
    public System.Numerics.Vector2 TileToWindow(Point tile) => (tile.ToVector2() * 8 * Scale + MapPosition).ToNumerics();
    public System.Numerics.Vector2 PixelToWindow(Point pixel) => (pixel.ToVector2() * Scale + MapPosition).ToNumerics();

    private void DrawCoords()
    {
        string text = $"{HoveredTile.X}, {HoveredTile.Y}";
        var pos = ImGui.GetMousePos();
        DrawList.AddRectFilled(pos, pos + ImGui.CalcTextSize(text), Color.DarkGray.PackedValue);
        DrawList.AddText(pos, Color.White.PackedValue, text);
    }

    /// <summary>
    /// Draw rectangle given tile coordinates
    /// </summary>
    /// <param name="min">The coordinates of the smallest point</param>
    /// <param name="max">The coordinates of the largest point</param>
    /// <param name="color">The color of the rectangle</param>
    /// <param name="hovered">True when hovered</param>
    /// <returns>Is rectangle hovered</returns>
    public bool Rectangle(Point min, Point max, Color color, bool hovered = false)
    {
        DrawCoords();
        var hov = HoveredTile;
        Color trans = new Color(color.R, color.G, color.B, (byte)(color.A * (hovered?0.75f:0.5f)));
        DrawList.AddRectFilled(TileToWindow(min), TileToWindow(max), trans.PackedValue);
        DrawList.AddRect(TileToWindow(min), TileToWindow(max), color.PackedValue, 0, 0, 2.0f);
        return hov.X >= min.X && hov.X < max.X && hov.Y >= min.Y && hov.Y < max.Y;
    }
    
    public override void Draw(bool hasFocus)
    {
        _drawList = ImGui.GetWindowDrawList();
        
        WindowPosition = ImGui.GetWindowPos();
        WindowSize = ImGui.GetWindowSize();

        MapPosition = WindowPosition + Translation;
        MapSize = new Vector2(Track.Size.X, Track.Size.Y) * 8 * Scale;

        if (_mapPtr == IntPtr.Zero)
            _mapPtr = AdvancedEdit.Instance.ImGuiRenderer.BindTexture(Track.Tilemap.TrackTexture);

        ImGui.SetCursorScreenPos(MapPosition.ToNumerics());
        ImGui.Image(_mapPtr, MapSize.ToNumerics());
        Hovered = ImGui.IsItemHovered();
    }

    ~TilemapWindow()
    {
        AdvancedEdit.Instance.ImGuiRenderer.UnbindTexture(_mapPtr);
    }
}