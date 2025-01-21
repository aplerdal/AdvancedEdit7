using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedEdit.Serialization.Types;
using AdvancedEdit.UI.Undo;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AdvancedEdit.UI.Windows.Editors;

public struct Drag : IUndoable
{
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


    public void Do()
    {
        _new ??= new(_sectors[SectorNumber]);
        _sectors[SectorNumber] = _new;
    }

    public void Undo()
    {
        _sectors[SectorNumber] = Original;
    }
}

public class AiEditor(TilemapWindow window) : TrackEditor(window)
{
    private MouseCursor _mouseCursor = MouseCursor.Arrow;
    private bool _dragging;
    private int _selectedSector = -1;

    private Drag _drag = new();

    public override string Name => "Ai Editor";
    public override string Id => "aieditor";

    public override void Update(bool hasFocus)
    {
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
            for (var i = 0; i < Window.Track.AiSectors.Count; i++)
            {
                var sector = Window.Track.AiSectors[i];
                if (hasFocus)
                {
                    var thisHover = sector.GetHover(Window.HoveredTile);
                    if (thisHover > hovered)
                    {
                        hovered = thisHover;
                        hoveredIndex = i;

                        handle = sector.GetResizeHandle(Window.HoveredTile);
                        if (handle != ResizeHandle.None && hovered != HoverPart.Target)
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
                    _drag = new Drag(Window.Track.AiSectors, hoveredIndex);
                    _drag.Part = hovered;
                    _drag.Handle = handle;
                    _drag.LastPosition = new Point(Window.HoveredTile.X/2, Window.HoveredTile.Y/2);
                }

                var sector = Window.Track.AiSectors[_drag.SectorNumber];
                _dragging = true;
                var halfHovTile = new Point(Window.HoveredTile.X/2, Window.HoveredTile.Y/2);
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
                    sector.Resize(_drag.Handle, Window.HoveredTile.X, Window.HoveredTile.Y);
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
                if (_drag.Original == Window.Track.AiSectors[_drag.SectorNumber])
                {
                    _selectedSector = _drag.SectorNumber;
                }
                else
                {
                    UndoManager.Do(_drag);
                }
            }
        }

        bool tab = ImGui.IsKeyDown(ImGuiKey.Tab);
        for (var i = 0; i < Window.Track.AiSectors.Count; i++)
        {
            var sector = Window.Track.AiSectors[i];
            DrawAiSector(sector, i==hoveredIndex, i==_selectedSector);
            if (tab){
                var halfTextSize = ImGui.CalcTextSize(i.ToString())/2;
                var center = sector.Center * Window.Scale * 8 + Window.MapPosition;
                ImGui.GetWindowDrawList().AddRectFilled(((center - halfTextSize)).ToNumerics(), (center + halfTextSize).ToNumerics(), 0x80404040);
                ImGui.GetWindowDrawList().AddText(((center - halfTextSize)).ToNumerics(), 0xffffffff, $"{i}");
            }
        }
    }

    public override void DrawInspector()
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
            var sector = Window.Track.AiSectors[_selectedSector];

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
        ImGui.SeparatorText("Sector List");
        if (ImGui.Button("Add Sector")) {
            Window.Track.AiSectors.Add(new AiSector(new Point(64,64)));
        }
        ImGui.SameLine();
        if ((ImGui.Button("Duplicate Sector") || ImGui.IsKeyChordPressed(ImGuiKey.ModCtrl | ImGuiKey.D)) && _selectedSector != -1){
            var sector = new AiSector(Window.Track.AiSectors[_selectedSector]);
            sector.Position += new Point(2);
            sector.Target += new Point(2);
            Window.Track.AiSectors.Add(sector);
            _selectedSector = Window.Track.AiSectors.Count - 1;
        }
        ImGui.SameLine();
        if (ImGui.Button("Delete Sector") || ImGui.IsKeyPressed(ImGuiKey.Delete)){
            Window.Track.AiSectors.RemoveAt(_selectedSector);
            _selectedSector = Math.Clamp(_selectedSector, 0, Window.Track.AiSectors.Count - 1);
        }
    }

    private static void HelpMarker(string desc)
    {
        ImGui.TextDisabled("(?)");
        if (ImGui.BeginItemTooltip())
        {
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
            ImGui.TextUnformatted(desc);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }

    // Colors: 0x82ed76ff, 0x76ede1ff, 0xe176ed, 0xed7682
    private static readonly uint[] SolidZoneColors = [0xff82ed76, 0xff76ede1, 0xffe176ed, 0xffed7682];
    private static readonly uint[] FillZoneColors = [0x4082ed76, 0x4076ede1, 0x40e176ed, 0x40ed7682];
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
            var min = rect.Location.ToVector2().ToNumerics() * Window.Scale * 8 + Window.MapPosition.ToNumerics();
            var max = (rect.Location + rect.Size).ToVector2().ToNumerics() * Window.Scale * 8 + Window.MapPosition.ToNumerics();
            drawlist.AddRectFilled(min, max, fillColor);
            drawlist.AddRect(min, max, outlineColor, 0, 0, outlineThickness);

            var tmin = ((sector.Target.ToVector2() - Vector2.One) * Window.Scale * 8 + Window.MapPosition).ToNumerics();
            var tget = ((sector.Target.ToVector2()) * Window.Scale * 8 + Window.MapPosition).ToNumerics();
            var tmax = ((sector.Target.ToVector2() + Vector2.One) * Window.Scale * 8 + Window.MapPosition).ToNumerics();
            drawlist.AddRectFilled(tmin, tmax, fillColor);
            drawlist.AddRect(tmin, tmax, outlineColor, 0, 0, outlineThickness);
            return;
        }

        var points = sector.GetTriangle();
        var loopPoints = points.Select(
            o=>(o.ToVector2() * Window.Scale * 8 + Window.MapPosition).ToNumerics()
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

        var targetMin = ((sector.Target.ToVector2() - Vector2.One) * Window.Scale * 8 + Window.MapPosition).ToNumerics();
        var target = ((sector.Target.ToVector2()) * Window.Scale * 8 + Window.MapPosition).ToNumerics();
        var targetMax = ((sector.Target.ToVector2() + Vector2.One) * Window.Scale * 8 + Window.MapPosition).ToNumerics();
        drawlist.AddRectFilled(targetMin, targetMax, fillColor);
        drawlist.AddRect(targetMin,targetMax, outlineColor, 0, 0, outlineThickness);
    }
}