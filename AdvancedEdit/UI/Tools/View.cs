using System;
using AdvancedEdit.UI.Windows;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace AdvancedEdit.UI.Tools;

public class View : MapTool
{
    const float ZoomFactor = 1.25f;
    private float _panSpeed = 5f;
    private bool _dragging;
    private Vector2 _dragPosition = Vector2.Zero;
    private Vector2 _dragMapPosition = Vector2.Zero;
    private float _lastScrollValue = 0;
    
    public override void Update(TilemapWindow window)
    {
        Vector2 mousePos = ImGui.GetMousePos();
        // Checks hover over image
        if (window.Hovered)
        {
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Middle))
            {
                _dragging = true;
                _dragPosition = ImGui.GetMousePos();
                _dragMapPosition = window.Translation;
            }
            
            var wheel = (Mouse.GetState().ScrollWheelValue / 360f) - _lastScrollValue;
            _lastScrollValue = Mouse.GetState().ScrollWheelValue / 360f;
            if (wheel != 0)
            {
                Vector2 relativeMousePosition = mousePos - window.MapPosition;
                if (wheel > 0)
                    window.Scale *= ZoomFactor;
                else
                    window.Scale /= ZoomFactor;
                var trackSize = new Vector2(window.Track.Size.X, window.Track.Size.Y);
                Vector2 pixelTrackSize = trackSize * 8f * window.Scale;
                window.Translation += relativeMousePosition - relativeMousePosition*(pixelTrackSize/window.MapSize);
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

        #region Kb input

        if (ImGui.IsKeyChordPressed(ImGuiKey.ModCtrl | ImGuiKey.Equal))
        {
            Vector2 center = ImGui.GetWindowPos()+ImGui.GetWindowSize()/2 - window.MapPosition;
            window.Scale *= ZoomFactor;
            var trackSize = new Vector2(window.Track.Size.X, window.Track.Size.Y);
            Vector2 pixelTrackSize = trackSize * 8f * window.Scale;
            window.Translation += center - center * (pixelTrackSize / window.MapSize);
        }

        if (ImGui.IsKeyChordPressed(ImGuiKey.ModCtrl | ImGuiKey.Minus))
        {
            Vector2 center = ImGui.GetWindowPos() + ImGui.GetWindowSize() / 2 - window.MapPosition;
            window.Scale /= ZoomFactor;
            var trackSize = new Vector2(window.Track.Size.X, window.Track.Size.Y);
            Vector2 pixelTrackSize = trackSize * 8f * window.Scale;
            window.Translation += center - center * (pixelTrackSize / window.MapSize);
        }

        // Does not work for me? maybe on windows build though.
        _panSpeed = Math.Clamp((ImGui.IsKeyDown(ImGuiKey.ModShift) ? 10f : 5f) / window.Scale, 2.5f, 20f);
        
        if (ImGui.IsKeyDown(ImGuiKey.UpArrow))
        {
            window.Translation += new Vector2(0, _panSpeed);
        }

        if (ImGui.IsKeyDown(ImGuiKey.DownArrow))
        {
            window.Translation += new Vector2(0, -_panSpeed);
        }

        if (ImGui.IsKeyDown(ImGuiKey.LeftArrow))
        {
            window.Translation += new Vector2(_panSpeed, 0);
        }

        if (ImGui.IsKeyDown(ImGuiKey.RightArrow))
        {
            window.Translation += new Vector2(-_panSpeed, 0);
        }
        
        #endregion
    }
}