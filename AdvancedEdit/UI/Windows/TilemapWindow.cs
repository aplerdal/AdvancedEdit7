using System;
using AdvancedEdit.Serialization.Types;
using AdvancedEdit.UI.Tools;
using ImGuiNET;
using Microsoft.Xna.Framework;

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

    protected IntPtr MapPtr;

    public override void Draw(bool hasFocus)
    {
        WindowPosition = ImGui.GetWindowPos();
        WindowSize = ImGui.GetWindowSize();

        MapPosition = WindowPosition + Translation;
        MapSize = new Vector2(Track.Size.X, Track.Size.Y) * 8 * Scale;

        if (MapPtr == IntPtr.Zero)
            MapPtr = AdvancedEdit.Instance.ImGuiRenderer.BindTexture(Track.Tilemap.TrackTexture);

        ImGui.SetCursorScreenPos(MapPosition.ToNumerics());
        ImGui.Image(MapPtr, MapSize.ToNumerics());
        Hovered = ImGui.IsItemHovered();
    }

    ~TilemapWindow()
    {
        AdvancedEdit.Instance.ImGuiRenderer.UnbindTexture(MapPtr);
    }
}