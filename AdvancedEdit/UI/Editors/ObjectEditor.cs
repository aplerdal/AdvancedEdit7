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

        if (_hoveredIndex == -1 && ImGui.IsMouseDown(ImGuiMouseButton.Left) && ! _dragging){
            _selectedObject = -1;
        }

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

    bool objectsLayer = true;
    bool positionsLayer = true;
    bool boxesLayer = true;
    public override void DrawInspector()
    {
        ImGui.SeparatorText($"Object Editor");

        ImGui.SeparatorText("Layers");
        ImGui.Checkbox("Objects", ref objectsLayer);
        ImGui.Checkbox("Positions", ref positionsLayer);
        ImGui.Checkbox("Boxes", ref boxesLayer);

        ImGui.SeparatorText("Properties");
        if (_selectedObject > -1) {
            var obj = Window.Track.Objects[_selectedObject];
            int id = obj.Id & 0b01111111;
            ImGui.InputInt("ID: ", ref id);
            id &= 0b01111111;
            HelpMarker("Changes the id of the object. If you are using a object from a different track it is recommended to make this object global.");
            ImGui.CheckboxFlags("Global Object: ", ref id, 0x80);
            HelpMarker("Changes object to global table. Allows access to all objects from other tracks. For a full list of global objects check (TODO)");

            ImGui.SeparatorText("Object List");
            if (ImGui.Button("Add Object")) {
                Window.Track.Objects.Add(new GameObject(2, new Point(64,64), 0));
            }
            ImGui.SameLine();
            if (ImGui.Button("Duplicate Object") || ImGui.IsKeyChordPressed(ImGuiKey.ModCtrl | ImGuiKey.D)){
                var gameobj = (GameObject)Window.Track.Objects[_selectedObject].Clone();
                gameobj.Position += new Point(2);
                Window.Track.Objects.Add(gameobj);
                _selectedObject = Window.Track.Objects.Count-1;
            }
            ImGui.SameLine();
            if (ImGui.Button("Delete Object") || ImGui.IsKeyPressed(ImGuiKey.Delete)){
                Window.Track.AiSectors.RemoveAt(_selectedObject);
                _selectedObject = -1;
            }
        } else {
            ImGui.BeginDisabled();
            var i = 0;
            ImGui.InputInt("ID: ", ref i);
            HelpMarker(" ");
            ImGui.CheckboxFlags("Global Object: ", ref i, 0x80);
            HelpMarker(" ");
            ImGui.EndDisabled();
        }
    }
}