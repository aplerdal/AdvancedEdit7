using System;
using System.Collections.Generic;
using System.Diagnostics;
using AdvancedEdit.Serialization;
using AdvancedEdit.UI.Editors;
using AdvancedEdit.UI.Undo;
using AdvancedEdit.UI.Windows;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace AdvancedEdit.UI.Tools;

public class DrawAction : IUndoable
{
    public Dictionary<Point, byte> NewTiles;
    private Dictionary<Point, byte> _oldTiles;
    private readonly Track _track;

    public DrawAction(Track track)
    {
        _track = track;
        NewTiles = new Dictionary<Point, byte>();
        _oldTiles = new Dictionary<Point, byte>();
    }

    // Could make this faster by removing draw the first time it is called if needed.
    public void Do()
    {
        _oldTiles = new Dictionary<Point, byte>();
        foreach (var tile in NewTiles.Keys)
        {
            _oldTiles.Add(tile, _track.Tilemap.Layout[tile.X, tile.Y]);
        }
        _track.Tilemap.SetTiles(NewTiles);
    }

    public void Undo()
    {
        _track.Tilemap.SetTiles(_oldTiles);
    }
}

public class Draw : TilemapEditorTool
{
    private DrawAction? _drawAction;
    
    public override void Update(TilemapEditor editor)
    {
        Point hoveredTile = editor.View.HoveredTile;

        if (editor.ActiveTile is not null)
        {
            byte tile = editor.ActiveTile.Value;
            if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && ImGui.IsWindowHovered())
            {
                if (_drawAction is null) {
                    _drawAction = new DrawAction(editor.View.Track);
                } else {
                    if (!_drawAction.NewTiles.ContainsKey(hoveredTile))
                    {
                        editor.View.Track.Tilemap.DrawTile(hoveredTile, tile);
                        _drawAction.NewTiles.Add(hoveredTile, tile);
                    }
                }
            }
            else if (_drawAction is not null)
            {
                editor.UndoManager.Do(_drawAction);
                _drawAction = null;
            }
            var min = editor.View.TileToWindow(hoveredTile);
            var max = editor.View.TileToWindow(hoveredTile+new Point(1));
            ImGui.SetCursorScreenPos(min);
            Debug.Assert(editor.View.Track.Tileset.TexturePtr != IntPtr.Zero);
            ImGui.Image(
                editor.View.Track.Tileset.TexturePtr,
                new(8 * editor.View.Scale),
                new(tile / 256f, 0),
                new(tile / 256f + 1 / 256f, 1)
            );
            ImGui.GetWindowDrawList().AddRect(min,max, Color.WhiteSmoke.PackedValue, 0, 0, 2.0f);
        }
    }
}
