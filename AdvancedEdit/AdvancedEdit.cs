using System;
using AdvancedEdit.UI;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AdvancedEdit;

public class AdvancedEdit : Game
{
    /// <summary>
    /// Program Version (Following the semver versioning system)
    /// </summary>
    public const string Version = "0.1.0"; 
    
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private ImGuiRenderer _imGuiRenderer;
    
    private UiManager _uiManager;

    public AdvancedEdit()
    {
        _graphics = new GraphicsDeviceManager(this);
        
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        _imGuiRenderer = new ImGuiRenderer(this);
        _imGuiRenderer.RebuildFontAtlas();
        _uiManager = new UiManager();
        
        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _imGuiRenderer.BeforeLayout(gameTime);
        // TODO: Add your drawing code here

        _uiManager.DrawWindows(this);
        
        _imGuiRenderer.AfterLayout();
        
        base.Draw(gameTime);
    }
}