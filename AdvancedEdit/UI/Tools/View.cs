using System.Numerics;
using AdvancedEdit.UI.Windows;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;

namespace AdvancedEdit.UI.Tools;

public class View : MapTool
{
    const float ZoomFactor = 1.25f;
    private bool _dragging;
    private Vector2 _dragPosition = Vector2.Zero;
    private Vector2 _dragMapPosition = Vector2.Zero;
    private float _lastScrollValue = 0;
    
    public override void Update(TilemapWindow window)
    {
        Vector2 mousePos = ImGui.GetMousePos();
        // Checks hover over image
        if (ImGui.IsItemHovered())
        {
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Middle))
            {
                _dragging = true;
                _dragPosition = ImGui.GetMousePos();
                _dragMapPosition = window.Translation;
            }
            
            var wheel = (Mouse.GetState().ScrollWheelValue / 360f) - _lastScrollValue;
            _lastScrollValue = (Mouse.GetState().ScrollWheelValue / 360f);
            if (wheel != 0)
            {
                Vector2 relativeMousePosition = mousePos - window.CursorPosition;
                if (wheel > 0)
                    window.Scale *= ZoomFactor;
                else
                    window.Scale /= ZoomFactor;
                var trackSize = new Vector2(window.Track.Size.X, window.Track.Size.Y);
                Vector2 newTrackSize = trackSize * 8f * window.Scale;
                window.Translation += relativeMousePosition - relativeMousePosition*(newTrackSize/window.MapSize);
            }
            
        }
        if (_dragging)
        {
            window.Translation = _dragMapPosition - (_dragPosition - mousePos);
        }

        if (_dragging && !ImGui.IsMouseDown(ImGuiMouseButton.Middle))
        {
            _dragging = false;
        }
    }
}