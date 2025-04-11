using System.Diagnostics;
using AdvancedEdit.UI.Framework.Progress;
using AdvancedEdit.UI.Renderer;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;

namespace AdvancedEdit.UI.Framework;

public class Framework : Game
{
    public MainWindow MainWindow;
    public ProcessLoading ProcessLoading;
    public ImGuiRenderer ImGuiRenderer;
    public Framework(MainWindow window) {
        MainWindow = window;
        window.Init(this);

        ProcessLoading = new ProcessLoading();
        ProcessLoading.OnUpdated += delegate {
            UpdateWindow();
        };
    }
    private void UpdateWindow() {
        if (!ProcessLoading.IsLoading)
            return;
            Draw(new GameTime());
    }
    protected override void Initialize() {
        ImGuiRenderer = new ImGuiRenderer(this);
        base.Initialize();
        MainWindow.OnApplicationLoad();
    }

    bool renderingFrame, executingAction, drawnOnce;
    protected override void Draw(GameTime gameTime)
    {
        if (renderingFrame) return;

        if (UIManager.ActionBeforeUIDraw != null && !executingAction) {
            if (!drawnOnce) {
                drawnOnce = true;
            }
            else {
                executingAction = true;
                UIManager.ActionBeforeUIDraw?.Invoke();
                UIManager.ActionBeforeUIDraw = null;
                executingAction = false;
                drawnOnce = false;
            }
        }
        // Run slower when not focused and nothing is happening
        if (!IsActive && !MainWindow.ForceFocus && !ProcessLoading.IsLoading)
            System.Threading.Thread.Sleep(1);
        
        if (MainWindow.ForceFocus)
            MainWindow.ForceFocus = false;
        
        renderingFrame = true;

        ImGuiRenderer.Update(gameTime);

        MainWindow.OnRenderFrame();

        ImGuiRenderer.Render();
        renderingFrame = false;

        base.Draw(gameTime);
    }
}