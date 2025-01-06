using System;
using AdvancedEdit.Serialization.Types;
using AdvancedEdit.UI.Tools;
using AdvancedEdit.UI.Undo;
using ImGuiNET;
using Microsoft.Xna.Framework;

using SVector2 = System.Numerics.Vector2;

namespace AdvancedEdit.UI.Windows;

public class MapEditor(Track track) : TilemapWindow(track)
{
    public override string Name => "Map Editor";

    public override ImGuiWindowFlags Flags => ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

    public UndoManager UndoManager = new();

    private MapTool _activeTool = new Draw();
    public override void Draw(bool hasFocus)
    {
        #region Menu Bar
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("View"))
            {
                if (ImGui.MenuItem("Reset View"))
                {
                    Translation = SVector2.Zero;
                    Scale = 1.0f;
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit"))
            {
                if (ImGui.MenuItem("Undo", "ctrl+z"))
                {
                    UndoManager.Undo();
                }

                if (ImGui.MenuItem("Redo", "ctrl+y"))
                {
                    UndoManager.Redo();
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
        #endregion
        
        // Basic map management
        base.Draw(hasFocus);

        if (hasFocus)
        {   
            _activeTool.Update(this);

            if (ImGui.IsKeyChordPressed(ImGuiKey.ModCtrl | ImGuiKey.Z))
                UndoManager.Undo();
            if (ImGui.IsKeyChordPressed(ImGuiKey.ModCtrl | ImGuiKey.Y))
                UndoManager.Redo();
        }
    }
}