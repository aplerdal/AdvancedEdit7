using System;
using AdvancedEdit.UI.Editors;
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
    /// <param name="trackView">The window the tool belongs to</param>
    public abstract void Update(TrackView trackView);
}

public abstract class TilemapEditorTool
{
    public abstract void Update(TilemapEditor editor);
}