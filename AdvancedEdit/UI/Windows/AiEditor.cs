using System.Linq;
using System.Numerics;
using AdvancedEdit.Serialization.Types;
using ImGuiNET;
using SVector2 = System.Numerics.Vector2;

namespace AdvancedEdit.UI.Windows;

public class AiEditor(Track track) : TilemapWindow(track), IInspector
{
    private HoverPart _hoverPart;
    private int _hoverSector;
    public override string Name => "Ai Editor";
    public override ImGuiWindowFlags Flags =>
        ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
    public override void Draw(bool hasFocus)
    {
        #region Menu Bar

        if (ImGui.BeginMenuBar())
        {
            // TODO: AI Menu bar
            ImGui.EndMenuBar();
        }

        #endregion
        
        base.Draw(hasFocus);

        int i = 0;
        foreach (var sector in track.AiSectors)
        {
            sector.Zone.Draw(this);
            if (hasFocus)
            {
                var hovered = sector.Zone.GetHoveredPart(ImGui.GetMousePos());
                if (hovered > _hoverPart)
                {
                    _hoverPart = hovered;
                    _hoverSector = i;
                }
            }

            i++;
        }

        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            track.AiSectors.ElementAt(_hoverSector).Zone.InitDrag(_hoverPart);
        } else if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            track.AiSectors.ElementAt(_hoverSector).Zone.Drag(_hoverPart);
        }
        
    }
    public void DrawInspector()
    {
        //
    }
}