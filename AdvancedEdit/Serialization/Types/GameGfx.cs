using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdvancedEdit.Serialization.Types;

public class GameGfx(Point size, byte[] indicies, Color[] palette)
{
    public Texture2D Texture
    {
        get
        {
            if (_cache is null) RegenCache();
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
    
    private Texture2D _cache;
    private IntPtr _texturePtrCache = IntPtr.Zero;
    
    public void RegenCache()
    {
        var tempTexture =
            new Texture2D(AdvancedEdit.Instance.GraphicsDevice, size.X, size.Y, false, SurfaceFormat.Color);
        Color[] colors = new Color[size.X * size.Y];
        for (int tile = 0; tile < (size.X * size.Y)/64; tile++)
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                    colors[x + y * 256*8 + tile * 8] = palette[indicies[x + y * 8 + tile * 64]];
        tempTexture.SetData(colors);
        _cache = tempTexture;
    }

    ~GameGfx()
    {
        if (_texturePtrCache != IntPtr.Zero)
            AdvancedEdit.Instance.ImGuiRenderer.UnbindTexture(_texturePtrCache);
    }
}