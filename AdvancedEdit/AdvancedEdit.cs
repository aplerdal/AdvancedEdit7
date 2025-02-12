using System;
using System.IO;
using System.Diagnostics;
using System.Configuration;
using AdvancedEdit.Serialization;
using AdvancedEdit.UI;
using AdvancedEdit.UI.Undo;
using AdvancedEdit.UI.Windows;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AdvancedEdit;

public class AdvancedEdit : Game
{
    public static AdvancedEdit Instance { get; private set; } = null!;

    /// <summary>
    /// Program Version
    /// </summary>
    public const string Version = "0.1.0"; 
    
    public GraphicsDeviceManager Graphics;
    public SpriteBatch SpriteBatch;

    public ImGuiRenderer ImGuiRenderer;
    public UiManager UiManager;
    public TrackManager TrackManager;

    #pragma warning disable CS8618
    public AdvancedEdit()
    {
        Instance = this;
        Debug.Assert(Instance is not null);
        Graphics = new GraphicsDeviceManager(this);
        
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
    }
    #pragma warning restore CS8618

    protected override void Initialize()
    {
        ImGuiRenderer = new ImGuiRenderer(this);
        ImGuiRenderer.RebuildFontAtlas();
        UiManager = new UiManager();
        
        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        //var track = new Track(
        //    new BinaryReader(File.OpenRead("/home/aplerdal/Development/Mksc/mksc.gba")), 27, 0x0000, 0x29FC74);
        //UiManager.AddWindow(new MapEditor(track));
        //UiManager.AddWindow(new AiEditor(track));
        UiManager.AddWindow(new TrackSelector());

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        ImGuiRenderer.BeforeLayout(gameTime);

        UiManager.DrawWindows();
        
        ImGuiRenderer.AfterLayout();
        
        base.Draw(gameTime);
    }
}