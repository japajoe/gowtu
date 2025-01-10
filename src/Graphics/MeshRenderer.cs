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
using OpenTK.Graphics.OpenGL;

namespace Gowtu
{
    public sealed class MeshRenderer : Renderer
    {
        private List<MeshRendererData> data;

        public MeshRenderer() : base()
        {
            data = new List<MeshRendererData>();
        }

        public void Add(Mesh mesh, Material material)
        {
            data.Add(new MeshRendererData(mesh, material));
        }

        public void SetMesh(Mesh mesh, int index)
        {
            if(data.Count == 0)
                return;

            if(index >= data.Count)
                return;

            data[index].mesh = mesh;
        }

        public override Mesh GetMesh(int index)
        {
            if(data.Count == 0)
                return null;

            if(index >= data.Count)
                return null;
            
            return data[index].mesh;
        }

        public void SetMaterial(Material material, int index)
        {
            if(data.Count == 0)
                return;

            if(index >= data.Count)
                return;
            
            data[index].material = material;
        }

        public T GetMaterial<T>(int index) where T : Material
        {
            if(data.Count == 0)
                return null;

            if(index >= data.Count)
                return null;

            return data[index].material as T;
        }

        public RenderSettings GetSettings(int index)
        {
            if(data.Count == 0)
                return null;

            if(index >= data.Count)
                return null;

            return data[index].settings;
        }

        internal override void OnRender()
        {
            if(!gameObject.isActive)
                return;

            if(data.Count == 0)
                return;
            
            Camera camera = Camera.mainCamera;

            if(camera == null || transform == null)
                return;

            bool ignoreCulling = (transform.gameObject.layer & (int)Layer.IgnoreCulling) > 0;

            for(int i = 0; i < data.Count; i++)
            {
                Mesh mesh = data[i].mesh;

                if(mesh == null)
                    continue;

                if(mesh.VAO.Id == 0)
                    continue;
                
                Material material = data[i].material;

                if(material == null)
                    continue;
                
                if(material.Shader == null)
                    continue;

                //Mom can we have frustum culling?
                //Mom: we have frustum culling at home
                if(!ignoreCulling)
                {
                    BoundingBox bounds = mesh.Bounds;
                    bounds.Transform(transform.GetModelMatrix());

                    if(!camera.frustum.Contains(bounds))
                        continue;
                }

                RenderSettings settings = data[i].settings;

                GLState.DepthTest(settings.depthTest);
                GLState.CullFace(settings.cullFace);
                GLState.BlendMode(settings.alphaBlend);
                GLState.SetDepthFunc(settings.depthFunc);

                material.Use(transform, camera);

                mesh.VAO.Bind();

                if(mesh.EBO.Id > 0)
                    GL.DrawElements(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, mesh.IndiceCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
                else
                    GL.DrawArrays(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, 0, mesh.VertexCount);
                
                mesh.VAO.Unbind();
            }
        }

        internal override void OnRender(Material material)
        {
            if(!gameObject.isActive)
                return;

            if(data.Count == 0)
                return;
            
            Camera camera = Camera.mainCamera;

            if(camera == null || transform == null)
                return;

            if(material == null)
                return;
            
            if(material.Shader == null)
                return;

            for(int i = 0; i < data.Count; i++)
            {
                Mesh mesh = data[i].mesh;

                if(mesh == null)
                    continue;

                if(mesh.VAO.Id == 0)
                    continue;

                RenderSettings settings = data[i].settings;

                GLState.DepthTest(settings.depthTest);
                GLState.CullFace(settings.cullFace);
                GLState.BlendMode(settings.alphaBlend);
                GLState.SetDepthFunc(settings.depthFunc);

                material.Use(transform, camera);

                mesh.VAO.Bind();

                if(mesh.EBO.Id > 0)
                    GL.DrawElements(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, mesh.IndiceCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
                else
                    GL.DrawArrays(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, 0, mesh.VertexCount);
                
                mesh.VAO.Unbind();
            }
        }
    }

    public class MeshRendererData
    {
        public Mesh mesh;
        public Material material;
        public RenderSettings settings;
        
        public MeshRendererData(Mesh mesh, Material material)
        {
            this.mesh = mesh;
            this.material = material;
            settings = new RenderSettings();
        }
    }
}