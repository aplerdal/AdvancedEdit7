using System;
using System.Diagnostics;
using AdvancedEdit.UI.Dialogs;
using AdvancedEdit.UI.Framework.Progress;
using AdvancedEdit.UI.Renderer;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TinyDialogsNet;

namespace AdvancedEdit.UI.Framework;

/// <summary>
/// The base app type. Only instantiate one per program.
/// </summary>
public class ImGuiApp : Game
{
    public static ImGuiApp Instance { get; private set; }
    public GraphicsDeviceManager Graphics;
    public SpriteBatch SpriteBatch;

    public ImGuiRenderer ImGuiRenderer;
    public MainWindow MainWindow;
    public ProcessLoading ProcessLoading;
    public ImGuiApp(MainWindow window)
    {
        Debug.Assert(Instance is null);
        Graphics = new GraphicsDeviceManager(this);
        Instance = this;
        MainWindow = window;

        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        
        window.Init(this);

        ProcessLoading = new ProcessLoading();
        ProcessLoading.OnUpdated += delegate {
            UpdateWindow();
        };
        Exiting += MainWindow.OnClosing;
        Window.FileDrop += (sender, args) => MainWindow.OnFileDrop(args.Files[0]);
        Window.ClientSizeChanged += (sender, args) => MainWindow.OnResize(Window.ClientBounds.Width, Window.ClientBounds.Height);
        Window.KeyDown += (sender, args) => MainWindow.OnKeyDown(args.Key);
    }
    private void UpdateWindow() {
        if (!ProcessLoading.IsLoading)
            return;
        Draw(new GameTime());
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        base.LoadContent();
    }

    protected override void Initialize() {
        ImGuiRenderer = new ImGuiRenderer(this);
        ImGuiRenderer.RebuildFontAtlas();
        MainWindow.OnApplicationLoad();
        base.Initialize();
    }

    bool renderingFrame, executingAction, drawnOnce;
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.White);
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
        ImGuiRenderer.BeforeLayout(gameTime);

        MainWindow.OnRenderFrame();

        ImGuiRenderer.AfterLayout();
        renderingFrame = false;

        base.Draw(gameTime);
    }
}