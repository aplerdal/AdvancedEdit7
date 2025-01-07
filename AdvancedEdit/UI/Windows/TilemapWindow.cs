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
    
    public Track Track = track;
    public Vector2 WindowPosition;
    public Vector2 WindowSize;
    public Vector2 CursorPosition;
    public Vector2 MapSize;
    public Vector2 Translation;
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