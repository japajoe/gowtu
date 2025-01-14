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
using OpenTK.Mathematics;

namespace Gowtu
{
    public sealed class BatchRenderer : Renderer
    {
        private Mesh m_mesh;
        private VertexBufferObject m_instanceVBO;
        private Material m_material;
        private RenderSettings m_settings;
        private uint m_instanceCount;
        private uint m_maxInstances;
        private List<BatchRendererInstanceData> m_instanceData;

        public uint InstanceCount
        {
            get
            {
                return m_instanceCount;
            }
            set
            {
                m_instanceCount = value;
                if(m_instanceCount > m_maxInstances)
                    m_instanceCount = m_maxInstances;
            }
        }

        public uint MaxInstances
        {
            get
            {
                return m_maxInstances;
            }
        }

        public BatchRenderer() : base()
        {
            m_settings = new RenderSettings();
            m_instanceCount = 0;
            //m_maxInstances = 1048576; // 2 ^ 20
            m_maxInstances = 100000; // 2 ^ 20
            m_instanceVBO = new VertexBufferObject();
            m_instanceData = new List<BatchRendererInstanceData>(new BatchRendererInstanceData[m_maxInstances]);
        }

        public void SetInstanceData(int index, Vector3 translation, Quaternion rotation, Vector3 scale, Color color)
        {
            if(m_instanceVBO.Id == 0)
                return;

            if(index < 0 || index >= m_instanceCount)
                return;

            Matrix4 t = Matrix4.CreateTranslation(translation);
            Matrix4 r = Matrix4.CreateFromQuaternion(rotation);
            Matrix4 s = Matrix4.CreateScale(scale);

            var data = m_instanceData[index];
            data.matrix = s * r * t;
            data.color = color;
            m_instanceData[index] = data;

            var slice = CollectionsMarshal.AsSpan(m_instanceData).Slice(index, 1);

            m_instanceVBO.Bind();
            m_instanceVBO.BufferSubData<BatchRendererInstanceData>(slice, index * Marshal.SizeOf<BatchRendererInstanceData>());
            m_instanceVBO.Unbind();
        }

        public BatchRendererInstanceData GetInstanceData(int index)
        {
            if(m_instanceVBO.Id == 0)
                return default(BatchRendererInstanceData);

            if(index < 0 || index >= m_instanceCount)
                return default(BatchRendererInstanceData);
            
            return m_instanceData[index];
        }

        public void SetMesh(Mesh mesh)
        {
            this.m_mesh = mesh;

            if(m_instanceVBO.Id == 0 && m_mesh.VAO.Id > 0)
            {
                m_mesh.VAO.Bind();
                
                m_instanceVBO.Generate();
                m_instanceVBO.Bind();
                m_instanceVBO.BufferData<BatchRendererInstanceData>(m_instanceData, BufferUsageARB.DynamicDraw);

                m_mesh.VAO.EnableVertexAttribArray(3);
                m_mesh.VAO.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<BatchRendererInstanceData>(), new IntPtr(0 * Marshal.SizeOf<Vector4>()));
                
                m_mesh.VAO.EnableVertexAttribArray(4);
                m_mesh.VAO.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<BatchRendererInstanceData>(), new IntPtr(1 * Marshal.SizeOf<Vector4>()));
                
                m_mesh.VAO.EnableVertexAttribArray(5);
                m_mesh.VAO.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<BatchRendererInstanceData>(), new IntPtr(2 * Marshal.SizeOf<Vector4>()));
                
                m_mesh.VAO.EnableVertexAttribArray(6);
                m_mesh.VAO.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<BatchRendererInstanceData>(), new IntPtr(3 * Marshal.SizeOf<Vector4>()));
                
                m_mesh.VAO.EnableVertexAttribArray(7);
                m_mesh.VAO.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<BatchRendererInstanceData>(), new IntPtr(4 * Marshal.SizeOf<Color>()));

                m_mesh.VAO.VertexAttribDivisor(3, 1);
                m_mesh.VAO.VertexAttribDivisor(4, 1);
                m_mesh.VAO.VertexAttribDivisor(5, 1);
                m_mesh.VAO.VertexAttribDivisor(6, 1);
                m_mesh.VAO.VertexAttribDivisor(7, 1);
                
                m_mesh.VAO.Unbind();
                
                m_instanceVBO.Unbind();
            }
        }

        public override Mesh GetMesh(int index)
        {
            //Just to keep the API behaving like expected
            if (index != 0)
                return null;

            return m_mesh;
        }

        public void SetMaterial(Material material)
        {
            this.m_material = material;
        }

        public T GetMaterial<T>() where T : Material
        {
            return m_material as T;
        }

        public RenderSettings GetSettings()
        {
            return m_settings;
        }

        internal override void OnRender()
        {
            if (!gameObject.isActive)
                return;

            if(m_instanceCount == 0)
                return;

            Camera camera = Camera.mainCamera;

            if (camera == null || transform == null)
                return;

            if (m_mesh == null)
                return;

            if (m_mesh.VAO.Id == 0)
                return;

            if (m_material == null)
                return;

            if (m_material.Shader == null)
                return;

            GLState.DepthTest(m_settings.depthTest);
            GLState.CullFace(m_settings.cullFace);
            GLState.BlendMode(m_settings.alphaBlend);
            GLState.SetDepthFunc(m_settings.depthFunc);

            m_material.Use(transform, camera);

            m_mesh.VAO.Bind();

            if (m_mesh.EBO.Id > 0)
                GL.DrawElementsInstanced(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, m_mesh.IndiceCount, DrawElementsType.UnsignedInt, IntPtr.Zero, (int)m_instanceCount);
            else
                GL.DrawArraysInstanced(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, 0, m_mesh.VertexCount, (int)m_instanceCount);

            m_mesh.VAO.Unbind();
        }

        internal override void OnRender(Material material)
        {
            if (!gameObject.isActive)
                return;

            if(m_instanceCount == 0)
                return;

            Camera camera = Camera.mainCamera;

            if (camera == null || transform == null)
                return;

            if (m_mesh == null)
                return;

            if (m_mesh.VAO.Id == 0)
                return;

            if (material == null)
                return;

            if (material.Shader == null)
                return;

            GLState.DepthTest(m_settings.depthTest);
            GLState.CullFace(m_settings.cullFace);
            GLState.BlendMode(m_settings.alphaBlend);
            GLState.SetDepthFunc(m_settings.depthFunc);

            material.Use(transform, camera);

            m_mesh.VAO.Bind();

            if (m_mesh.EBO.Id > 0)
                GL.DrawElementsInstanced(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, m_mesh.IndiceCount, DrawElementsType.UnsignedInt, IntPtr.Zero, (int)m_instanceCount);
            else
                GL.DrawArraysInstanced(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, 0, m_mesh.VertexCount, (int)m_instanceCount);

            m_mesh.VAO.Unbind();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BatchRendererInstanceData
    {
        public Matrix4 matrix;
        public Color color;
        
        public BatchRendererInstanceData()
        {
            matrix = Matrix4.Identity;
            color = Color.White;
        }
    }
}