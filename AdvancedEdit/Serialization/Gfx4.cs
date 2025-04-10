using System;
using System.Diagnostics;
using System.IO;
using Hexa.NET.ImGui;
using Hjg.Pngcs;
using Hjg.Pngcs.Chunks;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace AdvancedEdit.Serialization;

// TODO: Make gfx types inherit base class.

/// <summary>
/// A class representing 4bpp game graphics
/// </summary>
public class Gfx4{
    public byte[] Indices {get; set;}
    public Color[,] Palettes {get; set;}
    
    private int _activePalette;
    public int ActivePalette {
        get => _activePalette;
        set{
            _activePalette = value;
            _cache = GenerateTexture();
            if (_textureIDCache is not null)
                AdvancedEdit.Instance.ImGuiRenderer.UpdateTexture(_textureIDCache.Value, Texture);
        }
    }
    public Texture2D Texture {
        get {
            if (_cache is null) _cache = GenerateTexture();
            return _cache;
        }
    }
    protected Texture2D? _cache;
    public ImTextureID TexturePtr
    {
        get
        {
            if (_textureIDCache is null)
                _textureIDCache = AdvancedEdit.Instance.ImGuiRenderer.BindTexture(Texture);
            return _textureIDCache.Value;
        }
    }
    protected ImTextureID? _textureIDCache;

    public Gfx4(byte[] data, Color[,] palettes){
        Palettes = palettes;
        Indices = new byte[data.Length*2];
        for (int i = 0; i < data.Length; i++) {
            Indices[i*2] = (byte)(data[i]&0xf);
            Indices[i*2+1]=(byte)(data[i]>>4);
        }
    }
    public byte[] GetBytes(){
        byte[] bytes = new byte[Indices.Length/2];
        for (int i = 0; i < bytes.Length; i++) {
            bytes[i] = (byte)((Indices[i*2]&0xf) | (Indices[i*2+1]<<4));
        }
        return bytes;
    }
    public Texture2D GenerateTexture(){
        var tileCount = Indices.Length / 64;
        var tempTexture = new Texture2D(AdvancedEdit.Instance.GraphicsDevice, tileCount * 8, 8, false,
            SurfaceFormat.Color);
        Color[] colors = new Color[tileCount * 64];
        for (int tile = 0; tile < tileCount; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
            colors[x + y * tileCount * 8 + tile * 8] = Palettes[Indices[x + y * 8 + tile * 64] % 16, ActivePalette];
        tempTexture.SetData(colors);
        return tempTexture;
    }

    /// <summary>
    /// Loads game gfx from a png image. Supports indexed images
    /// </summary>
    /// <param name="path">Path to the png image</param>
    /// <returns>A new instance with the image data</returns>
    /// <exception cref="ArgumentException">Error reading png</exception>
    public static Gfx4 FromPng(string path)
    {
        byte[] indices;
        Color[,] palettes;
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

            var palLen = chunk.GetNentries();
            var palette = new Color[palLen];
            for (int i = 0; i < palLen; i++)
            {
                int[] rgb = new int[3];
                chunk.GetEntryRgb(i, rgb);
                palette[i] = new Color(rgb[0], rgb[1], rgb[2]);
            }
            palettes = new Color[16,(int)Math.Ceiling(palLen/16f)];
            for (int subPalette = 0; subPalette < Math.Ceiling(palLen/16f); subPalette++)
            for (int i = 0; i < 16; i++)
                palettes[i, subPalette] = (subPalette*16+i < palLen)?palette[subPalette*16+i] : Color.Black;
            
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

                byte[] data = new byte[indices.Length/2];
                for (int i = 0; i < data.Length; i++){
                    data[i] = (byte)((indices[i*2]&0xf) | (indices[i*2+1]<<4));
                }

                return new Gfx4(data, palettes);
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
    /// Exports an square, indexed PNG image from the gfx4 instance
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
        palette.SetNentries(Palettes.Length);
        for (int i = 0; i < Palettes.Length/16; i++)
        {
            for (int j = 0; j < 16; j++) {
                var col = Palettes[j,i];
                palette.SetEntry(i, col.R, col.G, col.B);
            }
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
    ~Gfx4() {
        if (_textureIDCache is not null)
            AdvancedEdit.Instance.ImGuiRenderer.UnbindTexture(_textureIDCache.Value);
    }
}