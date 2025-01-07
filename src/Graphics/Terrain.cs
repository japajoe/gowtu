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
using OpenTK.Mathematics;

namespace Gowtu
{
    public enum TerrainHeightMode
    {
        Overwrite,
        Additive
    }

    public sealed class Terrain : Renderer
    {
        private Mesh mesh;
        private TerrainMaterial material;
        private Vector2 resolution;
        private Vector3 scale;
        private float maxHeight;

        public Vector2 Resolution
        {
            get
            {
                return resolution;
            }
        }

        public Vector3 Scale
        {
            get
            {
                return scale;
            }
        }

        public float MaxHeight
        {
            get
            {
                return maxHeight;
            }
            set
            {
                maxHeight = value;
            }
        }

        public Terrain() : base()
        {
            resolution = new Vector2(128, 128);
            scale = new Vector3(10, 10, 10);
            maxHeight = 128.0f;
        }

        internal override void OnInitializeComponent()
        {
            mesh = MeshGenerator.CreateTerrain((uint)resolution.X, (uint)resolution.Y, scale);
            material = new TerrainMaterial();
            Graphics.Add(this);
        }

        internal override void OnDestroyComponent()
        {
            Graphics.Remove(this);
            mesh.Delete();
        }

        public override Mesh GetMesh(int index)
        {
            //Ignore index since we only have a single mesh
            return mesh;
        }

        public TerrainMaterial GetMaterial()
        {
            return material;
        }

        internal override void OnRender()
        {
            if(!gameObject.isActive)
                return;

            Camera camera = Camera.mainCamera;

            if(camera == null || transform == null)
                return;

            bool ignoreCulling = (transform.gameObject.layer & (int)Layer.IgnoreCulling) > 0;

            if(mesh == null)
                return;

            if(mesh.VAO.Id == 0)
                return;
            
            if(material == null)
                return;
            
            if(material.Shader == null)
                return;

            //Mom can we have frustum culling?
            //Mom: we have frustum culling at home
            if(!ignoreCulling)
            {
                BoundingBox bounds = mesh.Bounds;
                bounds.Transform(transform.GetModelMatrix());

                if(!camera.frustum.Contains(bounds))
                    return;
            }

            GLState.DepthTest(true);
            GLState.CullFace(true);
            GLState.BlendMode(false);
            GLState.SetDepthFunc(DepthFunction.Less);

            material.Use(transform, camera);

            mesh.VAO.Bind();

            if(mesh.EBO.Id > 0)
                GL.DrawElements(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, mesh.IndiceCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
            else
                GL.DrawArrays(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, 0, mesh.VertexCount);
            
            mesh.VAO.Unbind();
        }

        /// <summary>
        /// Note: highest X and Y are Resolution.X + 1 and Resolution.Y + 1
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="height"></param>
        /// <param name="update"></param>
        /// <param name="mode"></param>
        public void SetHeight(int x, int y, float height, bool update = false, TerrainHeightMode mode = TerrainHeightMode.Overwrite)
        {
            int width = (int)resolution.X;

            var vertices = mesh.Vertices;
            int vertsPerRow = width + 1;
            int index = (y * vertsPerRow) + x;

            if(index < vertices.Length)
            {
                if(mode == TerrainHeightMode.Overwrite)
                {
                    if(height > maxHeight)
                        height = maxHeight;
                    vertices[index].position.Y = height;
                }
                else
                {
                    float newHeight = vertices[index].position.Y + height;
                    if(newHeight > maxHeight)
                        newHeight = maxHeight;
                    vertices[index].position.Y = newHeight;
                }

                if(update)
                    Update();
            }
        }

        public void Update()
        {
            if(mesh.VAO.Id > 0)
            {
                mesh.RecalculateNormals();
                mesh.Generate();
            }
        }
    }
}