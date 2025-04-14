using System;
using System.IO;
using System.Linq;
using TinyDialogsNet;

namespace AdvancedEdit.UI.Dialogs;

public static class Dialog
{
    /// <summary>
    /// Open file dialog for ROMs
    /// </summary>
    /// <returns>file path or null</returns>
    public static string? OpenRom()
    {
        var result =
            TinyDialogs.OpenFileDialog(
                "Advanced Edit - Open ROM",
                "", false,
                new FileFilter("GBA Roms", ["*.gba"])
                );
        if (result.Canceled) return null;
        else return result.Paths.FirstOrDefault();
    }
    /// <summary>
    /// Save file dialog for ROMs
    /// </summary>
    /// <returns></returns>
    public static string? SaveRom()
    {
        var result =
            TinyDialogs.SaveFileDialog(
                "Advanced Edit - Open ROM",
                "",
                new FileFilter("GBA Roms", ["*.gba"])
            );
        if (result.Canceled) return null;
        else return result.Path;
    }
    /// <summary>
    /// Open file dialog for images
    /// </summary>
    /// <returns>file path or null</returns>
    public static string? OpenImage()
    {
        var result =
            TinyDialogs.OpenFileDialog(
                "Advanced Edit - Open Image",
                "", false,
                new FileFilter("Supported Image Files", ["*.png","*.bmp","*.gif"])
            );
        if (result.Canceled) return null;
        else return result.Paths.FirstOrDefault();
    }

    /// <summary>
    /// Save file dialog for images
    /// </summary>
    /// <returns>file path or null</returns>
    public static string? SaveImage()
    {
        var result =
            TinyDialogs.SaveFileDialog(
                "Advanced Edit - Open Image",
                "",
                new FileFilter("Supported Image Files", ["*.png", "*.bmp", "*.gif"])
            );
        if (result.Canceled) return null;
        else return result.Path;
    }
}