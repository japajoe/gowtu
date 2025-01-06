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
using StbImageSharp;

namespace Gowtu
{
    public sealed class Image
    {
        private byte[] data;
        private uint width;
        private uint height;
        private uint channels;
        private bool isLoaded;

        public byte[] Data
        {
            get
            {
                return data;
            }
        }

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

        public uint Channels
        {
            get
            {
                return channels;
            }
        }

        public uint DataSize
        {
            get
            {
                return width * height * channels;
            }
        }

        public bool IsLoaded
        {
            get
            {
                return isLoaded;
            }
        }

        public Image()
        {
            this.data = null;
            this.width = 0;
            this.height = 0;
            this.channels = 0;
            this.isLoaded = false;
        }

        public Image(string filepath)
        {
            this.data = null;
            this.width = 0;
            this.height = 0;
            this.channels = 0;
            this.isLoaded = false;

            if(LoadFromFile(filepath))
            {
                this.isLoaded = true;
            }
        }

        public Image(byte[] compressedData)
        {
            this.isLoaded = false;
            this.data = null;
            this.width = 0;
            this.height = 0;
            this.channels = 0;

            if(LoadFromMemory(compressedData))
            {
                this.isLoaded = true;
            }
        }

        public Image(byte[] uncompressedData, uint width, uint height, uint channels)
        {
            this.width = width;
            this.height = height;
            this.channels = channels;
            this.data = new byte[uncompressedData.Length];
            Buffer.BlockCopy(uncompressedData, 0, this.data, 0, uncompressedData.Length);
            this.isLoaded = true;
        }

        public Image(uint width, uint height, uint channels, Color color)
        {
            this.isLoaded = false;
            this.data = null;
            this.width = width;
            this.height = height;
            this.channels = channels;

            if(Load(width, height, channels, color))
            {
                this.isLoaded = true;
            }
        }

        private bool LoadFromFile(string filepath)
        {
            if (!System.IO.File.Exists(filepath))
            {
                Console.WriteLine("File does not exist: " + filepath);
                return false;
            }

            using (var stream = System.IO.File.OpenRead(filepath))
            {
                try
                {
                    ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                    width = (uint)image.Width;
                    height = (uint)image.Height;
                    channels = (uint)image.Comp;
                    data = image.Data;
                    return true;
                }
                catch(System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }

        private bool LoadFromMemory(byte[] compressedImageData)
        {
            try
            {
                ImageResult image = ImageResult.FromMemory(compressedImageData, ColorComponents.RedGreenBlueAlpha);
                width = (uint)image.Width;
                height = (uint)image.Height;
                channels = (uint)image.Comp;
                data = image.Data;
                return true;
            }
            catch(System.Exception ex)
            {
                width = 0;
                height = 0;
                channels = 0;
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private bool Load(uint width, uint height, uint channels, Color color)
        {
            if(channels < 3 || channels > 4)
                return false;

            uint size = width * height * channels;

            if(size == 0)
                return false;

            this.data = new byte[size];

            if(channels == 3)
            {
                for(int i = 0; i < size; i += 3)
                {
                    byte r = (byte)(Math.Clamp(color.r * 255, 0.0, 255.0));
                    byte g = (byte)(Math.Clamp(color.g * 255, 0.0, 255.0));
                    byte b = (byte)(Math.Clamp(color.b * 255, 0.0, 255.0));

                    data[i+0] = r;
                    data[i+1] = g;
                    data[i+2] = b;
                }
            }
            else
            {
                for(int i = 0; i < size; i += 4)
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
            }
            
            return true;
        }
    }
}