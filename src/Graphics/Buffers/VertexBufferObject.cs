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
using OpenTK.Graphics.OpenGL;

namespace Gowtu
{
    public sealed class VertexBufferObject : BufferObject
    {
        public VertexBufferObject() : base()
        {

        }

        public override void Generate()
        {
            GL.GenBuffers(1, ref id);
        }

        public override void Delete()
        {
            if(id > 0)
            {
                GL.DeleteBuffers(1, id);
                id = 0;
            }
        }
        
        public override void Bind()
        {
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, id);
        }

        public override void Unbind()
        {
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        }

        public void BufferData<T>(List<T> data, BufferUsageARB usage) where T : unmanaged
        {
            ReadOnlySpan<T> span = CollectionsMarshal.AsSpan(data);
            BufferData<T>(span, usage);
        }

        public void BufferData<T>(ReadOnlySpan<T> data, BufferUsageARB usage) where T : unmanaged
        {            
            GL.BufferData(BufferTargetARB.ArrayBuffer, data, usage);
        }

        public void BufferSubData<T>(List<T> data, int offset) where T : unmanaged
        {
            ReadOnlySpan<T> span = CollectionsMarshal.AsSpan(data);
            BufferSubData(span, offset);
        }

        public void BufferSubData<T>(ReadOnlySpan<T> data, int offset) where T : unmanaged
        {
            GL.BufferSubData(BufferTargetARB.ArrayBuffer, new IntPtr(offset), data);
        }
    }
}