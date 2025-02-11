using System.Collections.Generic;
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

    public void Do() // Could make this faster by removing draw the first time it is called.
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
    public byte? ActiveTile = null;
    
    public override void Update(TilemapEditor editor)
    {
        Vector2 mousePosition = ImGui.GetMousePos();
        if (ImGui.IsItemHovered())
        {
            Vector2 hoveredTile = editor.Window.HoveredTile.ToVector2();
            hoveredTile = new Vector2((int)hoveredTile.X, (int)hoveredTile.Y);
            Vector2 absoluteHoveredTile = editor.Window.MapPosition + hoveredTile * (8 * editor.Window.Scale);

            if (ActiveTile is not null)
            {
                byte tile = ActiveTile.Value;
                if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    if (_drawAction is null) {
                        _drawAction = new DrawAction(editor.Window.Track);
                    } else {
                        if (!_drawAction.NewTiles.ContainsKey(new((int)hoveredTile.X, (int)hoveredTile.Y)))
                        {
                            editor.Window.Track.Tilemap.DrawTile(new((int)hoveredTile.X, (int)hoveredTile.Y), tile);
                            _drawAction.NewTiles.Add(new((int)hoveredTile.X, (int)hoveredTile.Y), tile);
                        }
                    }
                }
                else if (_drawAction is not null)
                {
                    editor.UndoManager.Do(_drawAction);
                    _drawAction = null;
                }

                ImGui.SetCursorScreenPos(absoluteHoveredTile.ToNumerics());
                ImGui.Image(
                    editor.Window.Track.Tileset.TexturePtr,
                    new(8 * editor.Window.Scale),
                    new(tile / 256f, 0),
                    new(tile / 256f + 1 / 256f, 1)
                );
            }
        }
    }
}
