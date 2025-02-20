using System;
using System.Diagnostics;
using System.IO;
using Hjg.Pngcs;
using Hjg.Pngcs.Chunks;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace AdvancedEdit.Serialization;

/// <summary>
/// A class representing 8bpp game graphics
/// </summary>
public class Gfx8{
    public byte[] Indices {get; set;}
    public Color[] Palette {get; set;}
    public Texture2D Texture {
        get {
            if (_cache is null) _cache = GenerateTexture();
            return _cache;
        }
    }
    protected Texture2D? _cache;

    public IntPtr TexturePtr
    {
        get
        {
            if (_texturePtrCache is null)
                _texturePtrCache = AdvancedEdit.Instance.ImGuiRenderer.BindTexture(Texture);
            return _texturePtrCache.Value;
        }
    }
    protected IntPtr? _texturePtrCache;
    public Gfx8(byte[] data, Color[] palette){
        Palette = palette;
        Indices = data;
    }

    public byte[] GetBytes(){
        return Indices;
    }
    public Texture2D GenerateTexture(){
        var tileCount = Indices.Length / 64;
        var tempTexture = new Texture2D(AdvancedEdit.Instance.GraphicsDevice, tileCount * 8, 8, false,
            SurfaceFormat.Color);
        Color[] colors = new Color[tileCount * 64];
        for (int tile = 0; tile < tileCount; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
            colors[x + y * tileCount * 8 + tile * 8] = Palette[Indices[x + y * 8 + tile * 64] % Palette.Length];
        tempTexture.SetData(colors);
        return tempTexture;
    }

    /// <summary>
    /// Loads game gfx from a png image. Supports indexed images
    /// </summary>
    /// <param name="path">Path to the png image</param>
    /// <returns>A new Gfx8 instance with the image data</returns>
    /// <exception cref="ArgumentException">Error reading png</exception>
    public static Gfx8 FromPng(string path)
    {
        byte[] indices;
        Color[] palette;
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
                        var line = pngr.ReadRowByte(tileY * 8 + y);
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

                return new Gfx8(indices, palette);
            }
            else
            {
                throw new ArgumentException(
                    "Image is not 8bpp. If you see this, please send the image to me so I can add the format.");
            }
        }
        else
        {
            // Not indexed image loading (hard)
            throw new ArgumentException("Only paletted images are supported currently.");
        }
    }

    /// <summary>
    /// Exports an square, indexed PNG image from the gfx8 instance
    /// </summary>
    /// <param name="path">Path to the save location</param>
    public void ExportPng(string path)
    {
        // Exports in a square for now
        int width, height;
        var sqrt = Math.Sqrt(Indices.Length);
        if (sqrt%1f == 0f){
            width = (int)sqrt;
            height = (int)sqrt;
        } else {
            width = Indices.Length/8;
            height = 8;
        }
        var imageInfo = new ImageInfo(width, height, 8, false, false, true);
        var stream = File.OpenWrite(path);
        PngWriter pngw = new PngWriter(stream, imageInfo);
        pngw.CompLevel = 9;
        PngChunkPLTE palette = new PngChunkPLTE(imageInfo);
        palette.SetNentries(Palette.Length);
        for (int i = 0; i < Palette.Length; i++)
        {
            var col = Palette[i];
            palette.SetEntry(i, col.R, col.G, col.B);
        }
        pngw.GetMetadata().QueueChunk(palette);
        for (int tileY = 0; tileY < height / 8; tileY++)
        for (int y = 0; y < 8; y++)
        {
            ImageLine line = new ImageLine(imageInfo);
            for (int tileX = 0; tileX < width / 8; tileX++)
            for (int x = 0; x < 8; x++)
                line.Scanline[tileX * 8 + x] = Indices[64 * (tileY * (width / 8) + tileX) + y * 8 + x];
            pngw.WriteRow(line, tileY*8+y);
        }
        pngw.End();
    }

    ~Gfx8() {
        if (_texturePtrCache is not null)
            AdvancedEdit.Instance.ImGuiRenderer.UnbindTexture(_texturePtrCache.Value);
    }
}