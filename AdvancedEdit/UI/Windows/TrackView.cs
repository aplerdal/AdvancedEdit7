using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AdvancedEdit.Serialization;
using AdvancedEdit.UI.Editors;
using AdvancedEdit.UI.Tools;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace AdvancedEdit.UI.Windows;

public class TrackView
{
    public ImGuiWindowFlags Flags => ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
    public bool IsOpen = true;
    /// <summary>
    /// The current active track
    /// </summary>
    public Track Track;
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
    
    public View View = new();

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
        var hov = HoveredTile;
        Color trans = new Color(color.R, color.G, color.B, (byte)(color.A * (hovered?0.75f:0.5f)));
        DrawList.AddRectFilled(TileToWindow(min), TileToWindow(max), trans.PackedValue);
        DrawList.AddRect(TileToWindow(min), TileToWindow(max), color.PackedValue, 0, 0, 2.0f);
        return hov.X >= min.X && hov.X < max.X && hov.Y >= min.Y && hov.Y < max.Y;
    }
    public string Name => TrackSelector.GetTrackName(Track.Id);
    public string WindowId => "trackwindow";
    
    private List<TrackEditor> _editors;
    private int _activeEditor = -1;

    public TrackView(Track track)
    {
        Track = track;
        _editors = [new TilemapEditor(this), new AiEditor(this), new ObjectEditor(this)]; // Default editors
    }

    public void Draw(bool focused)
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
        
        if (_activeEditor != -1)
            _editors[_activeEditor].Update(focused);

        if (focused)
        {
            if (ImGui.IsKeyChordPressed(ImGuiKey.ModCtrl | ImGuiKey.Z))
                _editors[_activeEditor].UndoManager.Undo();
            if (ImGui.IsKeyChordPressed(ImGuiKey.ModCtrl | ImGuiKey.Y))
                _editors[_activeEditor].UndoManager.Redo();
        }
        
        View.Update(this);
    }

    public void DrawInspector()
    {
        if (ImGui.BeginTabBar("ActiveEditor", ImGuiTabBarFlags.Reorderable | ImGuiTabBarFlags.AutoSelectNewTabs))
        {
            if (ImGui.TabItemButton("+", ImGuiTabItemFlags.Trailing | ImGuiTabItemFlags.NoTooltip))
            {
                ImGui.OpenPopup("windowTypeSelector");
            }

            if (ImGui.BeginPopup("windowTypeSelector"))
            {
                if (ImGui.Button("Ai Editor"))
                {
                    if (!_editors.Exists(x => x.Id == "aieditor"))
                        _editors.Add(new AiEditor(this));
                    ImGui.CloseCurrentPopup();
                }

                if (ImGui.Button("Tilemap Editor"))
                {
                    if (!_editors.Exists(x => x.Id == "tileeditor"))
                        _editors.Add(new TilemapEditor(this));
                    ImGui.CloseCurrentPopup();
                }

               
                ImGui.EndPopup();
            }

            var list = _editors.ToImmutableList();
            _activeEditor = -1;
            for (var i = 0; i < list.Count; i++)
            {
                var editor = list[i];
                if (ImGui.BeginTabItem(editor.Name))
                {
                    _activeEditor = i;
                    editor.DrawInspector();
                    ImGui.EndTabItem();
                }
            }
        }
    }

    ~TrackView()
    {
        AdvancedEdit.Instance.ImGuiRenderer.UnbindTexture(_mapPtr);
    }
}