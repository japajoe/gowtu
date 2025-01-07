// MIT License

// Copyright (c) 2025 W.M.R Jap-A-Joe

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FreeTypeSharp;
using OpenTK.Graphics.OpenGL;
using static FreeTypeSharp.FT;
using static FreeTypeSharp.FT_LOAD;
using static FreeTypeSharp.FT_Render_Mode_;

namespace Gowtu
{
    public unsafe sealed class Font
    {
        private Int32 m_textureId;
        private UInt32 m_pixelSize;
        private UInt32 m_maxHeight;
        private UInt32 m_lineHeight;
        private UInt32 m_codePointOfFirstChar;
        private List<Glyph> m_glyphs;
        private List<byte> m_textureData;
        private static Dictionary<string,Font> fonts = new Dictionary<string, Font>();

        public UInt32 PixelSize
        {
            get => m_pixelSize;
        }

        public UInt32 MaxHeight
        {
            get => m_maxHeight;
        }

        public Int32 TextureId
        {
            get => m_textureId;
        }

        UInt32 CodePointOfFirstChar
        {
            get => m_codePointOfFirstChar;
        }

        float LineHeight
        {
            get => (float)m_lineHeight;
        }

        public Font()
        {
            m_textureId = 0;
            m_pixelSize = 0;
            m_maxHeight = 0;
            m_lineHeight = 0;
            m_codePointOfFirstChar = 0;
            m_glyphs = new List<Glyph>();
            m_textureData = new List<byte>();
        }

        public void Delete()
        {
            if(m_textureId > 0) 
            {
                GL.DeleteTextures(1, m_textureId);
                m_textureId = 0;
            }
            
            if(m_textureData.Count > 0) 
            {
                m_textureData.Clear();
            }
        }

        public bool LoadFromFile(string filepath, uint pixelSize)
        {
            if(m_textureData.Count > 0) {
                Console.WriteLine("Could not load font because texture is already generated");
                return false;
            }

            this.m_pixelSize = pixelSize;

            FT_LibraryRec_ *library = null;

            if (FT_Init_FreeType(&library) != FT_Error.FT_Err_Ok) 
            {
                Console.WriteLine("Could not init FreeType library");
                return false;
            }

            FT_FaceRec_ *fontFace;

            if (FT_New_Face(library, (byte*)Marshal.StringToHGlobalAnsi(filepath), 0, &fontFace) != FT_Error.FT_Err_Ok) 
            {
                FT_Done_FreeType(library);
                Console.WriteLine("Could not load font");
                return false;
            }

            bool result = Load(fontFace);

            FT_Done_Face(fontFace);
            FT_Done_FreeType(library);

            return result;
        }
        
        public bool LoadFromMemory(ReadOnlySpan<byte> data, int dataSize, uint pixelSize)
        {
            if(m_textureData.Count > 0) 
            {
                Console.WriteLine("Could not load font because texture is already generated");
                return false;
            }

            this.m_pixelSize = pixelSize;

            FT_LibraryRec_ *library;
            
            if (FT_Init_FreeType(&library) != FT_Error.FT_Err_Ok) 
            {
                Console.WriteLine("Could not init FreeType library");
                return false;
            }

            FT_FaceRec_ *fontFace;
            
            fixed(byte *pData = &data[0])
            {
                if (FT_New_Memory_Face(library, pData, dataSize, 0, &fontFace) != FT_Error.FT_Err_Ok) 
                {
                    FT_Done_FreeType(library);
                    Console.WriteLine("Could not load font");
                    return false;
                }
            }

            bool result = Load(fontFace);

            FT_Done_Face(fontFace);
            FT_Done_FreeType(library);

            return result;
        }

        public bool GenerateTexture() 
        {
            if(m_textureId > 0) 
            {
                Console.WriteLine("Could not generate texture because it is already generated");
                return false;
            }

            if(m_textureData == null)
            {
                Console.WriteLine("Could not generate texture because there is no texture data");
                return false;
            }

            if(m_textureData.Count == 0)
            {
                Console.WriteLine("Could not generate texture because there is no texture data");
                return false;
            }

            const Int32 textureWidth = 1024;
            const Int32 textureHeight = 1024;

            ReadOnlySpan<byte> pTextureData = CollectionsMarshal.AsSpan(m_textureData);

            GL.PixelStorei(PixelStoreParameter.UnpackAlignment, 1);

            GL.GenTextures(1, ref m_textureId);
            GL.BindTexture(TextureTarget.Texture2d, m_textureId);
            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.R8, textureWidth, textureHeight, 0, PixelFormat.Red, PixelType.UnsignedByte, pTextureData);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.GenerateMipmap(TextureTarget.Texture2d);

