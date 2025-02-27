using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdvancedEdit.Serialization;

public class Tilemap
{
    public RenderTarget2D TrackTexture;
    public byte[,] Layout;
    public Texture2D Tileset;

    public IntPtr TexturePtr
    {
        get
        {
            if (_texturePtrCache == IntPtr.Zero)
                _texturePtrCache = AdvancedEdit.Instance.ImGuiRenderer.BindTexture(TrackTexture);
            return _texturePtrCache;
        }
    }
    private IntPtr _texturePtrCache = IntPtr.Zero;

    public Tilemap(Point trackSize, Texture2D tileset, byte[] indices)
    {
        Tileset = tileset;
        //if (trackSize.X * trackSize.Y != indices.Length)
            //throw new InvalidDataException("Track size does not match indices size");
        Layout = new byte[trackSize.X, trackSize.Y];
        
        // Read the track in
        for (int y = 0; y < trackSize.Y; y++)
        for (int x = 0; x < trackSize.X; x++)
            Layout[x, y] = indices[x + y * trackSize.Y];
        TrackTexture = new RenderTarget2D(AdvancedEdit.Instance.GraphicsDevice, trackSize.X * 8, trackSize.Y * 8, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        AdvancedEdit.Instance.GraphicsDevice.SetRenderTarget(TrackTexture);
        AdvancedEdit.Instance.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
        for (int y = 0; y < trackSize.Y; y++)
        {
            for (int x = 0; x < trackSize.X; x++)
            {
                AdvancedEdit.Instance.SpriteBatch.Draw(Tileset, new Vector2(x * 8, y * 8),
                    new Rectangle(Layout[x, y] * 8, 0, 8, 8), Color.White);
            }
        }

        AdvancedEdit.Instance.SpriteBatch.End();
        AdvancedEdit.Instance.GraphicsDevice.SetRenderTarget(null);
    }

    public void RegenMap()
    {
        if (TrackTexture.Width != Layout.GetLength(0) || TrackTexture.Height != Layout.GetLength(1))
        {
            TrackTexture = new RenderTarget2D(AdvancedEdit.Instance.GraphicsDevice, Layout.GetLength(0) * 8, Layout.GetLength(1) * 8, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            AdvancedEdit.Instance.ImGuiRenderer.UpdateTexture(_texturePtrCache, TrackTexture);
        }
        AdvancedEdit.Instance.GraphicsDevice.SetRenderTarget(TrackTexture);
        AdvancedEdit.Instance.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp);
        for (int y = 0; y < Layout.GetLength(1); y++)
        {
            for (int x = 0; x < Layout.GetLength(0); x++)
            {
                AdvancedEdit.Instance.SpriteBatch.Draw(Tileset, new Vector2(x * 8, y * 8),
                    new Rectangle(Layout[x, y] * 8, 0, 8, 8), Color.White);
            }
        }

        AdvancedEdit.Instance.SpriteBatch.End();
        AdvancedEdit.Instance.GraphicsDevice.SetRenderTarget(null);
    }

    public void SetTiles(Dictionary<Point,byte> tiles)
    { 
        AdvancedEdit.Instance.GraphicsDevice.SetRenderTarget(TrackTexture);
        AdvancedEdit.Instance.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque,
            samplerState: SamplerState.PointClamp);
        foreach (var tile in tiles)
        {
            Layout[tile.Key.X, tile.Key.Y] = tile.Value;
            AdvancedEdit.Instance.SpriteBatch.Draw(Tileset, new Vector2(tile.Key.X * 8, tile.Key.Y * 8),
                new Rectangle(tile.Value * 8, 0, 8, 8), Color.White);
        }
        AdvancedEdit.Instance.SpriteBatch.End();
        AdvancedEdit.Instance.GraphicsDevice.SetRenderTarget(null);
    }

    public void SetTiles(HashSet<Point> positions, byte tile)
    {
        AdvancedEdit.Instance.GraphicsDevice.SetRenderTarget(TrackTexture);
        AdvancedEdit.Instance.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque,
            samplerState: SamplerState.PointClamp);
        foreach (var pos in positions)
        {
            Layout[pos.X, pos.Y] = tile;
            AdvancedEdit.Instance.SpriteBatch.Draw(Tileset, new Vector2(pos.X * 8, pos.Y * 8),
                new Rectangle(tile * 8, 0, 8, 8), Color.White);
        }

        AdvancedEdit.Instance.SpriteBatch.End();
        AdvancedEdit.Instance.GraphicsDevice.SetRenderTarget(null);
    }

    public void SetTiles(Rectangle area, byte tile)
    {
        AdvancedEdit.Instance.GraphicsDevice.SetRenderTarget(TrackTexture);
        AdvancedEdit.Instance.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque,
            samplerState: SamplerState.PointClamp);
        for (int y = area.Top; y < area.Bottom; y++)
        for (int x = area.Left; x < area.Right; x++)
        {
            Layout[x, y] = tile;
            AdvancedEdit.Instance.SpriteBatch.Draw(Tileset, new Vector2(x * 8, y * 8),
                new Rectangle(tile * 8, 0, 8, 8), Color.White);
        }
        AdvancedEdit.Instance.SpriteBatch.End();
        AdvancedEdit.Instance.GraphicsDevice.SetRenderTarget(null);
    }

    public void SetTiles(byte[,] data, Point position)
    {
        AdvancedEdit.Instance.GraphicsDevice.SetRenderTarget(TrackTexture);
        AdvancedEdit.Instance.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque,
            samplerState: SamplerState.PointClamp);
        for (int y = position.Y; y < position.Y + data.GetLength(1); y++)
        for (int x = position.X; x < position.X + data.GetLength(0); x++)
        {
            Layout[x, y] = data[x - position.X,y - position.Y];
            AdvancedEdit.Instance.SpriteBatch.Draw(Tileset, new Vector2(x * 8, y * 8),
                new Rectangle(data[x - position.X, y - position.Y] * 8, 0, 8, 8), Color.White);
        }

        AdvancedEdit.Instance.SpriteBatch.End();
        AdvancedEdit.Instance.GraphicsDevice.SetRenderTarget(null);
    }

    public void DrawTile(Point position, byte value)
    {
        AdvancedEdit.Instance.GraphicsDevice.SetRenderTarget(TrackTexture);
        AdvancedEdit.Instance.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque,
            samplerState: SamplerState.PointClamp);
        AdvancedEdit.Instance.SpriteBatch.Draw(Tileset, new Vector2(position.X * 8, position.Y * 8),
            new Rectangle(value * 8, 0, 8, 8), Color.White);
        AdvancedEdit.Instance.SpriteBatch.End();
        AdvancedEdit.Instance.GraphicsDevice.SetRenderTarget(null);
    }

    ~Tilemap()
    {
        if (_texturePtrCache != IntPtr.Zero)
            AdvancedEdit.Instance.ImGuiRenderer.UnbindTexture(_texturePtrCache);
    }
}