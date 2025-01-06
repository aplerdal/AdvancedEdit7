using System;
using AdvancedEdit.UI.Windows;

namespace AdvancedEdit.UI.Tools;

public abstract class Tool
{
    public abstract void Update();
}

public abstract class MapTool
{
    public abstract void Update(TilemapWindow window);
}

public abstract class MapEditorTool : MapTool
{
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