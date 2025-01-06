using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdvancedEdit.Serialization.Types;

public class Tilemap
{
    public RenderTarget2D TrackTexture;
    public byte[,] Layout;
    private Texture2D _tileset;

    public Tilemap(Point trackSize, Texture2D tileset, byte[] indicies)
    {
        _tileset = tileset;
        if (trackSize.X * trackSize.Y != indicies.Length)
            throw new InvalidDataException("Track size does not match indicies size");
        Layout = new byte[trackSize.X, trackSize.Y];
        
        // Read the track in
        for (int y = 0; y<trackSize.Y; y++)
        for (int x = 0; x < trackSize.X; x++)
            Layout[x, y] = indicies[x + y * trackSize.Y];
        TrackTexture = new RenderTarget2D(AdvancedEdit.Instance.GraphicsDevice, trackSize.X * 8, trackSize.Y * 8, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        AdvancedEdit.Instance.GraphicsDevice.SetRenderTarget(TrackTexture);
        AdvancedEdit.Instance.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
        for (int y = 0; y < trackSize.Y; y++)
        {
            for (int x = 0; x < trackSize.X; x++)
            {
                AdvancedEdit.Instance.SpriteBatch.Draw(_tileset, new Vector2(x * 8, y * 8),
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
            AdvancedEdit.Instance.SpriteBatch.Draw(_tileset, new Vector2(tile.Key.X * 8, tile.Key.Y * 8),
                new Rectangle(tile.Value * 8, 0, 8, 8), Color.White);
        }
        AdvancedEdit.Instance.SpriteBatch.End();
        AdvancedEdit.Instance.GraphicsDevice.SetRenderTarget(null);
    }

    public void DrawTile(Point position, byte value)
    {
        AdvancedEdit.Instance.GraphicsDevice.SetRenderTarget(TrackTexture);
        AdvancedEdit.Instance.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque,
            samplerState: SamplerState.PointClamp);
        AdvancedEdit.Instance.SpriteBatch.Draw(_tileset, new Vector2(position.X * 8, position.Y * 8),
            new Rectangle(value * 8, 0, 8, 8), Color.White);
        AdvancedEdit.Instance.SpriteBatch.End();
        AdvancedEdit.Instance.GraphicsDevice.SetRenderTarget(null);
    }
}