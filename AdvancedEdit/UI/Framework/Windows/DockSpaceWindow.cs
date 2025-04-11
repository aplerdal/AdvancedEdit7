using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Hexa.NET.ImGui;

namespace AdvancedEdit.UI.Framework.Windows;

public class DockSpaceWindow : Window, IDisposable
{
    public List<DockWindow> DockedWindows = new List<DockWindow>();
    public bool UpdateDockLayout = false;
    public bool IsFullscreen = false;
    public ImGuiWindowClassPtr WindowClass;

    public DockSpaceWindow(string name)
    {
        Name = name;
    }

    public override void Render()
    {
        uint dockspaceId = ImGui.GetID($"{Name}dock_layout");
        if (ImGuiP.DockBuilderGetNode(dockspaceId).IsNull || UpdateDockLayout)
        {
            ReloadDockLayout(dockspaceId);
        }

        ImGui.DockSpace(dockspaceId, Vector2.Zero, ImGuiDockNodeFlags.None, WindowClass);

        if (IsFullscreen)
        {
            var fullDock = DockedWindows.FirstOrDefault(x => x.IsFullScreen);
            fullDock?.Show();
        }
        else
        {
            foreach (var window in DockedWindows)
                window.Show();
        }
    }

    public void SetupParentDock(uint parentDockID, IEnumerable<DockSpaceWindow> children)
    {
        if (ImGuiP.DockBuilderGetNode(parentDockID).IsNull || UpdateDockLayout)
        {
            ImGuiP.DockBuilderRemoveNode(parentDockID); // Clear existing layout
            ImGuiP.DockBuilderAddNode(parentDockID, ImGuiDockNodeFlags.None); // Add empty node

            foreach (var workspace in children)
                ImGuiP.DockBuilderDockWindow(workspace.GetWindowName(), parentDockID);
            
            ImGuiP.DockBuilderFinish(parentDockID);
            UpdateDockLayout = false;
        }

        ImGui.DockSpace(parentDockID, Vector2.Zero, 0, WindowClass);
    }

    public void ReloadDockLayout(uint dockspaceID)
    {
        ImGuiP.DockBuilderRemoveNode(dockspaceID);
        ImGuiP.DockBuilderAddNode(dockspaceID, ImGuiDockNodeFlags.None);

        uint dockMainID = dockspaceID;

        foreach (var dock in DockedWindows)
            dock.DockID = 0;

        foreach (var dock in DockedWindows)
        {
            if (dock.DockDirection == ImGuiDir.None)
                dock.DockID = dockMainID;
            else
            {
                // Reuse existing dock ID when posssible
                var dockedWindow = DockedWindows.FirstOrDefault(x => x != dock &&
                                                                     x.DockDirection == dock.DockDirection &&
                                                                     x.SplitRatio == dock.SplitRatio &&
                                                                     x.ParentDock == dock.ParentDock);
                uint dockOut = 0;
                if (dockedWindow != null && dockedWindow.DockID != 0)
                    dock.DockID = dockedWindow.DockID;
                else if (dock.ParentDock != null)
                    dock.DockID = ImGuiP.DockBuilderSplitNode(dock.ParentDock.DockID, dock.DockDirection,
                        dock.SplitRatio, ref dockOut, ref dock.ParentDock.DockID);
                else
                    dock.DockID = ImGuiP.DockBuilderSplitNode(dockMainID, dock.DockDirection, dock.SplitRatio,
                        ref dockOut, ref dockMainID);
            }
            ImGuiP.DockBuilderDockWindow(dock.GetWindowName(), dock.DockID);
        }
        ImGuiP.DockBuilderFinish(dockspaceID);

        UpdateDockLayout = false;
    }

    public override void OnLoad()
    {
        if (Loaded)
            return;

        Loaded = true;

        unsafe
        {
            uint windowId = ImGui.GetID($"###window_{this.Name}");

            WindowClass = (ImGuiWindowClass*)Marshal.AllocHGlobal(sizeof(ImGuiWindowClass));
            ImGuiWindowClass windowClass = new ImGuiWindowClass();
            windowClass.ClassId = windowId;
            windowClass.DockingAllowUnclassed = 0;

            Marshal.StructureToPtr(windowClass, (IntPtr)WindowClass.Handle, false);
        }
    }


    public void Dispose()
    {
        unsafe
        {
            Marshal.FreeHGlobal((IntPtr)WindowClass.Handle);
            WindowClass = ImGuiWindowClassPtr.Null;
        }
    }
}