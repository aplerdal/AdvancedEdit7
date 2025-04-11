using System;
using System.Collections.Generic;
using Hexa.NET.ImGui;

namespace AdvancedEdit.UI.Framework.Menu;

public class MenuItem
{
    public bool Enabled { get; set; } = true;
    public bool Visible { get; set; } = true;
    public string Header { get; set; }
    public List<MenuItem> MenuItems = new List<MenuItem>();
    public bool CanCheck { get; set; }
    public bool IsChecked { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string ToolTip { get; set; } = string.Empty;
    public string Shortcut { get; set; } = string.Empty;
    public Action? RenderItems;
    private Action? _onClick;

    public MenuItem(string name)
    {
        Header = name;
    }

    public MenuItem(string name, char icon)
    {
        Header = name;
        Icon = icon.ToString();
    }

    public MenuItem(string name, EventHandler clicked, bool isChecked = false)
    {
        Header = name;
        _onClick = () => { clicked?.Invoke(this, EventArgs.Empty); };
        IsChecked = isChecked;
    }

    public MenuItem(string name, char icon, Action clicked, bool isChecked = false)
    {
        Header = name;
        Icon = icon.ToString();

        _onClick = clicked;
        IsChecked = isChecked;
    }

    public MenuItem(string name, Action clicked, bool isChecked = false)
    {
        Header = name;
        _onClick = clicked;
        IsChecked = isChecked;
    }

    /// <summary>
    /// Draws the menu item. Must be called inside a current menubar
    /// </summary>
    public void Render(bool alignFramePadding = true)
    {
        var header = Header;
        if (Icon != string.Empty)
            header = $"    {Icon}    {Header}";

        if (Header == string.Empty)
        {
            ImGui.Separator();
            return;
        }
        
        if (alignFramePadding)
            ImGui.AlignTextToFramePadding();

        bool opened = false;
        if (MenuItems.Count == 0 && RenderItems == null)
        {
            if (ImGui.MenuItem(header, Shortcut, IsChecked, Enabled))
            {
                if (CanCheck)
                    IsChecked = !IsChecked;
                
            }
        }
    }

    public void Execute()
    {
        _onClick?.Invoke();
    }
}