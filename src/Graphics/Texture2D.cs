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
    public sealed class Texture2D : Texture
    {
        private uint width;
        private uint height;

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

        public Texture2D(Image image) : base()
        {
            byte[] data = image.Data;

            if(data != null)
            {
                width = image.Width;
                height = image.Height;

                GL.GenTextures(1, ref id);
                GL.BindTexture(TextureTarget.Texture2d, id);

                GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.PixelStorei(PixelStoreParameter.UnpackAlignment, 1);

                uint channels = image.Channels;

                switch(channels)
                {
                    case 1:
                    {
                        GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, (int)image.Width, (int)image.Height, 0, PixelFormat.Red, PixelType.UnsignedByte, data);    
                        break;
                    }
                    case 2:
                    {
                        GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, (int)image.Width, (int)image.Height, 0, PixelFormat.Rg, PixelType.UnsignedByte, data);
                        break;
                    }
                    case 3:
                    {
                        GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, (int)image.Width, (int)image.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, data);
                        break;
                    }
                    case 4:
                    {
                        GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, (int)image.Width, (int)image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
                        break;
                    }
                    default:
                    {
                        GL.BindTexture(TextureTarget.Texture2d, 0);
                        GL.DeleteTextures(1, id);
                        id = 0;
                        string error = "Failed to load texture: Unsupported number of channels: " + image.Channels;
                        throw new Exception(error);
                    }
                }
                
                GL.GenerateMipmap(TextureTarget.Texture2d);
                GL.BindTexture(TextureTarget.Texture2d, 0);
            } 
            else 
            {
                throw new Exception("Failed to load texture: No valid data passed");
            }
        }

        public Texture2D(uint width, uint height, Color color) : base()
        {
            this.width = 0;
            this.height = 0;

            if(width == 0 || height == 0)
                throw new Exception("Failed to load texture: Texture width and height must be greater than 0");

            uint channels = 4;
            uint size = width * height * channels;
            byte[] data = new byte[size];

            if(data != null)
            {
                this.width = width;
                this.height = height;

                for(uint i = 0; i < size; i += channels)
                {
                    byte r = (byte)(Math.Clamp(color.r * 255, 0.0, 255.0));
                    byte g = (byte)(Math.Clamp(color.g * 255, 0.0, 255.0));
                    byte b = (byte)(Math.Clamp(color.b * 255, 0.0, 255.0));
                    byte a = (byte)(Math.Clamp(color.a * 255, 0.0, 255.0));

                    data[i+0] = r;
                    data[i+1] = g;
                    data[i+2] = b;
                    data[i+3] = a;
                }

                GL.GenTextures(1, ref id);
                GL.BindTexture(TextureTarget.Texture2d, id);

                GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, (int)width, (int)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
                
                GL.GenerateMipmap(TextureTarget.Texture2d);
                GL.BindTexture(TextureTarget.Texture2d, 0);

            } 
            else 
            {
                throw new Exception("Failed to load texture: Failed to allocate memory");
            }
        }

        public Texture2D(int id, uint width, uint height)
        {
            this.id = id;
            this.width = width;
            this.height = height;
        }

        public override void Bind(int unit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + (uint)unit);
            GL.BindTexture( TextureTarget.Texture2d, id);
        }

        public override void Unbind()
        {
            GL.BindTexture(TextureTarget.Texture2d, 0);
        }
    }
}