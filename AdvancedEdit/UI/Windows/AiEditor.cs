using System;
using System.Diagnostics;
using System.Linq;
using AdvancedEdit.UI.Undo;
using AdvancedEdit.Serialization.Types;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace AdvancedEdit.UI.Windows;
public struct Drag(AiSector sector) : IUndoable{
    public int SectorNumber;
    public HoverPart Part;
    public ResizeHandle Handle;
    public Point LastPosition;
    public AiSector Sector = sector;
    private AiSector _end = null;
    private AiSector _start = sector.Clone();


    public void Do(){
        _end ??= Sector.Clone();
        Sector = _end;
    }
    public void Undo(){
        Sector = _start;
    }
}
public class AiEditor(Track track) : TilemapWindow(track), IInspector
{
    public override string Name => "Ai Editor";
    public override ImGuiWindowFlags Flags =>
        ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
    private MouseCursor _mouseCursor = MouseCursor.Arrow;
    public UndoManager UndoManager = new();
    private bool _dragging;

    private Drag drag = new();
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

        #region Ai Input
        _mouseCursor = MouseCursor.Arrow;
        var hovered = HoverPart.None;
        var handle = ResizeHandle.None;
        int hoveredIndex = -1;

        if (_dragging)
        {
            hovered = drag.Part;
            handle = drag.Handle;
            hoveredIndex = drag.SectorNumber;
        }
        else
        {
            for (var i = 0; i < track.AiSectors.Count; i++)
            {
                var sector = track.AiSectors[i];
                if (hasFocus)
                {
                    var thisHover = sector.GetHover(HoveredTile);
                    if (thisHover > hovered)
                    {
                        hovered = thisHover;
                        hoveredIndex = i;

                        handle = sector.GetResizeHandle(HoveredTile);
                        if (handle != ResizeHandle.None)
                        {
                            switch (handle)
                            {
                                case ResizeHandle.Bottom:
                                case ResizeHandle.Top:
                                    _mouseCursor = MouseCursor.SizeNS;
                                    break;
                                case ResizeHandle.Left:
                                case ResizeHandle.Right:
                                    _mouseCursor = MouseCursor.SizeWE;
                                    break;
                                case ResizeHandle.TopLeft:
                                case ResizeHandle.BottomRight:
                                    _mouseCursor = MouseCursor.SizeNWSE;
                                    break;
                                case ResizeHandle.TopRight:
                                case ResizeHandle.BottomLeft:
                                    _mouseCursor = MouseCursor.SizeNESW;
                                    break;
                            }
                        }
                    }
                }
            }

            Mouse.SetCursor(_mouseCursor);
        }
        
        
        if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            if (hovered != HoverPart.None || _dragging)
            {
                if (!_dragging)
                {
                    drag = new Drag(track.AiSectors[hoveredIndex]);
                    drag.Part = hovered;
                    drag.Handle = handle;
                    drag.SectorNumber = hoveredIndex;
                    drag.LastPosition = new Point((HoveredTile.X/2), (HoveredTile.Y/2));
                }

                var sector = drag.Sector;
                _dragging = true;
                var halfHovTile = new Point((HoveredTile.X/2), (HoveredTile.Y/2));
                var delta = halfHovTile - drag.LastPosition;
                drag.LastPosition = halfHovTile;
                if (drag.Handle == ResizeHandle.None)
                {
                    if (drag.Part == HoverPart.Zone)
                    {
                        sector.Zone = sector.Zone with {X = sector.Zone.X+delta.X*2, Y = sector.Zone.Y+delta.Y * 2 };
                    }
                    sector.Target += delta * new Point(2);   
                }
                else
                {
                    sector.Resize(drag.Handle, HoveredTile.X, HoveredTile.Y);
                }
            }
        }
        else
        {
            if (_dragging)
            {
                UndoManager.Do(drag);
            }
        }

        for (var i = 0; i < track.AiSectors.Count; i++)
        {
            var sector = track.AiSectors[i];
            DrawAiSector(sector, i==hoveredIndex, false);
        }
        #endregion
        
        

        if (hasFocus) View.Update(this);
        
    }
    public void DrawInspector()
    {
        //
    }

    // Colors: 0x82ed76ff, 0x76ede1ff, 0xe176ed, 0xed7682
    private static readonly uint[] SolidZoneColors = [0xff82ed76, 0xff76ede1, 0xffe176ed, 0xffed7682];
    private static readonly uint[] FillZoneColors  = [0x4082ed76, 0x4076ede1, 0x40e176ed, 0x40ed7682];
    private static readonly uint[] HoverZoneColors = [0x8082ed76, 0x8076ede1, 0x80e176ed, 0x80ed7682];

    private void DrawAiSector(AiSector sector, bool hovered, bool selected)
    {
        var drawlist = ImGui.GetWindowDrawList();
        var fillColor = hovered | selected ? HoverZoneColors[sector.Speed] : FillZoneColors[sector.Speed];
        var outlineColor = selected ? 0xffffffff : SolidZoneColors[sector.Speed];
        float outlineThickness = hovered | selected ? 3f : 1f;
        if (sector.Shape == ZoneShape.Rectangle)
        {
            var rect = sector.Zone;
            var min = rect.Location.ToVector2().ToNumerics() * Scale * 8 + MapPosition.ToNumerics();
            var max = (rect.Location + rect.Size).ToVector2().ToNumerics() * Scale * 8 + MapPosition.ToNumerics();
            drawlist.AddRectFilled(min, max, fillColor);
            drawlist.AddRect(min, max, outlineColor, 0, 0, outlineThickness);

            var tmin = ((sector.Target.ToVector2() - Vector2.One) * Scale * 8 + MapPosition).ToNumerics();
            var tget = ((sector.Target.ToVector2()) * Scale * 8 + MapPosition).ToNumerics();
            var tmax = ((sector.Target.ToVector2() + Vector2.One) * Scale * 8 + MapPosition).ToNumerics();
            drawlist.AddRectFilled(tmin, tmax, fillColor);
            drawlist.AddRect(tmin, tmax, outlineColor, 0, 0, outlineThickness);
            return;
        }

        var points = sector.GetTriangle();
        var loopPoints = points.Select(
            o=>(o.ToVector2() * Scale * 8 + MapPosition).ToNumerics()
            ).ToArray();

        drawlist.AddPolyline(ref loopPoints[0], loopPoints.Length, outlineColor, 0,
            outlineThickness);
        var vertex = loopPoints[^2];
        var armX = loopPoints[^2];
        var armY = loopPoints[^3];
        
        drawlist.Flags = ImDrawListFlags.None;
        for (int i = 0; i < loopPoints.Length - 3; i++)
        {
            ImGui.GetWindowDrawList().AddTriangleFilled(vertex, loopPoints[i], loopPoints[i+1], fillColor);
        }

        var target_min = ((sector.Target.ToVector2() - Vector2.One) * Scale * 8 + MapPosition).ToNumerics();
        var target = ((sector.Target.ToVector2()) * Scale * 8 + MapPosition).ToNumerics();
        var target_max = ((sector.Target.ToVector2() + Vector2.One) * Scale * 8 + MapPosition).ToNumerics();
        drawlist.AddRectFilled(target_min, target_max, fillColor);
        drawlist.AddRect(target_min,target_max, outlineColor, 0, 0, outlineThickness);

        // Vector2 mousePosition = ImGui.GetMousePos();
        // switch (sector.Shape)
        // {
        //     case ZoneShape.TopLeft:
        //         if (mousePosition.Y < vertex.Y && mousePosition.X < vertex.X)
        //         {
        //             drawlist.AddLine(target, armX, outlineColor, outlineThickness);
        //             drawlist.AddLine(target, armY, outlineColor, outlineThickness);
        //             break;
        //         } else if (mousePosition.Y < vertex.Y || mousePosition.X < vertex.X)
        //         {
        //             drawlist.AddLine(target, armX, outlineColor, outlineThickness);
        //         }
        //
        //         break;
        //             
        // }
        
    }
}