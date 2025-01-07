using ImGuiNET;

namespace AdvancedEdit.UI;

public abstract class UiWindow
{
    /// <summary>
    /// The human-readable name of the current window
    /// </summary>
    public abstract string Name { get; }
    /// <summary>
    /// Window flags to be called when drawing this window
    /// </summary>
    public abstract ImGuiWindowFlags Flags { get; }
    /// <summary>
    /// True if the window is open
    /// </summary>
    public bool IsOpen = true;
    /// <summary>
    /// The internal ID of the window
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Draw the window, not including Imgui.Begin or End
    /// </summary>
    /// <param name="hasFocus">Is the window focused by the user</param>
    public abstract void Draw(bool hasFocus);
}