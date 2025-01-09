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
public struct Drag : IUndoable{
    public int SectorNumber;
    public HoverPart Part;
    public ResizeHandle Handle;
    public Point LastPosition;
    public AiSector Original = null;
    private AiSector _new = null;
    private readonly List<AiSector> _sectors;

    public Drag(List<AiSector> sectors, int sectorNumber)
    {
        _sectors = sectors;
        Original = new(sectors[sectorNumber]);
        SectorNumber = sectorNumber;
    }


    public void Do(){
        _new ??= new (_sectors[SectorNumber]);
        _sectors[SectorNumber] = _new;
    }
    public void Undo(){
        _sectors[SectorNumber] = Original;
    }
}
public class AiEditor(Track track) : TilemapWindow(track), IInspector
{
    public override string Name => $"Ai - {TrackSelector.GetTrackName(Track.Id)}";
    public override string WindowId => "aieditor";

    public override ImGuiWindowFlags Flags =>
        ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

    public UndoManager UndoManager = new();
    
    private MouseCursor _mouseCursor = MouseCursor.Arrow;
    private bool _dragging;
    private int _selectedSector;

    private Drag _drag = new();
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
            hovered = _drag.Part;
            handle = _drag.Handle;
            hoveredIndex = _drag.SectorNumber;
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
        
        if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && ImGui.IsWindowHovered()) // confirm mouse is in window before accepting input
        {
            if (hovered != HoverPart.None || _dragging)
            {
                if (!_dragging)
                {
                    _drag = new Drag(track.AiSectors, hoveredIndex);
                    _drag.Part = hovered;
                    _drag.Handle = handle;
                    _drag.LastPosition = new Point((HoveredTile.X/2), (HoveredTile.Y/2));
                }

                var sector = track.AiSectors[_drag.SectorNumber];
                _dragging = true;
                var halfHovTile = new Point((HoveredTile.X/2), (HoveredTile.Y/2));
                var delta = halfHovTile - _drag.LastPosition;
                _drag.LastPosition = halfHovTile;
                if (_drag.Handle == ResizeHandle.None || _drag.Part == HoverPart.Target)
                {
                    if (_drag.Part == HoverPart.Zone)
                    {
                        sector.Zone = sector.Zone with {X = sector.Zone.X+delta.X*2, Y = sector.Zone.Y+delta.Y * 2 };
                    }
                    sector.Target += delta * new Point(2);   
                }
                else
                {
                    sector.Resize(_drag.Handle, HoveredTile.X, HoveredTile.Y);
                }
            }
            else
            {
                _selectedSector = -1;
            }
        }
        else if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            if (_dragging)
            {
                _dragging = false;
                if (_drag.Original == track.AiSectors[_drag.SectorNumber])
                {
                    _selectedSector = _drag.SectorNumber;
                }
                else
                {
                    UndoManager.Do(_drag);
                }
            }
        }

        for (var i = 0; i < track.AiSectors.Count; i++)
        {
            var sector = track.AiSectors[i];
            DrawAiSector(sector, i==hoveredIndex, i==_selectedSector);
        }
        #endregion



        if (hasFocus)
        {
            if (ImGui.IsKeyChordPressed(ImGuiKey.ModCtrl | ImGuiKey.Z))
                UndoManager.Undo();
            if (ImGui.IsKeyChordPressed(ImGuiKey.ModCtrl | ImGuiKey.Y))
                UndoManager.Redo();
            View.Update(this);
        }
        
    }
    public void DrawInspector()
    {
        ImGui.SeparatorText($"Ai Editor");
        if (_selectedSector == -1)
        {
            ImGui.BeginDisabled();
            int temp = 0;
            bool tempBool = false;
            ImGui.Combo("Shape", ref temp, ["Rectangle"], 1);
            ImGui.SameLine();
            HelpMarker("Sets shape of the zone. The direction on triangles refers to the right angle position.");
            ImGui.InputInt("Speed", ref temp);
            ImGui.SameLine();
            HelpMarker("Sets the speed the AI will move through the zone from 0(slowest) to 3(fastest).");
            ImGui.Checkbox("Intersection", ref tempBool);
            ImGui.SameLine();
            HelpMarker("Determines if the element is at an intersection. When an AI element is flagged as an intersection, this tells the AI to ignore the intersected AI zones, and avoids track object display issues when switching zones.");
            ImGui.EndDisabled();
        }
        else
        {
            var sector = track.AiSectors[_selectedSector];

            int shape = (int)sector.Shape;
            ImGui.Combo("Shape", ref shape, [
                "Rectangle", 
                "Triangle; top left", 
                "Triangle; top right", 
                "Triangle; bottom right",
                "Triangle; bottom left"
            ], 5);
            ImGui.SameLine();
            HelpMarker("Sets shape of the zone. The direction on triangles refers to the right angle position.");
            sector.Shape = (ZoneShape)shape;
            
            int speedBuffer = sector.Speed;
            ImGui.InputInt("Speed", ref speedBuffer);
            ImGui.SameLine();
            HelpMarker("Sets the speed the AI will move through the zone from 0(slowest) to 3(fastest).");
            sector.Speed = speedBuffer;

            bool intersectionBuffer = sector.Intersection;
            ImGui.Checkbox("Intersection", ref intersectionBuffer);
            ImGui.SameLine();
            HelpMarker("Determines if the element is at an intersection. When an AI element is flagged as an intersection, this tells the AI to ignore the intersected AI zones, and avoids track object display issues when switching zones.");
            sector.Intersection = intersectionBuffer;
        }
    }

    private static void HelpMarker(string desc)
    {
        ImGui.TextDisabled("(?)");
        if (ImGui.BeginItemTooltip())
        {
            ImGui.PushTextWrapPos(ImGui.GetFontSize()*35f);
            ImGui.TextUnformatted(desc);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
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
        var vertex = loopPoints[^2];
        var armX = loopPoints[^2];
        var armY = loopPoints[^3];
        
        drawlist.Flags = ImDrawListFlags.None;
        for (int i = 0; i < loopPoints.Length - 3; i++)
        {
            ImGui.GetWindowDrawList().AddTriangleFilled(vertex, loopPoints[i], loopPoints[i+1], fillColor);
        }
        drawlist.AddPolyline(ref loopPoints[0], loopPoints.Length, outlineColor, 0,
            outlineThickness);

        var targetMin = ((sector.Target.ToVector2() - Vector2.One) * Scale * 8 + MapPosition).ToNumerics();
        var target = ((sector.Target.ToVector2()) * Scale * 8 + MapPosition).ToNumerics();
        var targetMax = ((sector.Target.ToVector2() + Vector2.One) * Scale * 8 + MapPosition).ToNumerics();
        drawlist.AddRectFilled(targetMin, targetMax, fillColor);
        drawlist.AddRect(targetMin,targetMax, outlineColor, 0, 0, outlineThickness);

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