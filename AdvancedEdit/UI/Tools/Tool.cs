using System;
using AdvancedEdit.UI.Windows;
using AdvancedEdit.UI.Windows.Editors;

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

public abstract class TilemapEditorTool
{
    public abstract void Update(TilemapEditor editor);
}