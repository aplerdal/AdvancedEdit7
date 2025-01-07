using System;
using AdvancedEdit.UI.Windows;

namespace AdvancedEdit.UI.Tools;

public abstract class Tool
{
    public abstract void Update();
}

public abstract class MapTool
{
    /// <summary>
    /// Update and draw the tool
    /// </summary>
    /// <param name="window">The window the tool belongs to</param>
    public abstract void Update(TilemapWindow window);
}

public abstract class MapEditorTool : MapTool
{
    /// <summary>
    /// Update and draw the tool
    /// </summary>
    /// <param name="window">The window the tool belongs to</param>
    /// <exception cref="InvalidOperationException"></exception>
    public override void Update(TilemapWindow window)
    {
        // Ensure the passed TilemapWindow is a MapEditor
        if (window is MapEditor editor)
        {
            Update(editor);
        }
        else
        {
            throw new InvalidOperationException("Expected a MapEditor instance.");
        }
    }

    // Abstract method specific to MapEditorTool
    public abstract void Update(MapEditor editor);
}