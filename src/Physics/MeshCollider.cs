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
using BulletSharp;
using System.IO;

namespace Gowtu
{
    public sealed class MeshCollider : Collider
    {
        private TriangleIndexVertexArray indexVertexArrays;

        private Mesh m_mesh;

        public Mesh mesh
        {
            get { return m_mesh; }
            set 
            { 
                m_mesh = value;
                if(m_mesh != null)
                {                    
                    OnInitializeComponent();
                }
            }

        }

        public MeshCollider() : base()
        {

        }

        internal override void OnInitializeComponent()
        {
            if(m_mesh == null)
                return;

            var rigidbody = gameObject.GetComponent<Rigidbody>();

            if(rigidbody != null)
            {
                if (CreateMeshData())
                {
                    shape = new BvhTriangleMeshShape(indexVertexArrays, false);
                }
                else if(mesh != null)
                {
                    shape = new BvhTriangleMeshShape(indexVertexArrays, false);
                }

                rigidbody.OnInitializeComponent();                
            }
        }

        private bool CreateMeshData(Mesh mesh)
        {
            int totalVerts = mesh.Vertices.Length;
            int totalTriangles = mesh.Indices.Length / 3;
            int triangleIndexStride = 3 * sizeof(int);

            var indexMesh = new IndexedMesh();
            indexMesh.Allocate(totalTriangles, totalVerts, triangleIndexStride, 12);
            indexMesh.NumTriangles = totalTriangles;
            indexMesh.NumVertices = totalVerts;
            indexMesh.TriangleIndexStride = 3 * sizeof(int);
            indexMesh.VertexStride = 12;

            uint[] indices = new uint[mesh.Indices.Length];
            mesh.Indices.CopyTo(indices);

            using (var indicesStream = indexMesh.GetTriangleStream())
            {
                var ind = new BinaryWriter(indicesStream);
                
                for(int i = 0; i < indices.Length; i++)
                    ind.Write(indices[i]);

                ind.Dispose();
            }

            using (var vertexStream = indexMesh.GetVertexStream())
            {
                var verts = new BinaryWriter(vertexStream);
                for(int i = 0; i < mesh.Vertices.Length; i++)
                {
                    verts.Write(mesh.Vertices[i].position.X);
                    verts.Write(mesh.Vertices[i].position.Y);
                    verts.Write(mesh.Vertices[i].position.Z);
                }
                verts.Dispose();
            }


            indexVertexArrays = new TriangleIndexVertexArray();
            indexVertexArrays.AddIndexedMesh(indexMesh);

            return true;            
        }

        private bool CreateMeshData()
        {
            Renderer renderer = gameObject.GetComponentOfSubType<Renderer>();

            if(renderer == null)
            {
                Console.WriteLine("Can't create MeshCollider because there is no Renderer on this game object");
                return false;
            }

            if(renderer.GetMesh(0) == null)
            {
                Console.WriteLine("Can't create MeshCollider because the Renderer on this game object has no mesh");
                return false;
            }

            var mesh = renderer.GetMesh(0);

            int totalVerts = mesh.Vertices.Length;
            int totalTriangles = mesh.Indices.Length / 3;
            int triangleIndexStride = 3 * sizeof(int);

            var indexedMesh = new IndexedMesh();
            indexedMesh.Allocate(totalTriangles, totalVerts, triangleIndexStride, 12);
            indexedMesh.NumTriangles = totalTriangles;
            indexedMesh.NumVertices = totalVerts;
            indexedMesh.TriangleIndexStride = 3 * sizeof(int);
            indexedMesh.VertexStride = 12;

            uint[] indices = new uint[mesh.Indices.Length];
            mesh.Indices.CopyTo(indices);

            using (var indicesStream = indexedMesh.GetTriangleStream())
            {
                var ind = new BinaryWriter(indicesStream);
                
                for(int i = 0; i < indices.Length; i++)
                    ind.Write(indices[i]);

                ind.Dispose();
            }

            using (var vertexStream = indexedMesh.GetVertexStream())
            {
                var verts = new BinaryWriter(vertexStream);
                for(int i = 0; i < mesh.Vertices.Length; i++)
                {
                    verts.Write(mesh.Vertices[i].position.X);
                    verts.Write(mesh.Vertices[i].position.Y);
                    verts.Write(mesh.Vertices[i].position.Z);
                }
                verts.Dispose();
            }


            indexVertexArrays = new TriangleIndexVertexArray();
            indexVertexArrays.AddIndexedMesh(indexedMesh);

            return true;
        }
    }
}
