using System.Collections.Generic;
using System.Security.AccessControl;
using AdvancedEdit.Serialization;
using AdvancedEdit.UI.Undo;
using AdvancedEdit.UI.Windows;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace AdvancedEdit.UI.Editors;

public struct ObjDrag : IUndoable
{
    public int ObjectType;
    public int ObjectNumber;
    public HoverPart Part;
    public ResizeHandle Handle;
    public Point LastPosition;
    public GameObject Original;
    private GameObject? _new;
    public List<GameObject> ObjectsList;

    public ObjDrag(List<GameObject> objects, int objectNumber)
    {
        ObjectsList = objects;
        Original = (GameObject)objects[objectNumber].Clone();
        ObjectNumber = objectNumber;
    }


    public void Do()
    {
        _new ??= (GameObject)ObjectsList[ObjectNumber].Clone();
        ObjectsList[ObjectNumber] = _new;
    }

    public void Undo()
    {
        ObjectsList[ObjectNumber] = Original;
    }
}
enum ObjectType {
    Actor,
    Positions,
    Boxes,
}

public class ObjectEditor(TilemapWindow window) : TrackEditor(window)
{
    // This is a terrible name, but basically it gives the object type and index in the track of said object
    private record struct ObjectAccess(List<GameObject> List, int index);

    private ObjectAccess? _selection = null;
    private ObjectAccess? _hover = null;
    bool _hoverSet = false;

    private bool _dragging;
    private ObjDrag _drag = new();
    public override string Name => "Object Editor";
    public override string Id => "objeditor";
    
    public override void Update(bool hasFocus)
    {
        _hoverSet = false;
        var track = Window.Track;
        if (_actorsLayer)
            for (int i = 0; i < track.Actors.Count; i++){
                UpdateObject(Window.Track.Actors[i], new ObjectAccess(track.Actors, i));
            }
        if (_boxesLayer)
            for (int i = 0; i < track.ItemBoxes.Count; i++) {
                UpdateObject(Window.Track.ItemBoxes[i], new ObjectAccess(track.ItemBoxes, i));    
            }
        if (_positionsLayer)
            for (int i = 0; i < track.Positions.Count; i++){
                UpdateObject(Window.Track.Positions[i], new ObjectAccess(track.Positions, i));
            }
        if (!_hoverSet) _hover = null;
        UpdateDrag();

    }
    private void UpdateObject(GameObject obj, ObjectAccess access) {
        Color color = GetIdColor(obj.Id);
        ImGui.GetWindowDrawList().AddCircleFilled(
            Window.TileToWindow(obj.Position),
             4 * Window.Scale,
              color.PackedValue
        );
        if (Window.Rectangle(
            obj.Position + new Point(-2, -4), 
            obj.Position + new Point(2, 0),
            color,
            access == _hover)
        ) 
        {
            if (!_dragging) {
                _hoverSet = true;
                _hover = access;
                if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && ImGui.IsWindowHovered())
                {
                    _drag = new ObjDrag(access.List, access.index);
                    _dragging = true;
                    _drag.LastPosition = Window.HoveredTile;
                }
            }
        }
    }
    private void UpdateDrag(){
        if (_dragging) {
            if (ImGui.IsMouseDown(ImGuiMouseButton.Left)){
                if (ImGui.IsWindowHovered())
                {
                    var delta = Window.HoveredTile - _drag.LastPosition;
                    _drag.LastPosition = Window.HoveredTile;

                    _drag.ObjectsList[_drag.ObjectNumber].Position += delta;
                }
            } else {
                _dragging = false;
                if (_drag.Original.Position == _drag.ObjectsList[_drag.ObjectNumber].Position)
                {
                    _selection = new ObjectAccess(_drag.ObjectsList, _drag.ObjectNumber);
                }
                else
                {
                    for (var i = 0; i < Window.Track.AiSectors.Count; i++)
                    {
                        if (Window.Track.AiSectors[i].GetResizeHandle(_drag.ObjectsList[_drag.ObjectNumber].Position) != ResizeHandle.None)
                        {
                            _drag.ObjectsList[_drag.ObjectNumber].Zone = (byte)i;
                        }
                    }

                    UndoManager.Do(_drag);
                }
            }
        } else {
            if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && _hover == null && ImGui.IsWindowHovered()){
                _selection = null;
            }
        }
    }
    private static Color GetIdColor(int id){
        switch(id) {
            case 0x81: // GP start positions
            case 0x82:
            case 0x83:
            case 0x84:
            case 0x85:
            case 0x86:
            case 0x87:
            case 0x88:
                return Color.Aquamarine;
            case 0x89: // singlepak positions
            case 0x8A:
                return Color.DarkTurquoise;
            case 0x1: // Item boxes
                return Color.Violet;
            default:
                return Color.WhiteSmoke;
        }
    }
    bool _actorsLayer = true;
    bool _positionsLayer = true;
    bool _boxesLayer = true;
    public override void DrawInspector()
    {

        ImGui.SeparatorText("Layers");
        ImGui.Checkbox("Objects", ref _actorsLayer);
        ImGui.Checkbox("Positions", ref _positionsLayer);
        ImGui.Checkbox("Boxes", ref _boxesLayer);

        ImGui.SeparatorText("Properties");
        if (_selection is not null) {
            var selection = _selection.Value;
            GameObject obj = selection.List[selection.index];
            int id = obj.Id & 0b01111111;
            ImGui.InputInt("ID: ", ref id);
            id &= 0b01111111;
            int global = obj.Id & 0x80;
            HelpMarker("Changes the id of the object. If you are using a object from a different track it is recommended to make this object global.");
            ImGui.CheckboxFlags("Global Object: ", ref global, 0x80);
            HelpMarker("Changes object to global table. Allows access to all objects from other tracks. For a full list of global objects check (TODO)");
            obj.Id = (byte)(id | global);
            ImGui.SeparatorText("Object List");
            if (ImGui.Button("Add Object")) {
                selection.List.Add(new GameObject(2, new Point(64,64), 0));
            }
            ImGui.SameLine();
            if (ImGui.Button("Duplicate Object") || ImGui.IsKeyChordPressed(ImGuiKey.ModCtrl | ImGuiKey.D)){
                var gameobj = (GameObject)obj.Clone();
                gameobj.Position += new Point(2);
                selection.List.Add(gameobj);
                _selection = new ObjectAccess(selection.List, selection.List.Count-1);
            }
            ImGui.SameLine();
            if (ImGui.Button("Delete Object") || ImGui.IsKeyPressed(ImGuiKey.Delete)){
                selection.List.RemoveAt(selection.index);
                _selection = null;
            }
        } 
        else 
        {
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