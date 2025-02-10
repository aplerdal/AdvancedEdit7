using AdvancedEdit.UI.Undo;
using AdvancedEdit.UI.Windows;
using ImGuiNET;
namespace AdvancedEdit.UI.Editors;

/// <summary>
/// Base class for all the editor types that will be used. Tilemap editor, Ai editor, &c. &c.
/// </summary>
public abstract class TrackEditor(TilemapWindow window) : IInspector
{
    public abstract string Name { get; }
    public abstract string Id { get; }

    public UndoManager UndoManager = new();

    public TilemapWindow Window { get; set; } = window;

    public abstract void Update(bool hasFocus);

    public abstract void DrawInspector();

    protected static void HelpMarker(string desc)
    {
        ImGui.SameLine();
        ImGui.TextDisabled("(?)");
        if (ImGui.BeginItemTooltip())
        {
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
            ImGui.TextUnformatted(desc);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }
}