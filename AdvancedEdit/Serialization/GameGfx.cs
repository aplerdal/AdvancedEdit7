using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Hjg.Pngcs;
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
            if (_texturePtrCache == IntPtr.Zero)
                _texturePtrCache = AdvancedEdit.Instance.ImGuiRenderer.BindTexture(Texture);
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
    /// <summary>
    /// Loads game gfx from a png image. Supports indexed images
    /// </summary>
    /// <param name="path">Path to the png image</param>
    /// <returns>A new gamegfx instance with the image data</returns>
    /// <exception cref="ArgumentException">Error reading png</exception>
    public static GameGfx FromPng(string path)
    {
        Color[] palette;
        byte[] indices;
        if (Path.GetExtension(path) != ".png") throw new ArgumentException("Provided path is not a png image.");
        Stream fileStream = File.OpenRead(path);
        PngReader pngr = new PngReader(fileStream);
        if (pngr.ImgInfo.Indexed)
        {
            int width = pngr.ImgInfo.Cols;
            int height = pngr.ImgInfo.Rows;
            if (height % 8 != 0 || width % 8 != 0) throw new ArgumentException("Image size was not a multiple of 8");
            int tileWidth = width / 8;
            int tileHeight = height / 8;
            
            // Load Image palette
            var chunk = pngr.GetMetadata().GetPLTE();
            if (chunk == null) throw new ArgumentException("Error reading image palette");
            
            palette = new Color[chunk.GetNentries()];
            for (int i = 0; i < chunk.GetNentries(); i++)
            {
                int[] rgb = new int[3];
                chunk.GetEntryRgb(i, rgb);
                palette[i] = new Color(rgb[0], rgb[1], rgb[2]);
            }
            // Load image indicies
            if (pngr.ImgInfo.BitspPixel == 8)
            {
                indices = new byte[width * height];
                for (int tileY = 0; tileY < tileHeight; tileY++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        var line = pngr.ReadRowByte(tileY*8+y);
                        byte[] scanline = line.GetScanlineByte();
                        Debug.Assert(width == scanline.Length);
                        for (int tileX = 0; tileX < tileWidth; tileX++)
                        {
                            for (int x = 0; x < 8; x++)
                            {
                                indices[64 * (tileY * tileWidth + tileX) + y * 8 + x] = scanline[tileX * 8 + x];
                            }
                        }
                    }
                }

                return new GameGfx(indices, palette);
            }
            else
            {
                throw new ArgumentException("Image is not 8bpp. If you see this, please send the image to me so I can add the format.");
            }
        }
        else
        {
            // Not indexed image loading (hard)
            throw new ArgumentException("Only paletted images are supported currently.");
        }
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