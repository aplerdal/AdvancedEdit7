using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace AdvancedEdit.Serialization;

public class GameGfx
{
    public Texture2D Texture
    {
        get
        {
            if (_cache is null) RegenCache();
            Debug.Assert(_cache is not null);
            return _cache;
        }
    }

    public Color[] Palette { get; set; }

    public byte[] Indices { get; set; }
    public IntPtr TexturePtr
    {
        get
        {
            if (_texturePtrCache == IntPtr.Zero) _texturePtrCache = AdvancedEdit.Instance.ImGuiRenderer.BindTexture(Texture);
            return _texturePtrCache;
        }
    }
    
    private Texture2D? _cache;
    private IntPtr _texturePtrCache = IntPtr.Zero;
    public GameGfx(byte[] indices, Color[] palette)
    {
        Indices = indices;
        Palette = palette;
    }

    public GameGfx()
    {
        Indices = [];
        Palette = [];
    }

    private void RegenCache()
    {
        var tileCount = Indices.Length / 64;
        var tempTexture = new Texture2D(AdvancedEdit.Instance.GraphicsDevice, tileCount*8, 8, false, SurfaceFormat.Color);
        Color[] colors = new Color[tileCount * 64];
        for (int tile = 0; tile < tileCount; tile++)
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                    colors[x + y * tileCount*8 + tile * 8] = Palette[Indices[x + y * 8 + tile * 64]% Palette.Length];
        tempTexture.SetData(colors);
        _cache = tempTexture;
    }

    public static byte[] IndicesFrom4Bpp(byte[] data)
    {
        byte[] indices = new byte[data.Length * 2];
        for (int i = 0; i < data.Length; i++)
        {
            indices[i * 2] = (byte)(data[i] & 0xF); // Lower 4 bits
            indices[i * 2 + 1] = (byte)((data[i] >> 4) & 0xF); // Upper 4 bits
        }

        return indices;
    }

    public static byte[] IndicesTo4Bpp(byte[] indices)
    {
        if (indices.Length % 2 != 0)
        {
            throw new ArgumentException("Index array must contain an even number of entries.");
        }

        byte[] data = new byte[indices.Length / 2];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)(indices[i * 2] | (indices[i * 2 + 1] << 4));
        }

        return data;
    }

    ~GameGfx()
    {
        if (_texturePtrCache != IntPtr.Zero)
            AdvancedEdit.Instance.ImGuiRenderer.UnbindTexture(_texturePtrCache);
    }
}