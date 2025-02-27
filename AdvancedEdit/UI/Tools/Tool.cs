using System;
using AdvancedEdit.UI.Editors;
using AdvancedEdit.UI.Windows;
using ImGuiNET;

namespace AdvancedEdit.UI.Tools;

public interface ISelectableTool
{
    public string Icon { get; }
    public ImGuiKey? Shortcut { get;  }
}
public abstract class Tool
{
    public abstract void Update(object? sender);
}

public abstract class MapTool : Tool
{
    public override void Update(object? sender)
    {
        if (sender is not null && sender is TrackView view)
            Update(view);
        else
            throw new ArgumentException("Invalid tool type");
    }
    public abstract void Update(TrackView trackView);
}

public abstract class TilemapEditorTool : Tool
{
    public override void Update(object? sender)
    {
        if (sender is not null && sender is TilemapEditor editor)
            Update(editor);
        else
            throw new ArgumentException("Invalid tool type");
    }

    public abstract void Update(TilemapEditor editor);
}