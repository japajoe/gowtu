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
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;
        
        public Vertex()
        {
            position = new Vector3(0, 0, 0);
            normal = new Vector3(0, 0, 0);
            uv = new Vector2(0, 0);
        }

        public Vertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            this.position = position;
            this.normal = normal;
            this.uv = uv;
        }
    }

    public sealed class Mesh
    {
        private Vertex[] vertices;
        private uint[] indices;
        private int sizeOfVertices;
        private int sizeOfIndices;
        private VertexArrayObject vao;
        private VertexBufferObject vbo;
        private ElementBufferObject ebo;
        private BoundingBox bounds;

        public int VertexCount
        {
            get
            {
                return vertices.Length;
            }
        }

        public int IndiceCount
        {
            get
            {
                return indices.Length;
            }
        }

        public Span<Vertex> Vertices
        {
            get
            {
                return new Span<Vertex>(vertices);
            }
        }

        public Span<uint> Indices
        {
            get
            {
                return new Span<uint>(indices);
            }
        }

        public VertexArrayObject VAO
        {
            get
            {
                return vao;
            }
        }

        public VertexBufferObject VBO
        {
            get
            {
                return vbo;
            }
        }

        public ElementBufferObject EBO
        {
            get
            {
                return ebo;
            }
        }

        public BoundingBox Bounds
        {
            get
            {
                return bounds;
            }
        }

        public Mesh()
        {
            sizeOfVertices = 0;
            sizeOfIndices = 0;
            vao = new VertexArrayObject();
            vbo = new VertexBufferObject();
            ebo = new ElementBufferObject();
            bounds = new BoundingBox();
        }

        public Mesh(Vertex[] vertices, uint[] indices, bool calculateNormals)
        {
            this.vertices = vertices;
            this.indices = indices;

            sizeOfVertices = this.vertices.Length;
            sizeOfIndices = this.indices != null ? this.indices.Length : 0;

            vao = new VertexArrayObject();
            vbo = new VertexBufferObject();
            ebo = new ElementBufferObject();

            if(calculateNormals)
                RecalculateNormals();

            bounds = new BoundingBox();
        }

        public void Generate()
        {
            if(vao.Id == 0 && vbo.Id == 0 && ebo.Id == 0)
            {
                vao.Generate();
                vao.Bind();

                vbo.Generate();
                vbo.Bind();

                vbo.BufferData<Vertex>(vertices, BufferUsageARB.StaticDraw);

                vao.EnableVertexAttribArray(0);
                vao.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), Marshal.OffsetOf(typeof(Vertex), "position"));

                vao.EnableVertexAttribArray(1);
                vao.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), Marshal.OffsetOf(typeof(Vertex), "normal"));

                vao.EnableVertexAttribArray(2);
                vao.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), Marshal.OffsetOf(typeof(Vertex), "uv"));

                if(indices?.Length > 0)
                {
                    ebo.Generate();
                    ebo.Bind();                
                    ebo.BufferData<uint>(indices, BufferUsageARB.StaticDraw);
                }

                vao.Unbind();
            }
            else
            {
                vbo.Bind();

                if(sizeOfVertices == vertices.Length)
                    vbo.BufferSubData<Vertex>(vertices, 0);
                else
                    vbo.BufferData<Vertex>(vertices, BufferUsageARB.StaticDraw);

                sizeOfVertices = vertices.Length;

                if(indices.Length > 0)
                {
                    ebo.Bind();

                    if(sizeOfIndices == indices.Length)
                        ebo.BufferSubData<uint>(indices, 0);
                    else
                        ebo.BufferData<uint>(indices, BufferUsageARB.StaticDraw);

                    sizeOfIndices = indices.Length;

                    ebo.Unbind();
                }

                vbo.Unbind();
            }

            bounds.Clear();

            for(int i = 0; i < vertices.Length; i++)
            {
                bounds.Grow(vertices[i].position);
            }
        }

        public void Delete()
        {
            ebo.Delete();
            vbo.Delete();
            vao.Delete();
        }

        public void RecalculateNormals()
        {
            int triangleCount = indices.Length / 3;

            for (int i = 0; i < triangleCount; i++)
            {
                int normalTriangleIndex = i * 3;
                int vertexIndexA = (int)indices[normalTriangleIndex];
                int vertexIndexB = (int)indices[normalTriangleIndex + 1];
                int vertexIndexC = (int)indices[normalTriangleIndex + 2];
                
                Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

                Vertex v1 = vertices[vertexIndexA];
                Vertex v2 = vertices[vertexIndexB];
                Vertex v3 = vertices[vertexIndexC];

                v1.normal += triangleNormal;
                v2.normal += triangleNormal;
                v3.normal += triangleNormal;

                v1.normal = Vector3.Normalize(v1.normal);
                v2.normal = Vector3.Normalize(v2.normal);
                v3.normal = Vector3.Normalize(v3.normal);

                vertices[vertexIndexA] = v1;
                vertices[vertexIndexB] = v2;
                vertices[vertexIndexC] = v3;
            }
        }

        private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
        {
            Vector3 pA = vertices[indexA].position;
            Vector3 pB = vertices[indexB].position;
            Vector3 pC = vertices[indexC].position;

            Vector3 sideAB = pB - pA;
            Vector3 sideAC = pC - pA;
            return Vector3.Normalize(Vector3.Cross(sideAB, sideAC));
        }
    }

    public static class MeshGenerator
    {
        public static void SetScale(Span<Vertex> vertices, Vector3 scale)
        {
            for(int i = 0; i < vertices.Length; i++)
            {
                Vertex v = vertices[i];
                v.position *= scale;
                vertices[i] = v;
            }
        }

        public static Mesh CreateCube(Vector3 scale)
        {
            Vertex[] vertices = new Vertex[24];
            
            for(int i = 0; i < vertices.Length; i++)
                vertices[i] = new Vertex();

            vertices[0].position = new Vector3(0.5f, -0.5f, 0.5f);
            vertices[1].position = new Vector3(-0.5f, -0.5f, 0.5f);
            vertices[2].position = new Vector3(0.5f, 0.5f, 0.5f);
            vertices[3].position = new Vector3(-0.5f, 0.5f, 0.5f);

            vertices[4].position = new Vector3(0.5f, 0.5f, -0.5f);
            vertices[5].position = new Vector3(-0.5f, 0.5f, -0.5f);
            vertices[6].position = new Vector3(0.5f, -0.5f, -0.5f);
            vertices[7].position = new Vector3(-0.5f, -0.5f, -0.5f);

            vertices[8].position = new Vector3(0.5f, 0.5f, 0.5f);
            vertices[9].position = new Vector3(-0.5f, 0.5f, 0.5f);
            vertices[10].position = new Vector3(0.5f, 0.5f, -0.5f);
            vertices[11].position = new Vector3(-0.5f, 0.5f, -0.5f);

            vertices[12].position = new Vector3(0.5f, -0.5f, -0.5f);
            vertices[13].position = new Vector3(0.5f, -0.5f, 0.5f);
            vertices[14].position = new Vector3(-0.5f, -0.5f, 0.5f);
            vertices[15].position = new Vector3(-0.5f, -0.5f, -0.5f);

            vertices[16].position = new Vector3(-0.5f, -0.5f, 0.5f);
            vertices[17].position = new Vector3(-0.5f, 0.5f, 0.5f);
            vertices[18].position = new Vector3(-0.5f, 0.5f, -0.5f);
            vertices[19].position = new Vector3(-0.5f, -0.5f, -0.5f);

            vertices[20].position = new Vector3(0.5f, -0.5f, -0.5f);
            vertices[21].position = new Vector3(0.5f, 0.5f, -0.5f);
            vertices[22].position = new Vector3(0.5f, 0.5f, 0.5f);
            vertices[23].position = new Vector3(0.5f, -0.5f, 0.5f);

            // UVS
            vertices[0].uv = new Vector2(0.0f, 0.0f);
            vertices[1].uv = new Vector2(1.0f, 0.0f);
            vertices[2].uv = new Vector2(0.0f, 1.0f);
            vertices[3].uv = new Vector2(1.0f, 1.0f);

            vertices[4].uv = new Vector2(0.0f, 1.0f);
            vertices[5].uv = new Vector2(1.0f, 1.0f);
            vertices[6].uv = new Vector2(0.0f, 1.0f);
            vertices[7].uv = new Vector2(1.0f, 1.0f);
            
            vertices[8].uv = new Vector2(0.0f, 0.0f);
            vertices[9].uv = new Vector2(1.0f, 0.0f);
            vertices[10].uv = new Vector2(0.0f, 0.0f);
            vertices[11].uv = new Vector2(1.0f, 0.0f);
            
            vertices[12].uv = new Vector2(0.0f, 0.0f);
            vertices[13].uv = new Vector2(0.0f, 1.0f);
            vertices[14].uv = new Vector2(1.0f, 1.0f);
            vertices[15].uv = new Vector2(1.0f, 0.0f);
            
            vertices[16].uv = new Vector2(0.0f, 0.0f);
            vertices[17].uv = new Vector2(0.0f, 1.0f);
            vertices[18].uv = new Vector2(1.0f, 1.0f);
            vertices[19].uv = new Vector2(1.0f, 0.0f);
            
            vertices[20].uv = new Vector2(0.0f, 0.0f);
            vertices[21].uv = new Vector2(0.0f, 1.0f);
            vertices[22].uv = new Vector2(1.0f, 1.0f);
            vertices[23].uv = new Vector2(1.0f, 0.0f);

            uint[] indices = new uint[]
            {
                0, 2, 3,
                0, 3, 1,

                8, 4, 5,
                8, 5, 9,

                10, 6, 7,
                10, 7, 11,

                12, 13, 14,
                12, 14, 15,

                16, 17, 18,
                16, 18, 19,

                20, 21, 22,
                20, 22, 23
            };

            SetScale(vertices, scale);
            Mesh mesh = new Mesh(vertices, indices, true);
            mesh.Generate();
            return mesh;
        }

        public static Mesh CreatePlane(Vector3 scale)
        {
            Vertex[] vertices = new Vertex[4];
            
            for(int i = 0; i < vertices.Length; i++)
                vertices[i] = new Vertex();

            Vertex bottomLeft = new Vertex();
            Vertex bottomRight = new Vertex();
            Vertex topleft = new Vertex();
            Vertex topRight = new Vertex();

            topleft.position = new Vector3(-0.5f, 0.0f, -0.5f);
            bottomLeft.position = new Vector3(-0.5f, 0.0f, 0.5f);
            bottomRight.position = new Vector3(0.5f, 0.0f, 0.5f);
            topRight.position = new Vector3(0.5f, 0.0f, -0.5f);

            topleft.uv = new Vector2(0.0f, 0.0f);
            bottomLeft.uv = new Vector2(0.0f, 1.0f);
            bottomRight.uv = new Vector2(1.0f, 1.0f);
            topRight.uv = new Vector2(1.0f, 0.0f);

            vertices[0] = topleft;
            vertices[1] = bottomLeft;
            vertices[2] = topRight;
            vertices[3] = bottomRight;

            uint[] indices = new uint[]
            {
                0, 1, 2,
                2, 1, 3,
            };

            SetScale(vertices, scale);
            Mesh mesh = new Mesh(vertices, indices, true);
            mesh.Generate();
            return mesh;
        }

        public static Mesh CreateQuad(Vector3 scale)
        {
            Vertex[] vertices = new Vertex[4];
            
            for(int i = 0; i < vertices.Length; i++)
                vertices[i] = new Vertex();

            Vertex bottomLeft;
            Vertex bottomRight;
            Vertex topleft;
            Vertex topRight;

            topleft.position     = new Vector3(0.0f, 1.0f, 0.0f);
            bottomLeft.position  = new Vector3(0.0f, 0.0f, 0.0f);
            bottomRight.position = new Vector3(1.0f, 0.0f, 0.0f);
            topRight.position    = new Vector3(1.0f, 1.0f, 0.0f);

            topleft.uv           = new Vector2(0.0f, 1.0f);
            bottomLeft.uv        = new Vector2(0.0f, 0.0f);
            bottomRight.uv       = new Vector2(1.0f, 0.0f);
            topRight.uv          = new Vector2(1.0f, 1.0f);

            topleft.normal       = new Vector3(0, 1, 0);
            bottomLeft.normal    = new Vector3(0, 1, 0);
            bottomRight.normal   = new Vector3(0, 1, 0);
            topRight.normal      = new Vector3(0, 1, 0);

            vertices[0] = topleft;
            vertices[1] = bottomLeft;
            vertices[2] = topRight;
            vertices[3] = bottomRight;

            for(int i = 0; i < vertices.Length; i++)
                vertices[i].position -= new Vector3(0.5f, 0.5f, 0.0f);

            uint[] indices = new uint[]
            {
                0, 1, 2, 
                1, 3, 2
            };

            SetScale(vertices, scale);
            Mesh mesh = new Mesh(vertices, indices, true);
            mesh.Generate();
            return mesh;
        }

        public static Mesh CreateSkybox(Vector3 scale)
        {
            Vertex[] vertices = new Vertex[36];
            
            for(int i = 0; i < vertices.Length; i++)
                vertices[i] = new Vertex();

            vertices[0].position = new Vector3(-1.0f, 1.0f, -1.0f);
            vertices[1].position = new Vector3(-1.0f, -1.0f, -1.0f);
            vertices[2].position = new Vector3(1.0f, -1.0f, -1.0f);
            vertices[3].position = new Vector3(1.0f, -1.0f, -1.0f);
            vertices[4].position = new Vector3(1.0f, 1.0f, -1.0f);
            vertices[5].position = new Vector3(-1.0f, 1.0f, -1.0f);

            vertices[6].position = new Vector3(-1.0f, -1.0f, 1.0f);
            vertices[7].position = new Vector3(-1.0f, -1.0f, -1.0f);
            vertices[8].position = new Vector3(-1.0f, 1.0f, -1.0f);
            vertices[9].position = new Vector3(-1.0f, 1.0f, -1.0f);
            vertices[10].position = new Vector3(-1.0f, 1.0f, 1.0f);
            vertices[11].position = new Vector3(-1.0f, -1.0f, 1.0f);

            vertices[12].position = new Vector3(1.0f, -1.0f, -1.0f);
            vertices[13].position = new Vector3(1.0f, -1.0f, 1.0f);
            vertices[14].position = new Vector3(1.0f, 1.0f, 1.0f);
            vertices[15].position = new Vector3(1.0f, 1.0f, 1.0f);
            vertices[16].position = new Vector3(1.0f, 1.0f, -1.0f);
            vertices[17].position = new Vector3(1.0f, -1.0f, -1.0f);

            vertices[18].position = new Vector3(-1.0f, -1.0f, 1.0f);
            vertices[19].position = new Vector3(-1.0f, 1.0f, 1.0f);
            vertices[20].position = new Vector3(1.0f, 1.0f, 1.0f);
            vertices[21].position = new Vector3(1.0f, 1.0f, 1.0f);
            vertices[22].position = new Vector3(1.0f, -1.0f, 1.0f);
            vertices[23].position = new Vector3(-1.0f, -1.0f, 1.0f);

            vertices[24].position = new Vector3(-1.0f, 1.0f, -1.0f);
            vertices[25].position = new Vector3(1.0f, 1.0f, -1.0f);
            vertices[26].position = new Vector3(1.0f, 1.0f, 1.0f);
            vertices[27].position = new Vector3(1.0f, 1.0f, 1.0f);
            vertices[28].position = new Vector3(-1.0f, 1.0f, 1.0f);
            vertices[29].position = new Vector3(-1.0f, 1.0f, -1.0f);

            vertices[30].position = new Vector3(-1.0f, -1.0f, -1.0f);
            vertices[31].position = new Vector3(-1.0f, -1.0f, 1.0f);
            vertices[32].position = new Vector3(1.0f, -1.0f, -1.0f);
            vertices[33].position = new Vector3(1.0f, -1.0f, -1.0f);
            vertices[34].position = new Vector3(-1.0f, -1.0f, 1.0f);
            vertices[35].position = new Vector3(1.0f, -1.0f, 1.0f);

            for(int i = 0; i < vertices.Length; i+=6)
            {
                Vertex v1 = vertices[i+0];
                Vertex v2 = vertices[i+1];
                Vertex v3 = vertices[i+2];
                Vertex v4 = vertices[i+3];
                Vertex v5 = vertices[i+4];
                Vertex v6 = vertices[i+5];

                v1.uv = new Vector2(0.0f, 0.0f);
                v2.uv = new Vector2(0.0f, 1.0f);
                v3.uv = new Vector2(1.0f, 1.0f);
                v4.uv = new Vector2(1.0f, 1.0f);
                v5.uv = new Vector2(1.0f, 0.0f);
                v6.uv = new Vector2(0.0f, 0.0f);

                vertices[i+0] = v1;
                vertices[i+1] = v2;
                vertices[i+2] = v3;
                vertices[i+3] = v4;
                vertices[i+4] = v5;
                vertices[i+5] = v6;
            }
            
            uint[] indices = null;
            Mesh mesh = new Mesh(vertices, indices, false);
            mesh.Generate();
            return mesh;
        }

        public static Mesh CreateSphere(Vector3 scale)
        {
            List<Vertex> vertices = new List<Vertex>();
            List<uint> indices = new List<uint>();

            uint sectorCount = 72;
            uint stackCount = 24;
            float radius = 0.5f;

            float x, y, z, xy;                              // vertex position
            float lengthInv = 1.0f / radius;    // vertex normal
            float s, t;                                     // vertex texCoord

            const float PI = (float)Math.PI;

            float sectorStep = 2 * PI / sectorCount;
                    
            float stackStep = PI / stackCount;
            float sectorAngle, stackAngle;

            for(uint i = 0; i <= stackCount; ++i)
            {
                stackAngle = PI / 2 - i * stackStep;        // starting from pi/2 to -pi/2
                xy = radius * (float)Math.Cos(stackAngle);             // r * cos(u)
                z = radius* (float)Math.Sin(stackAngle);              // r * sin(u)

                // add (sectorCount+1) vertices per stack
                // the first and last vertices have same position and normal, but different tex coords
                for(int j = 0; j <= sectorCount; ++j)
                {
                    Vertex v = new Vertex();

                    sectorAngle = j * sectorStep;           // starting from 0 to 2pi

                    // vertex position (x, y, z)
                    x = xy * (float)Math.Cos(sectorAngle);             // r * cos(u) * cos(v)
                    y = xy * (float)Math.Sin(sectorAngle);             // r * cos(u) * sin(v)          
                    v.position = new Vector3(x, y, z);

                    // vertex tex coord (s, t) range between [0, 1]
                    s = (float) j / sectorCount;
                    t = (float) i / stackCount;          
                    v.uv = new Vector2(s, t);
                    
                    vertices.Add(v);
                }
            }

            uint k1, k2;

            for(uint i = 0; i < stackCount; ++i)
            {
                k1 = i * (sectorCount + 1);     // beginning of current stack
                k2 = k1 + sectorCount + 1;      // beginning of next stack

                for(int j = 0; j<sectorCount; ++j, ++k1, ++k2)
                {
                    // 2 triangles per sector excluding first and last stacks
                    // k1 => k2 => k1+1
                    if(i != 0)
                    {
                        indices.Add(k1);
                        indices.Add(k2);
                        indices.Add(k1 + 1);
                    }

                    // k1+1 => k2 => k2+1
                    if(i != (stackCount-1))
                    {
                        indices.Add(k1 + 1);
                        indices.Add(k2);
                        indices.Add(k2 + 1);
                    }
                }
            }

            Vertex[] verts = vertices.ToArray();
            uint[] idx = indices.ToArray();

            SetScale(verts, scale);
            Mesh mesh = new Mesh(verts, idx, true);
            mesh.Generate();
            return mesh;
        }

        public static Mesh CreateTerrain(uint width, uint height, Vector3 scale)
        {
            width += 1;
            height += 1;

            uint LOD = 0;

            uint meshSimplificationIncrement = (LOD == 0) ? 1 : LOD * 2;
            uint verticesPerLine = ((width - 1) / meshSimplificationIncrement + 1);

            float topLeftX = 0;
            float topLeftZ = 0;

            uint numVertices = verticesPerLine * height;
            uint numIndices = (verticesPerLine - 1) * (verticesPerLine - 1) * 6;
            int vertexIndex = 0;
            int triangleIndex = 0;

            List<Vertex> vertices = new List<Vertex>();
            List<uint> indices = new List<uint>();

            for(int i = 0; i < numVertices; i++)
                vertices.Add(new Vertex());
            
            for(int i = 0; i < numIndices; i++)
                indices.Add(0);

            for (uint y = 0; y < height; y += meshSimplificationIncrement)
            {
                for (uint x = 0; x < width; x += meshSimplificationIncrement)
                {
                    Vertex vertex = vertices[vertexIndex];
                    vertex.position = new Vector3(topLeftX + x, 0.0f, topLeftZ - y);
                    vertex.uv = new Vector2((float)x / (width - 1.0f), (float)y / (height - 1.0f));
                    vertex.normal = new Vector3(0, 1, 0);

                    vertices[vertexIndex] = vertex;

                    if (x < width - 1 && y < height - 1)
                    {
                        indices[triangleIndex + 0] = (uint)vertexIndex;
                        indices[triangleIndex + 1] = (uint)(vertexIndex + verticesPerLine + 1);
                        indices[triangleIndex + 2] = (uint)(vertexIndex + verticesPerLine);

                        indices[triangleIndex + 3] = (uint)(vertexIndex + verticesPerLine + 1);
                        indices[triangleIndex + 4] = (uint)vertexIndex;
                        indices[triangleIndex + 5] = (uint)(vertexIndex + 1);
                        triangleIndex += 6;
                    }

                    vertexIndex++;
                }
            }

            Vertex[] verts = vertices.ToArray();
            uint[] idx = indices.ToArray();

            SetScale(verts, scale);
            Mesh mesh = new Mesh(verts, idx, false); //Do not generate normals as they all face up already
            mesh.Generate();
            return mesh;
        }
    }
}