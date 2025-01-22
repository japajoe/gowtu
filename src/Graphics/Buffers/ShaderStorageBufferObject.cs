using System;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Gowtu
{
    public sealed class ShaderStorageBufferObject : BufferObject
    {
        public override void Generate()
        {
            id = GL.CreateBuffer();
        }

        public override void Delete()
        {
            if(id > 0)
            {
                GL.DeleteBuffer(id);
                id = 0;
            }
        }

        public override void Bind()
        {
            GL.BindBuffer(BufferTargetARB.ShaderStorageBuffer, id);
        }

        public override void Unbind()
        {
            GL.BindBuffer(BufferTargetARB.ShaderStorageBuffer, 0);
        }

        public void BindBufferBase(uint index)
        {
            GL.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, index, id);
        }

        public void BufferData<T>(List<T> data, BufferUsageARB usage) where T : unmanaged
        {
            ReadOnlySpan<T> span = CollectionsMarshal.AsSpan(data);
            BufferData<T>(span, usage);
        }

        public void BufferData<T>(ReadOnlySpan<T> data, BufferUsageARB usage) where T : unmanaged
        {            
            GL.BufferData(BufferTargetARB.ShaderStorageBuffer, data, usage);
        }

        public void BufferSubData<T>(List<T> data, int offset) where T : unmanaged
        {
            ReadOnlySpan<T> span = CollectionsMarshal.AsSpan(data);
            BufferSubData(span, offset);
        }

        public void BufferSubData<T>(ReadOnlySpan<T> data, int offset) where T : unmanaged
        {
            GL.BufferSubData(BufferTargetARB.ShaderStorageBuffer, new IntPtr(offset), data);
        }
    }
}