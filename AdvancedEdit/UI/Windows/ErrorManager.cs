using System;
using System.Collections.Generic;
using ImGuiNET;

namespace AdvancedEdit.UI.Windows;

public static class ErrorManager
{
    record Error(string message, Exception? exception);
    private static Queue<Error> _errorQueue = new Queue<Error>();
    private static bool _showPopup = false;

    public static void ShowError(string prefix, Exception? e = null)
    {
        _errorQueue.Enqueue(new Error(prefix, e));
        _showPopup = true;
    }

    public static void Update()
    {
        if (_showPopup && _errorQueue.Count > 0)
        {
            ImGui.OpenPopup("Error");
            if (ImGui.BeginPopupModal("Error", ref _showPopup, ImGuiWindowFlags.AlwaysAutoResize))
            {
                var exception = _errorQueue.Peek().exception;
                var details = (exception is null) ? "" : exception.Message; 
                ImGui.TextWrapped(_errorQueue.Peek().message + " " + details);
                ImGui.NewLine();
                ImGui.TextWrapped("If you believe this error is a bug please copy the error info and submit a bug report.");
                if (exception is not null)
                {
                    if (ImGui.Button("Copy Error Info"))
                    {
                        var ex = exception.ToString();
                        ImGui.SetClipboardText(ex);
                    }

                    ImGui.SameLine();
                }
                if (ImGui.Button("Continue"))
                {
                    _errorQueue.Dequeue();
                    if (_errorQueue.Count == 0)
                    {
                        _showPopup = false;
                    }
                }
            }
        }
    }
}