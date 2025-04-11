using System;
using System.Numerics;
using Hexa.NET.ImGui;

namespace AdvancedEdit.UI.Framework.Windows;

public class Window
{
    public virtual string Name { get; set; } = "Window";
    public virtual ImGuiWindowFlags Flags { get; set; } = ImGuiWindowFlags.None;
    public bool Opened = true;
    public Vector2 Size { get; set; }
    public bool PlaceAtCenter = false;
    public bool IsFocused = false;
    public bool IsWindowHovered = false;
    public bool IsWindowFocused = false;
    public EventHandler? WindowClosing;
    private bool _windowClosing = false;
    protected bool Loaded = false;

    public virtual string GetWindowID() => Name;
    public virtual string GetWindowName() => Name; // TODO: Translation

    public Window()
    {
    }

    public Window(string name)
    {
        Name = name;
    }

    public Window(string name, Vector2 size)
    {
        Name = name;
        Size = size;
    }

    /// <summary>
    /// Displays and renders the window.
    /// </summary>
    /// <returns>True when window is visible</returns>
    public virtual bool Show()
    {
        if (!Opened)
        {
            IsWindowHovered = false;
            IsWindowFocused = false;
            return false;
        }

        if (!Loaded)
        {
            OnLoad();
            Loaded = true;
        }
        
        if (Size.X != 0 && Size.Y != 0) ImGui.SetNextWindowSize(Size, ImGuiCond.Once);

        if (PlaceAtCenter)
        {
            var size = ImGui.GetMainViewport().Size;
            ImGui.SetNextWindowPos(size * 0.5f, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        }

        string name = GetWindowName();
        bool visible = ImGui.Begin(name, ref Opened, Flags);
        IsWindowHovered = ImGui.IsWindowHovered();
        IsFocused = ImGui.IsWindowFocused();
        Size = ImGui.GetWindowSize();

        if (!Opened && !_windowClosing)
        {
            _windowClosing = true;
            WindowClosing?.Invoke(this, EventArgs.Empty);
            OnWindowClosing();
        }

        if (visible)
        {
            Render();
        }
        ImGui.End();
        return visible;
    }

    public virtual void OnLoad()
    {
        
    }

    public void Close() => Opened = false;

    public virtual void Render()
    {
        
    }

    public virtual void OnWindowClosing()
    {
        
    }
}