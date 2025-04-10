using AdvancedEdit.UI.Editors;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;

namespace AdvancedEdit.UI.Tools;

public class Move : TilemapEditorTool, ISelectableTool
{
    public string Icon => "move";
    public ImGuiKey? Shortcut => ImGuiKey.M;

    private bool _dragging = false;
    private Point _dragStart;
    
    public override void Update(TilemapEditor editor)
    {
        if (editor.SelectionManager.HasPoint(editor.View.HoveredTile))
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeAll);
            if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
            {
                if (!_dragging)
                {
                    _dragStart = editor.View.HoveredTile;
                    _dragging = true;
                }
            } else if (_dragging)
            {
                _dragging = false;
                //editor.View.Track.Tilemap.SetTiles();
            }
        }
    }
}