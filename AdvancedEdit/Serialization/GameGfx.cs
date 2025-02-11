using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdvancedEdit.Serialization;

// TODO: Change this to only allow tile accessability instead of single texture. If an atlas is needed (eg tilesets) make a seperate class
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
    
    public void RegenCache()
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

    ~GameGfx()
    {
        if (_texturePtrCache != IntPtr.Zero)
            AdvancedEdit.Instance.ImGuiRenderer.UnbindTexture(_texturePtrCache);
    }
}