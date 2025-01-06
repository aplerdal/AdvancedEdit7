using ImGuiNET;

namespace AdvancedEdit.UI;

public abstract class UiWindow
{
    public abstract string Name { get; }
    public abstract ImGuiWindowFlags Flags { get; }
    public bool IsOpen = true;
    public int Id { get; set; }

    public abstract void Draw(bool hasFocus);
}