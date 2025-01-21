using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AdvancedEdit.Serialization.Types;
using AdvancedEdit.UI.Windows.Editors;
using ImGuiNET;

namespace AdvancedEdit.UI.Windows;

public class TrackWindow : TilemapWindow, IInspector
{
    public override string Name => TrackSelector.GetTrackName(Track.Id);
    public override string WindowId => "trackwindow";
    private List<TrackEditor> _editors;
    private int _activeEditor = -1;

    public TrackWindow(Track track) : base(track)
    {
        _editors = [new TilemapEditor(this), new AiEditor(this)]; // Default editors
    }

    public override void Draw(bool hasFocus)
    {
        base.Draw(hasFocus);
        
        if (_activeEditor != -1)
            _editors[_activeEditor].Update(hasFocus);

        if (hasFocus)
        {
            if (ImGui.IsKeyChordPressed(ImGuiKey.ModCtrl | ImGuiKey.Z))
                _editors[_activeEditor].UndoManager.Undo();
            if (ImGui.IsKeyChordPressed(ImGuiKey.ModCtrl | ImGuiKey.Y))
                _editors[_activeEditor].UndoManager.Redo();
        }
        
        View.Update(this);
    }

    public void DrawInspector()
    {
        if (ImGui.BeginTabBar("ActiveEditor", ImGuiTabBarFlags.Reorderable | ImGuiTabBarFlags.AutoSelectNewTabs))
        {
            if (ImGui.TabItemButton("+", ImGuiTabItemFlags.Trailing | ImGuiTabItemFlags.NoTooltip))
            {
                ImGui.OpenPopup("windowTypeSelector");
            }

            if (ImGui.BeginPopup("windowTypeSelector"))
            {
                if (ImGui.Button("Ai Editor"))
                {
                    if (!_editors.Exists(x => x.Id == "aieditor"))
                        _editors.Add(new AiEditor(this));
                    ImGui.CloseCurrentPopup();
                }

                if (ImGui.Button("Tilemap Editor"))
                {
                    if (!_editors.Exists(x => x.Id == "tileeditor"))
                        _editors.Add(new TilemapEditor(this));
                    ImGui.CloseCurrentPopup();
                }

               
                ImGui.EndPopup();
            }

            var list = _editors.ToImmutableList();
            _activeEditor = -1;
            for (var i = 0; i < list.Count; i++)
            {
                bool open = true;
                var editor = list[i];
                if (ImGui.BeginTabItem(editor.Name))
                {
                    _activeEditor = i;
                    editor.DrawInspector();
                    ImGui.EndTabItem();
                }
            }
        }
    }
}