using System;
using AdvancedEdit.UI.Windows;
using Hexa.NET.ImGui;
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
    
    public override void Update(TrackView trackView)
    {
        Vector2 mousePos = ImGui.GetMousePos();
        // Checks hover over image
        if (trackView.Hovered)
        {
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Middle))
            {
                _dragging = true;
                _dragPosition = ImGui.GetMousePos();
                _dragMapPosition = trackView.Translation;
            }
            
            var wheel = (Mouse.GetState().ScrollWheelValue / 360f) - _lastScrollValue;
            _lastScrollValue = Mouse.GetState().ScrollWheelValue / 360f;
            if (wheel != 0)
            {
                Vector2 relativeMousePosition = mousePos - trackView.MapPosition;
                if (wheel > 0)
                    trackView.Scale *= ZoomFactor;
                else
                    trackView.Scale /= ZoomFactor;
                var trackSize = new Vector2(trackView.Track.Size.X, trackView.Track.Size.Y);
                Vector2 pixelTrackSize = trackSize * 8f * trackView.Scale;
                trackView.Translation += relativeMousePosition - relativeMousePosition*(pixelTrackSize/trackView.MapSize);
            }
            
        }
        if (_dragging)
        {
            trackView.Translation = _dragMapPosition - (_dragPosition - mousePos);
        }

        if (_dragging && !ImGui.IsMouseDown(ImGuiMouseButton.Middle))
        {
            _dragging = false;
        }

        #region Kb input

        if (ImGui.Shortcut((int)(ImGuiKey.ModCtrl | ImGuiKey.Equal)))
        {
            Vector2 center = ImGui.GetWindowPos()+ImGui.GetWindowSize()/2 - trackView.MapPosition;
            trackView.Scale *= ZoomFactor;
            var trackSize = new Vector2(trackView.Track.Size.X, trackView.Track.Size.Y);
            Vector2 pixelTrackSize = trackSize * 8f * trackView.Scale;
            trackView.Translation += center - center * (pixelTrackSize / trackView.MapSize);
        }

        if (ImGui.Shortcut((int)(ImGuiKey.ModCtrl | ImGuiKey.Minus)))
        {
            Vector2 center = ImGui.GetWindowPos() + ImGui.GetWindowSize() / 2 - trackView.MapPosition;
            trackView.Scale /= ZoomFactor;
            var trackSize = new Vector2(trackView.Track.Size.X, trackView.Track.Size.Y);
            Vector2 pixelTrackSize = trackSize * 8f * trackView.Scale;
            trackView.Translation += center - center * (pixelTrackSize / trackView.MapSize);
        }

        _panSpeed = Math.Clamp((ImGui.IsKeyDown(ImGuiKey.ModShift) ? 10f : 5f) / trackView.Scale, 2.5f, 20f);
        
        if (ImGui.IsKeyDown(ImGuiKey.UpArrow))
        {
            trackView.Translation += new Vector2(0, _panSpeed);
        }

        if (ImGui.IsKeyDown(ImGuiKey.DownArrow))
        {
            trackView.Translation += new Vector2(0, -_panSpeed);
        }

        if (ImGui.IsKeyDown(ImGuiKey.LeftArrow))
        {
            trackView.Translation += new Vector2(_panSpeed, 0);
        }

        if (ImGui.IsKeyDown(ImGuiKey.RightArrow))
        {
            trackView.Translation += new Vector2(-_panSpeed, 0);
        }
        
        #endregion
    }
}