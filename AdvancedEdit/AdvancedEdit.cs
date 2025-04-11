using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Configuration;
using AdvancedEdit.Serialization;
using AdvancedEdit.UI.Renderer;
using Hexa.NET.ImGui;
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
    public const string Version = "1.0.0"; 
    
    public GraphicsDeviceManager Graphics;
    public SpriteBatch SpriteBatch;

    public Dictionary<string, ImTextureID> Icons;

    public ImGuiRenderer ImGuiRenderer;
    public TrackManager? TrackManager;

    public GameTime GameTime;

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
        var io = ImGui.GetIO();
        io.Fonts.AddFontFromFileTTF(Path.Combine("Content", "OpenSans-Regular.ttf"), 18);
        ImGuiRenderer.RebuildFontAtlas();
        //Theme.UpdateStyle();
        
        //UiManager = new UiManager();
        
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        Icons = new Dictionary<string, ImTextureID>
        {
            { "bucket", ImGuiRenderer.BindTexture(Content.Load<Texture2D>("bucket_fill")) },
            { "eraser", ImGuiRenderer.BindTexture(Content.Load<Texture2D>("eraser")) },
            { "pencil", ImGuiRenderer.BindTexture(Content.Load<Texture2D>("pencil")) },
            { "select", ImGuiRenderer.BindTexture(Content.Load<Texture2D>("select")) },
            { "wand", ImGuiRenderer.BindTexture(Content.Load<Texture2D>("wand")) },
            { "eyedropper", ImGuiRenderer.BindTexture(Content.Load<Texture2D>("picker")) },
            { "rectangle", ImGuiRenderer.BindTexture(Content.Load<Texture2D>("rectangle")) },
            { "move", ImGuiRenderer.BindTexture(Content.Load<Texture2D>("move")) },
        };
        

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GameTime = gameTime;
        GraphicsDevice.Clear(Color.CornflowerBlue);
        ImGuiRenderer.BeforeLayout(gameTime);
        
        ImGuiRenderer.AfterLayout();
        
        base.Draw(gameTime);
    }
}