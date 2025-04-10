using AdvancedEdit.UI.Undo;
using AdvancedEdit.UI.Windows;
using Hexa.NET.ImGui;
namespace AdvancedEdit.UI.Editors;

/// <summary>
/// Base class for all the editor types that will be used. Tilemap editor, Ai editor, etc
/// </summary>
public abstract class TrackEditor(TrackView trackView)
{
    public abstract string Name { get; }
    public abstract string Id { get; }

    public UndoManager UndoManager = new();

    public TrackView View { get; set; } = trackView;

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