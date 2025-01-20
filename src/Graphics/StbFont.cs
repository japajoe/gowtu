using System;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace Gowtu
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
    public class TestingPurposeAttribute : Attribute
    {
        public string Message { get; }

        public TestingPurposeAttribute(string message)
        {
            Message = message;
        }
    }

    // private structure
    [StructLayout(LayoutKind.Sequential)]
    public struct stbtt__buf
    {
        public IntPtr data;
        public int cursor;
        public int size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct stbtt_bakedchar
    {
        public ushort x0,y0,x1,y1; // coordinates of bbox in bitmap
        public float xoff,yoff,xadvance;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct stbtt_packedchar
    {
        public ushort x0,y0,x1,y1; // coordinates of bbox in bitmap
        public float xoff,yoff,xadvance;
        public float xoff2,yoff2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct stbtt_aligned_quad
    {
        public float x0,y0,s0,t0; // top-left
        public float x1,y1,s1,t1; // bottom-right
    }

    [StructLayout(LayoutKind.Sequential)]
    struct stbtt_pack_context 
    {
        IntPtr user_allocator_context;
        IntPtr pack_info;
        int   width;
        int   height;
        int   stride_in_bytes;
        int   padding;
        int   skip_missing;
        uint   h_oversample, v_oversample;
        IntPtr pixels;
        IntPtr nodes;
    }

    // The following structure is defined publicly so you can declare one on
    // the stack or as a global or etc, but you should treat it as opaque.
    [StructLayout(LayoutKind.Sequential)]
    public struct stbtt_fontinfo
    {
        public IntPtr userdata;
        public IntPtr data;              // pointer to .ttf file
        public int fontstart;         // offset of start of font
        public int numGlyphs;                     // number of glyphs, needed for range checking
        public int loca,head,glyf,hhea,hmtx,kern,gpos,svg; // table locations as offset from start of .ttf
        public int index_map;                     // a cmap mapping for our chosen character encoding
        public int indexToLocFormat;              // format needed to map from glyph index to glyph
        public stbtt__buf cff;                    // cff font data
        public stbtt__buf charstrings;            // the charstring index
        public stbtt__buf gsubrs;                 // global charstring subroutines index
        public stbtt__buf subrs;                  // private charstring subroutines index
        public stbtt__buf fontdicts;              // array of font dicts
        public stbtt__buf fdselect;               // map from glyph to fontdict
    }

    [TestingPurpose("Do not use, it's for testing")]
    public sealed class StbFont
    {
        private stbtt_fontinfo fontInfo;
        private int pixelSize;
        private int textureId;
        private int codePointOfFirstChar;
        private int textureWidth;
        private int textureHeight;
        private float lineHeight;
        private int maxHeight;
        private stbtt_packedchar[] packedChars;
        private stbtt_aligned_quad[] alignedQuads;
        private byte[] fontAtlasTextureData;

        public int TextureId
        {
            get => textureId;
        }

        public StbFont()
        {
            textureId = 0;
            pixelSize = 14;
            textureWidth = 1024;
            textureHeight = 1024;
        }

        public bool Load(string filepath, int pixelSize)
        {
            if(textureId > 0) //already loaded
                return false;
            
            this.pixelSize = pixelSize;
            textureId = 0;
            return LoadFromFile(filepath);
        }

        public bool Load(ReadOnlySpan<byte> fontData, int pixelSize)
        {
            if(textureId > 0) //already loaded
                return false;
            
            this.pixelSize = pixelSize;
            textureId = 0;
            return LoadFromMemory(fontData);
        }

        
        public void Delete()
        {
            if(textureId > 0) 
            {
                GL.DeleteTexture(textureId);
                textureId = 0;
            }
        }
        
        private bool LoadFromFile(string filepath)
        {
            if(!File.Exists(filepath))
            {
                return false;
            }

            var data = File.ReadAllBytes(filepath);

            return Load(data);
        }
        
        private bool LoadFromMemory(ReadOnlySpan<byte> fontData)
        {
            if(fontData == null)
            {
                return false;
            }

            return Load(fontData);
        }
        
        private unsafe bool Load(ReadOnlySpan<byte> data)
        {
            int fontCount = 0;

            fixed(byte *pData = &data[0])
            {
                fontCount = stbtt_GetNumberOfFonts(pData);
            }

            float scale = 1.0f;

            fixed(stbtt_fontinfo *pFontInfo = &fontInfo)
            {
                fixed(byte *pData = &data[0])
                {
                    if(stbtt_InitFont(pFontInfo, pData, 0) == 0) 
                    {
                        return false;
                    }
                }

                scale = stbtt_ScaleForPixelHeight(pFontInfo, (float)pixelSize);

                int ascent, descent, lineGap;
                stbtt_GetFontVMetrics(pFontInfo, &ascent, &descent, &lineGap);

                // Calculate the maximum height
                maxHeight = ascent - descent + lineGap;
            }

            const int fontAtlasWidth = 1024;
            const int fontAtlasHeight = 1024;

            textureWidth = fontAtlasWidth;
            textureHeight = fontAtlasHeight;

            codePointOfFirstChar = 32;                     // ASCII of ' '(Space)
            const int charsToIncludeInFontAtlas = 95; // Include 95 charecters

            fontAtlasTextureData = new byte[fontAtlasWidth * fontAtlasHeight];
            packedChars = new stbtt_packedchar[charsToIncludeInFontAtlas];
            alignedQuads = new stbtt_aligned_quad[charsToIncludeInFontAtlas];

            float fontSize = (float)pixelSize;
            
            stbtt_pack_context ctx = new stbtt_pack_context();

            fixed(byte *pFontAtlasTextureData = &fontAtlasTextureData[0])
            fixed(stbtt_packedchar *pPackedChars = &packedChars[0])
            fixed(byte *pData = &data[0])
            fixed(stbtt_aligned_quad * pAlignedQuads = &alignedQuads[0])
            {
                stbtt_PackBegin(
                    &ctx,                                     // stbtt_pack_context (this call will initialize it) 
                    pFontAtlasTextureData,                    // Font Atlas texture data
                    fontAtlasWidth,                           // Width of the font atlas texture
                    fontAtlasHeight,                          // Height of the font atlas texture
                    0,                                        // Stride in bytes
                    1,                                        // Padding between the glyphs
                    null);


                stbtt_PackFontRange(
                    &ctx,                                     // stbtt_pack_context
                    pData,                                     // Font Atlas texture data
                    0,                                        // Font Index                                 
                    fontSize,                                 // Size of font in pixels. (Use STBTT_POINT_SIZE(fontSize) to use points) 
                    codePointOfFirstChar,                     // Code point of the first charecter
                    charsToIncludeInFontAtlas,                // No. of charecters to be included in the font atlas 
                    pPackedChars                        // stbtt_packedchar array, this struct will contain the data to render a glyph
                );

                stbtt_PackEnd(&ctx);

                for (int i = 0; i < charsToIncludeInFontAtlas; i++) {
                    float unusedX, unusedY;

                    stbtt_GetPackedQuad(
                        pPackedChars,                  // Array of stbtt_packedchar
                        fontAtlasWidth,                      // Width of the font atlas texture
                        fontAtlasHeight,                     // Height of the font atlas texture
                        i,                                   // Index of the glyph
                        &unusedX, &unusedY,                  // current position of the glyph in screen pixel coordinates, (not required as we have a different corrdinate system)
                        &pAlignedQuads[i],                   // stbtt_alligned_quad struct. (this struct mainly consists of the texture coordinates)
                        0                                    // Allign X and Y position to a integer (doesn't matter because we are not using 'unusedX' and 'unusedY')
                    );
                }
            }

            for (int i = 0; i < packedChars.Length; i++) 
            {
                var glyph = packedChars[i];

                float glyphHeight = (glyph.y1 - glyph.y0);
                
                if (glyphHeight > lineHeight) 
                {
                    lineHeight = glyphHeight;
                }
            }



            return true;
        }

        public bool GenerateTexture() 
        {
            if(textureId > 0) 
            {
                Console.WriteLine("Could not generate texture because it is already generated");
                return false;
            }

            if(fontAtlasTextureData == null)
            {
                Console.WriteLine("Could not generate texture because there is no texture data");
                return false;
            }

            if(fontAtlasTextureData.Length == 0)
            {
                Console.WriteLine("Could not generate texture because there is no texture data");
                return false;
            }

            const Int32 textureWidth = 1024;
            const Int32 textureHeight = 1024;

            GL.GenTextures(1, ref textureId);
            GL.BindTexture(TextureTarget.Texture2d, textureId);
            
            GL.PixelStorei(PixelStoreParameter.UnpackAlignment, 1);

            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.R8, textureWidth, textureHeight, 0, PixelFormat.Red, PixelType.UnsignedByte, fontAtlasTextureData);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            //GL.GenerateMipmap(TextureTarget.Texture2d);

            GL.ObjectLabel(ObjectIdentifier.Texture, (uint)textureId, -1, "StbFontAtlas");

            GL.BindTexture(TextureTarget.Texture2d, 0);

            return textureId > 0;
        }

        private const string libName = "stbtt";

        [DllImport(libName, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int stbtt_GetNumberOfFonts(byte *data);

        [DllImport(libName, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int stbtt_InitFont(stbtt_fontinfo *info, byte *data, int offset);

        [DllImport(libName, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int stbtt_PackBegin(stbtt_pack_context *spc, byte *pixels, int pw, int ph, int stride_in_bytes, int padding, void *alloc_context);

        [DllImport(libName, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int stbtt_PackFontRange(stbtt_pack_context *spc, byte *fontdata, int font_index, float font_size, int first_unicode_codepoint_in_range, int num_chars_in_range, stbtt_packedchar *chardata_for_range);

        [DllImport(libName, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe void stbtt_PackEnd(stbtt_pack_context *spc);

        [DllImport(libName, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe void stbtt_GetPackedQuad(stbtt_packedchar *chardata, int pw, int ph, int char_index, float *xpos, float *ypos, stbtt_aligned_quad *q, int align_to_integer);

        [DllImport(libName, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe void stbtt_GetFontVMetrics(stbtt_fontinfo *info, int *ascent, int *descent, int *lineGap);

        [DllImport(libName, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe float stbtt_ScaleForPixelHeight(stbtt_fontinfo *info, float height);
    }
}