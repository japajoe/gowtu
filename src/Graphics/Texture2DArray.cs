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
using OpenTK.Graphics.OpenGL;

namespace Gowtu
{
    public sealed class Texture2DArray : Texture
    {
        private uint width;
        private uint height;
        private uint depth;

        public uint Width
        {
            get
            {
                return width;
            }
        }

        public uint Height
        {
            get
            {
                return height;
            }
        }

        public uint Depth
        {
            get
            {
                return depth;
            }
        }

        public Texture2DArray(uint width, uint height, uint depth) : base()
        {
            this.width = width;
            this.height = height;
            this.depth = depth;

            GL.GenTextures(1, ref id);
            GL.BindTexture(TextureTarget.Texture2dArray, id);
            GL.TexImage3D(TextureTarget.Texture2dArray, 0, InternalFormat.DepthComponent32f, (int)width, (int)height, (int)depth, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            
            GL.TexParameteri(TextureTarget.Texture2dArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameteri(TextureTarget.Texture2dArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameteri(TextureTarget.Texture2dArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameteri(TextureTarget.Texture2dArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            
            unsafe
            {
                Color bordercolor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                float *pBorderColor = &bordercolor.r;
                GL.TexParameterfv(TextureTarget.Texture2dArray, TextureParameterName.TextureBorderColor, pBorderColor);
            }
            
            GL.BindTexture(TextureTarget.Texture2dArray, 0);
        }

        public override void Bind(int unit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + (uint)unit);
            GL.BindTexture( TextureTarget.Texture2dArray, id);
        }

        public override void Unbind()
        {
            GL.BindTexture(TextureTarget.Texture2dArray, 0);
        }

        public Texture2D GetTexture2DAtLevel(uint level)
        {
            if(id == 0)
                return null;

            if(level >= depth)
                return null;

            int texture = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2dArray, id);

            // Create a temporary buffer to hold the pixel data
            byte[] pixelData = new byte[width * height * 4]; // Assuming RGBA format

            // Copy the specific layer from the texture array
            GL.CopyImageSubData((uint)id, CopyImageSubDataTarget.Texture2dArray, 0, 0, 0, (int)level,
                                (uint)texture, CopyImageSubDataTarget.Texture2d, 0, 0, 0, 0,
                                (int)width, (int)height, (int)depth);

            GL.BindTexture(TextureTarget.Texture2dArray, id);

            return new Texture2D(texture, width, height);
        }
    }
}