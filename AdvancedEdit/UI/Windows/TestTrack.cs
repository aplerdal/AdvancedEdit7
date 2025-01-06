using System;
using System.IO;
using AdvancedEdit.Serialization.Types;
using ImGuiNET;

namespace AdvancedEdit.UI.Windows;

public class TestTrack : UiWindow
{
    public override string Name { get => "Test Track"; }
    public override ImGuiWindowFlags Flags => ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
    Track track;
    private IntPtr _texture;

    public TestTrack()
    {
        track = new Track(new BinaryReader(File.OpenRead("/home/aplerdal/Development/Mksc/mksc.gba")), 25, 0x0000,
            0x283d04);
        _texture = AdvancedEdit.Instance.ImGuiRenderer.BindTexture(track.Tilemap.TrackTexture);
    }
    public override void Draw(bool hasFocus)
    {
        ImGui.Text("This is a test track view");
        ImGui.Image(_texture, new(1024, 1024));
    }

    ~TestTrack()
    {
        AdvancedEdit.Instance.ImGuiRenderer.UnbindTexture(_texture);
    }
}