            GL.ObjectLabel(ObjectIdentifier.Texture, (uint)m_textureId, -1, "FontAtlas");

            GL.BindTexture(TextureTarget.Texture2d, 0);

            return m_textureId > 0;
        }

        public bool GetGlyph(char c, out Glyph glyph) 
        {
            glyph = default(Glyph);

            UInt32 index = (c - m_codePointOfFirstChar);
            if(index >= m_glyphs.Count)
                return false;

            glyph = m_glyphs[(Int32)index];
            return true;
        }

        public float GetPixelScale(float fontSize)
        {
            return fontSize / m_pixelSize;
        }

        public void CalculateBounds(string text, Int32 size, float fontSize, out float width, out float height) 
        {
            width = 0;
            height = 0;

            Int32 maxHeight = 0; // Maximum height of any character
            Int32 currentLineWidth = 0; // Width of the current line
            UInt32 currentHeight = 0;
            UInt32 lineCount = 1; // Count of lines

            for(Int32 i = 0; i < size; i++) 
            {
                char c = text[i];

                if (c == '\n') 
                {
                    // End of a line
                    if (currentLineWidth > width) 
                    {
                        width = (float)currentLineWidth;
                    }

                    currentHeight += m_maxHeight; //Nore that this is the global max height

                    currentLineWidth = 0; // Reset for the next line
                    lineCount++; // Increment line count
                    continue;
                }

                if(!GetGlyph(c, out Glyph glyph))
                    continue;

                // Accumulate the width using the advanceX of the glyph
                currentLineWidth += glyph.advanceX;

                // Update the maximum height based on the glyph's height
                Int32 glyphHeight = glyph.height;
                if (glyphHeight > maxHeight) 
                {
                    maxHeight = glyphHeight;
                }
            }

            // Check the last line
            if (currentLineWidth > width) 
            {
                width = (float)currentLineWidth;
            }

            if(lineCount > 1)
                height = currentHeight + maxHeight;
            else
                height = maxHeight;

            width *= GetPixelScale(fontSize);
            height *= GetPixelScale(fontSize);
        }

        public void CalculateCharacterPosition(string text, Int32 size, Int32 characterIndex, float fontSize, out float x, out float y) 
        {
            x = 0;
            y = 0;

            if(size == 0)
                return;

            float startPosX = x;
            float startPosY = y;
            float characterPosX = x;
            float characterPosY = y;

            // Calculate the character position based on the character index
            for (int i = 0; i < characterIndex; ++i) 
            {
                char ch = text[i];

                // Handle line breaks
                if (ch == '\n') 
                {
                    characterPosX = startPosX; // Reset X position for a new line
                    characterPosY += m_maxHeight * GetPixelScale(fontSize);
                    continue;
                }

                if(!GetGlyph(ch, out Glyph glyph))
                    continue;

                // Update the character position based on the glyph's x advance
                characterPosX += glyph.advanceX * GetPixelScale(fontSize);
            }

            x = characterPosX;
            y = characterPosY;
        }

        public float CalculateYOffset(string text, Int32 size, float fontSize)
        {
            float height = 0.0f;
            float yOffset = 0.0f;
            
            for(Int32 i = 0; i < size; i++) 
            {
                if(text[i] == '\n')
                    break;

                if(!GetGlyph(text[i], out Glyph glyph))
                    continue;
                
                float h = glyph.bearingY;

                if(h > height) 
                {
                    height = h;
                    yOffset = (glyph.bearingY - glyph.bottomBearing) * GetPixelScale(fontSize);
                }
            }

            return yOffset;
        }

