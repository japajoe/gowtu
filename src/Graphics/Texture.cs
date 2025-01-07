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
    public abstract class Texture
    {
        protected int id;

        public int Id
        {
            get
            {
                return id;
            }
        }

        public IntPtr Handle
        {
            get
            {
                return new IntPtr(id);
            }
        }

        public Texture()
        {
            this.id = 0;
        }

        public virtual void Bind(int unit)
        {

        }

        public virtual void Unbind()
        {

        }

        public void Delete()
        {
            if(id > 0)
            {
                GL.DeleteTextures(1, id);
                id = 0;
            }
        }
    }
}