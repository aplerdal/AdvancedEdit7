using System.Collections.Generic;
using AdvancedEdit.Serialization.Types;
using AdvancedEdit.UI.Undo;
using AdvancedEdit.UI.Windows;
using ImGuiNET;
using Microsoft.Xna.Framework;

using SVector2 = System.Numerics.Vector2;

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

public class Draw : MapEditorTool
{
    public byte? ActiveTile = 0;

    private bool _held = false;
    private DrawAction _drawAction;
    
    public override void Update(MapEditor editor)
    {
        SVector2 mousePosition = ImGui.GetMousePos();
        if (ImGui.IsItemHovered())
        {
            SVector2 hoveredTile = (mousePosition - editor.CursorPosition) / (8 * editor.Scale);
            hoveredTile = new SVector2((int)hoveredTile.X, (int)hoveredTile.Y);
            SVector2 absoluteHoveredTile = editor.CursorPosition + hoveredTile * (8 * editor.Scale);

            if (ActiveTile is not null)
            {
                byte tile = ActiveTile.Value;
                if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    if (!_held) _drawAction = new DrawAction(editor.Track);
                    if (!_drawAction.NewTiles.ContainsKey(new((int)hoveredTile.X, (int)hoveredTile.Y)))
                    {
                        editor.Track.Tilemap.DrawTile(new((int)hoveredTile.X, (int)hoveredTile.Y), tile);
                        _drawAction.NewTiles.Add(new((int)hoveredTile.X, (int)hoveredTile.Y), tile);
                    }

                    _held = true;
                } else if (_held)
                {
                    _held = false;

                    editor.UndoManager.Do(_drawAction);
                }

                ImGui.SetCursorScreenPos(absoluteHoveredTile);
                ImGui.Image(
                    editor.Track.Tileset.TexturePtr, 
                    new SVector2(8* editor.Scale), 
                    new(tile/256f, 0),
                    new(tile/256f+1/256f, 1)
                    );
            }
        }
        
    }
}
