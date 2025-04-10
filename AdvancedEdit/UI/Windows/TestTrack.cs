using System;
using System.IO;
using AdvancedEdit.Serialization;
using Hexa.NET.ImGui;

namespace AdvancedEdit.UI.Windows;

public class TestTrack : UiWindow
{
    public override string Name { get => "Test Track"; }
    public override string WindowId => "testtrack";
    public override ImGuiWindowFlags Flags => ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
    Track _track;

    public TestTrack()
    {
        _track = new Track(new BinaryReader(File.OpenRead("/home/aplerdal/Development/Mksc/mksc.gba")), 25, 0x0000,
            0x283d04);
    }
    public override void Draw()
    {
        ImGui.Text("This is a test track view");
        ImGui.Image(_track.Tilemap.TexturePtr, new(1024, 1024));
    }
}