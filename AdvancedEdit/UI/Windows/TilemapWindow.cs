using System;
using AdvancedEdit.Serialization.Types;
using AdvancedEdit.UI.Tools;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace AdvancedEdit.UI.Windows;

public abstract class TilemapWindow(Track track) : UiWindow
{
    //TODO: Maybe make if the image is hovered a bool or something? Lots of tools call it and that could cause issues if anything else is drawn for some reason
    
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
    public Vector2 CursorPosition;
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
    
    protected View View = new();

    protected IntPtr MapPtr;

    public override void Draw(bool hasFocus)
    {
        WindowPosition = ImGui.GetWindowPos();
        WindowSize = ImGui.GetWindowSize();

        CursorPosition = WindowPosition + Translation;
        MapSize = new Vector2(Track.Size.X, Track.Size.Y) * 8 * Scale;

        if (MapPtr == IntPtr.Zero)
            MapPtr = AdvancedEdit.Instance.ImGuiRenderer.BindTexture(Track.Tilemap.TrackTexture);

        ImGui.SetCursorScreenPos(CursorPosition.ToNumerics());
        ImGui.Image(MapPtr, MapSize.ToNumerics());
        
        if (hasFocus) View.Update(this);
    }

    ~TilemapWindow()
    {
        AdvancedEdit.Instance.ImGuiRenderer.UnbindTexture(MapPtr);
    }
}