        private bool Load(FT_FaceRec_ *fontFace) 
        {
            FT_Set_Pixel_Sizes(fontFace, 0, m_pixelSize);
        
            const UInt32 textureWidth = 1024;
            const UInt32 textureHeight = 1024;
            const UInt32 padding = 2;
            UInt32 row = 0;
            UInt32 col = 0;
            UInt64 height = 0;

            Int32 LOAD_RENDER = (int)FT_LOAD_RENDER;
            Int32 RENDER_MODE_SDF = (int)FT_RENDER_MODE_SDF;
            Int32 flags = LOAD_RENDER | RENDER_MODE_SDF;
            FT_LOAD loadFlags = (FT_LOAD)flags;

            m_textureData = new List<byte>(new byte[textureWidth * textureHeight]);

            m_codePointOfFirstChar = 32;
            const UInt32 charsToIncludeInFontAtlas = 95;
            UInt64 start = (UInt64)m_codePointOfFirstChar;
            UInt64 end = start + (UInt64)charsToIncludeInFontAtlas;

            m_glyphs = new List<Glyph>(new Glyph[charsToIncludeInFontAtlas]);

            Int32 index = 0;
            Int64 lineHeight = 0;
            UInt32 maxRowHeight = 0;

            for(UInt64 glyphIdx = start; glyphIdx < end; glyphIdx++) 
            {
                if(FT_Load_Char(fontFace, new nuint(glyphIdx), loadFlags) != FT_Error.FT_Err_Ok)
                    continue;

                if(FT_Render_Glyph(fontFace->glyph, FT_RENDER_MODE_NORMAL) != FT_Error.FT_Err_Ok)
                    continue;

                if(fontFace->glyph->bitmap.rows > maxRowHeight)
                    maxRowHeight = fontFace->glyph->bitmap.rows;

                if(col + fontFace->glyph->bitmap.width + padding >= textureWidth) 
                {
                    col = padding;
                    row += maxRowHeight + padding;
                    maxRowHeight = 0;
                }

                m_maxHeight = (UInt32)Math.Max((UInt64)(fontFace->size->metrics.ascender - fontFace->size->metrics.descender) >> 6, height);

                for(UInt32 y = 0; y < fontFace->glyph->bitmap.rows; y++) 
                {
                    for(UInt32 x = 0; x < fontFace->glyph->bitmap.width; x++) 
                    {
                        UInt32 indexA = (row + y) * textureWidth + col + x;
                        UInt32 indexB = y * fontFace->glyph->bitmap.width + x;
                        m_textureData[(int)indexA] = fontFace->glyph->bitmap.buffer[indexB];
                    }
                }

                Glyph glyph = m_glyphs[index];
                glyph.sizeX = (Int32)fontFace->glyph->bitmap.width;
                glyph.sizeY = (Int32)fontFace->glyph->bitmap.rows;
                glyph.advanceX = (Int32)fontFace->glyph->advance.x >> 6;
                glyph.advanceY = (Int32)fontFace->glyph->advance.y >> 6;
                glyph.bearingX = fontFace->glyph->bitmap_left;
                glyph.bearingY = fontFace->glyph->bitmap_top;
                glyph.height = (Int32)fontFace->glyph->metrics.height >> 6;
                glyph.bottomBearing = (Int32)(fontFace->glyph->bitmap.rows - fontFace->glyph->bitmap_top);
                glyph.leftBearing = (Int32)(fontFace->glyph->bitmap.width - fontFace->glyph->bitmap_left);

                lineHeight = Math.Max(glyph.height, lineHeight);

                glyph.u0 = (float)col / textureWidth;
                glyph.v0 = (float)row / textureHeight;
                glyph.u1 = (float)(col + fontFace->glyph->bitmap.width) / textureWidth;
                glyph.v1 = (float)(row + fontFace->glyph->bitmap.rows) / textureHeight;

                m_glyphs[index] = glyph;

                index++;

                col += fontFace->glyph->bitmap.width + padding;
            }

            this.m_lineHeight = (UInt32)lineHeight;

            return true;
        }
    }

    public struct Glyph 
    {
        public Int32 sizeX, sizeY;
        public Int32 advanceX, advanceY;
        public Int32 bearingX, bearingY;
        public Int32 height;
        public Int32 bottomBearing;
        public Int32 leftBearing;
        public float u0, v0;
        public float u1, v1;
    }
}