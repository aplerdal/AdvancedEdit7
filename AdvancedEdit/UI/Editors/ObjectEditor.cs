using System.Collections.Generic;
using AdvancedEdit.Serialization;
using AdvancedEdit.UI.Undo;
using AdvancedEdit.UI.Windows;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace AdvancedEdit.UI.Editors;

public struct ObjDrag : IUndoable
{
    public int ObjectNumber;
    public HoverPart Part;
    public ResizeHandle Handle;
    public Point LastPosition;
    public GameObject Original = null;
    private GameObject _new = null;
    private readonly List<GameObject> _objects;

    public ObjDrag(List<GameObject> objects, int objectNumber)
    {
        _objects = objects;
        Original = (GameObject)objects[objectNumber].Clone();
        ObjectNumber = objectNumber;
    }


    public void Do()
    {
        _new ??= (GameObject)_objects[ObjectNumber].Clone();
        _objects[ObjectNumber] = _new;
    }

    public void Undo()
    {
        _objects[ObjectNumber] = Original;
    }
}

public class ObjectEditor(TilemapWindow window) : TrackEditor(window)
{
    private bool _dragging;
    private int _selectedObject = -1;
    private int _hoveredIndex = -1;

    private ObjDrag _drag = new();
    public override string Name => "Object Editor";
    public override string Id => "objeditor";
    
    public override void Update(bool hasFocus)
    {
        if (_dragging)
        {
            _hoveredIndex = _drag.ObjectNumber;
        }

        bool newHover = false;
        for (var i = 0; i < Window.Track.Objects.Count; i++)
        {
            var gameObject = Window.Track.Objects[i];
            ImGui.GetWindowDrawList().AddCircleFilled(Window.TileToWindow(gameObject.Position), 4 * Window.Scale,
                Color.WhiteSmoke.PackedValue);
            if (Window.Rectangle(gameObject.Position + new Point(-2, -4), gameObject.Position + new Point(2, 0),
                    Color.WhiteSmoke, i == _hoveredIndex))
            {
                newHover = true;
                if (!_dragging)
                {
                    _hoveredIndex = i;
                    if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && !_dragging && ImGui.IsWindowHovered())
                    {
                        _drag = new ObjDrag(Window.Track.Objects, i);
                        _dragging = true;
                        _drag.LastPosition = Window.HoveredTile;
                    }
                }
            }
        }

        if (!newHover) _hoveredIndex = -1;

        if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && _dragging)
        {
            var delta = Window.HoveredTile - _drag.LastPosition;
            _drag.LastPosition = Window.HoveredTile;

            Window.Track.Objects[_drag.ObjectNumber].Position += delta;
        }

        if (_dragging && !ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            _dragging = false;
            if (_drag.Original.Position == Window.Track.Objects[_drag.ObjectNumber].Position)
            {
                _selectedObject = _drag.ObjectNumber;
            }
            else
            {
                UndoManager.Do(_drag);
            }
        }
    }

    public override void DrawInspector()
    {
        ImGui.SeparatorText($"Object Editor");
    }
}