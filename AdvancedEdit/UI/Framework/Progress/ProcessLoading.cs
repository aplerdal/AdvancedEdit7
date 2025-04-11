using System;
using Hexa.NET.ImGui;
using AdvancedEdit.UI.Framework.Helpers;

namespace AdvancedEdit.UI.Framework.Progress;

public class ProcessLoading {
    public static ProcessLoading? Instance = null;
    public bool IsLoading;
    public int ProcessAmount;
    public int ProcessTotal;
    public string? ProcessName = null;
    public EventHandler? OnUpdated;
    public string? Title {get; set;} = null;
    public ProcessLoading() {
        Instance = this;
    }

    public void UpdateIncrease(int amount, string process) {
        ProcessAmount += amount;
        ProcessName = process;
        OnUpdated?.Invoke(this,EventArgs.Empty);
    }
    public void Update(int amount, int total, string process, string title = "Loading") {
        Title = title;
        ProcessAmount = amount;
        ProcessTotal = total;
        ProcessName = process;
        Title = title;
        OnUpdated?.Invoke(this, EventArgs.Empty);
    }
    public void Draw(int width, int height) {
        if (!IsLoading)
            return;
        ImGui.SetNextWindowPos(new System.Numerics.Vector2(width * 0.5f, height * 0.5f),
            ImGuiCond.Always, new System.Numerics.Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(300, 68));

        var flags = ImGuiWindowFlags.AlwaysAutoResize;

        if (ImGui.Begin(Title, ref IsLoading, flags))
        {
            float progress = (float)this.ProcessAmount / this.ProcessTotal;
            ImGui.ProgressBar(progress, new System.Numerics.Vector2(300, 20));

            ImGuiHelper.DrawCenteredText($"{ProcessName}");
        }
        ImGui.End();
    }
}