using Hexa.NET.ImGui;

namespace AdvancedEdit.UI.Framework.Windows;

public class DockWindow : Window
{
    public ImGuiDir DockDirection = ImGuiDir.None;
    public float SplitRatio = 0.0f;
    public DockWindow? ParentDock;
    public uint DockID;
    public bool IsFullScreen = false;
    private DockSpaceWindow? _parent = null;

    public DockWindow(DockSpaceWindow parent)
    {
        _parent = parent;
    }

    public DockWindow(DockSpaceWindow parent, string name) : base(name)
    {
        _parent = parent;
    }

    public override string GetWindowID()
    {
        if (_parent != null)
            return $"{ParentDock.GetWindowID()}_{Name}";
        return Name;
    }

    public override string ToString() => $"{Name}_{DockDirection}_{SplitRatio}_{DockID}";
}