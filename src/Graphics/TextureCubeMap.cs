using System;
using OpenTK.Graphics.OpenGL;

namespace Gowtu
{
    public sealed class TextureCubeMap : Texture
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

        public TextureCubeMap(uint width, uint height, Color color) : base()
        {
            this.width = width;
            this.height = height;

            uint channels = 4;
            uint size = width * height * channels;
            byte[] data = new byte[size];

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
            GL.BindTexture(TextureTarget.TextureCubeMap, id);

            for(int i = 0; i < 6; i++)
            {
                TextureTarget target = (TextureTarget)((int)TextureTarget.TextureCubeMapPositiveX + i);
                GL.TexImage2D(target, 0, InternalFormat.Rgba, (int)width, (int)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            }

            GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            
            GL.PixelStorei(PixelStoreParameter.UnpackAlignment, 1);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
        }

        /// <summary>
        /// Creates a cubemap from given images.
        /// </summary>
        /// <param name="images">A collection of 6 images. Order: right, left, bottom, top, front, back</param>
        /// <returns></returns>
        public TextureCubeMap(ReadOnlySpan<Image> images) : base()
        {
            if(images.Length != 6)
                throw new ArgumentException("Constructor expects 6 images but got " + images.Length);

            GL.GenTextures(1, ref id);
            GL.BindTexture(TextureTarget.TextureCubeMap, id);

            for(int i = 0; i < images.Length; i++)
            {
                Image image = images[i];

                ReadOnlySpan<byte> data = new ReadOnlySpan<byte>(image.Data);
                this.width = image.Width;
                this.height = image.Height;
                
                if(data.Length > 0)
                {
                    TextureTarget target = (TextureTarget)((int)TextureTarget.TextureCubeMapPositiveX + i);

                    switch(image.Channels)
                    {
                        case 1:
                        {
                            GL.TexImage2D(target, 0, InternalFormat.Rgba, (int)image.Width, (int)image.Height, 0, PixelFormat.Red, PixelType.UnsignedByte, data);    
                            break;
                        }
                        case 2:
                        {
                            GL.TexImage2D(target, 0, InternalFormat.Rgba, (int)image.Width, (int)image.Height, 0, PixelFormat.Rg, PixelType.UnsignedByte, data);
                            break;
                        }
                        case 3:
                        {
                            GL.TexImage2D(target, 0, InternalFormat.Rgb, (int)image.Width, (int)image.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, data);
                            break;
                        }
                        case 4:
                        {
                            GL.TexImage2D(target, 0, InternalFormat.Rgba, (int)image.Width, (int)image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
                            break;
                        }
                        default:
                        {
                            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
                            GL.DeleteTextures(1, id);
                            id  = 0;
                            string error = "Failed to load texture: Unsupported number of channels: " + image.Channels;
                            throw new ArgumentException(error);
                        }
                    }
                }
                else 
                {
                    GL.BindTexture(TextureTarget.TextureCubeMap, 0);
                    GL.DeleteTextures(1, id);
                    id = 0;
                    throw new ArgumentException("Failed to load texture: No valid data passed");
                }
            }

            GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            
            GL.PixelStorei(PixelStoreParameter.UnpackAlignment, 1);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
        }

        public override void Bind(int unit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + (uint)unit);
            GL.BindTexture( TextureTarget.TextureCubeMap, id);
        }

        public override void Unbind()
        {
            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
        }
    }
}