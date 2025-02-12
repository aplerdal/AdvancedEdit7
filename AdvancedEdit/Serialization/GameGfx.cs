using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdvancedEdit.Serialization;

// TODO: Change this to only allow tile accessability instead of single texture. If an atlas is needed (eg tilesets) make a inheritor class
public class GameGfx(byte[] indicies, Color[] palette)
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

    public Color[] Palette => palette;

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
    
    private void RegenCache()
    {
        var tileCount = indicies.Length / 64;
        var tempTexture = new Texture2D(AdvancedEdit.Instance.GraphicsDevice, tileCount*8, 8, false, SurfaceFormat.Color);
        Color[] colors = new Color[tileCount * 64];
        for (int tile = 0; tile < tileCount; tile++)
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                    colors[x + y * tileCount*8 + tile * 8] = palette[indicies[x + y * 8 + tile * 64]%palette.Length];
        tempTexture.SetData(colors);
        _cache = tempTexture;
    }

    public byte[] GetIndicies()
    {
        int tileCount = Texture.Width / 8;
        byte[] indices = new byte[tileCount * 64];

        Color[] colors = new Color[tileCount * 64];
        Texture.GetData(colors);

        for (int tile = 0; tile < tileCount; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            Color color = colors[x + y * tileCount * 8 + tile * 8];
            int paletteIndex = Array.IndexOf(palette, color);
            indices[x + y * 8 + tile * 64] = (byte)(paletteIndex >= 0 ? paletteIndex : 0); // Default to 0 if not found
        }
        return indices;
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