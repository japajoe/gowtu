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

        public static Mesh CreateIcosahedron(Vector3 scale, int detail)
        {
            if(detail < 0)
                detail = 0;

            List<Vertex> vertices = new List<Vertex>();
            List<uint> indices = new List<uint>();

            float t = (1.0f + (float)Math.Sqrt(5.0f)) / 2.0f;

            vertices.Add(new Vertex(new Vector3(-1, t, 0), Vector3.Zero, Vector2.Zero));
            vertices.Add(new Vertex(new Vector3(1, t, 0), Vector3.Zero, Vector2.Zero));
            vertices.Add(new Vertex(new Vector3(-1, -t, 0), Vector3.Zero, Vector2.Zero));
            vertices.Add(new Vertex(new Vector3(1, -t, 0), Vector3.Zero, Vector2.Zero));

            vertices.Add(new Vertex(new Vector3(0, -1, t), Vector3.Zero, Vector2.Zero));
            vertices.Add(new Vertex(new Vector3(0, 1, t), Vector3.Zero, Vector2.Zero));
            vertices.Add(new Vertex(new Vector3(0, -1, -t), Vector3.Zero, Vector2.Zero));
            vertices.Add(new Vertex(new Vector3(0, 1, -t), Vector3.Zero, Vector2.Zero));

            vertices.Add(new Vertex(new Vector3(t, 0, -1), Vector3.Zero, Vector2.Zero));
            vertices.Add(new Vertex(new Vector3(t, 0, 1), Vector3.Zero, Vector2.Zero));
            vertices.Add(new Vertex(new Vector3(-t, 0, -1), Vector3.Zero, Vector2.Zero));
            vertices.Add(new Vertex(new Vector3(-t, 0, 1), Vector3.Zero, Vector2.Zero));

            indices.AddRange(new uint[] {
                0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
                1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
                3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
                4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1
            });

            for (int i = 0; i < detail; i++)
            {
                Dictionary<(uint, uint), uint> midPointCache = new Dictionary<(uint, uint), uint>();

                uint GetMidPoint(uint a, uint b)
                {
                    var key = a < b ? (a, b) : (b, a);
                    if (midPointCache.TryGetValue(key, out uint value))
                        return value;

                    Vector3 mid = Vector3.Normalize((vertices[(int)a].position + vertices[(int)b].position) / 2.0f);
                    uint index = (uint)vertices.Count;
                    vertices.Add(new Vertex(mid, Vector3.Zero, Vector2.Zero));
                    midPointCache[key] = index;
                    return index;
                }

                List<uint> newIndices = new List<uint>();
                for (int j = 0; j < indices.Count; j += 3)
                {
                    uint v1 = indices[j];
                    uint v2 = indices[j + 1];
                    uint v3 = indices[j + 2];

                    uint a = GetMidPoint(v1, v2);
                    uint b = GetMidPoint(v2, v3);
                    uint c = GetMidPoint(v3, v1);

                    newIndices.AddRange(new uint[] { v1, a, c, v2, b, a, v3, c, b, a, b, c });
                }
                indices = newIndices;
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 n = Vector3.Normalize(vertices[i].position);
                vertices[i] = new Vertex(n, n, new Vector2((float)(Math.Atan2(n.X, n.Z) / (2 * Math.PI) + 0.5), (float)(Math.Asin(n.Y) / Math.PI + 0.5)));
            }

            Vertex[] verts = vertices.ToArray();
            uint[] idx = indices.ToArray();

            SetScale(verts, scale);
            Mesh mesh = new Mesh(verts, idx, true);
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

        public static Mesh CreateCapsule(Vector3 scale)
        {
            float height = 2.0f;
            float radius = 0.5f;   
            int segments = 32;
            int rings = 8;
            float cylinderHeight = height - radius * 2;
            int vertexCount = 2 * rings * segments + 2;
            int triangleCount = 4 * rings * segments;
            float horizontalAngle = 360.0f / segments;
            float verticalAngle = 90.0f / rings;

            Vertex[] vertices = new Vertex[vertexCount];
            uint[] indices = new uint[3 * triangleCount];

            int vi = 2;
            int ti = 0;
            int topCapIndex = 0;
            int bottomCapIndex = 1;

            vertices[topCapIndex].position = new Vector3(0, cylinderHeight / 2 + radius, 0);
            vertices[topCapIndex].normal = new Vector3(0, 1, 0);
            vertices[bottomCapIndex].position = new Vector3(0, -cylinderHeight / 2 - radius, 0);
            vertices[bottomCapIndex].normal = new Vector3(0, -1, 0);

            for (int s = 0; s < segments; s++)
            {
                for (int r = 1; r <= rings; r++)
                {
                    // Top cap vertex
                    Vector3 normal = PointOnSphere(1, s * horizontalAngle, 90 - r * verticalAngle);
                    Vector3 vertex = new Vector3(radius * normal.X, radius * normal.Y + cylinderHeight / 2, radius * normal.Z);
                    vertices[vi].position = vertex;
                    vertices[vi].normal = normal;
                    vi++;

                    // Bottom cap vertex
                    vertices[vi].position = new Vector3(vertex.X, -vertex.Y, vertex.Z);
                    vertices[vi].normal = new Vector3(normal.X, -normal.Y, normal.Z);
                    vi++;

                    int top_s1r1 = vi - 2;
                    int top_s1r0 = vi - 4;
                    int bot_s1r1 = vi - 1;
                    int bot_s1r0 = vi - 3;
                    int top_s0r1 = top_s1r1 - 2 * rings;
                    int top_s0r0 = top_s1r0 - 2 * rings;
                    int bot_s0r1 = bot_s1r1 - 2 * rings;
                    int bot_s0r0 = bot_s1r0 - 2 * rings;

                    if (s == 0)
                    {
                        top_s0r1 += vertexCount - 2;
                        top_s0r0 += vertexCount - 2;
                        bot_s0r1 += vertexCount - 2;
                        bot_s0r0 += vertexCount - 2;
                    }

                    // Create cap triangles
                    if (r == 1)
                    {
                        indices[3 * ti + 0] = (uint)topCapIndex;
                        indices[3 * ti + 1] = (uint)top_s0r1;
                        indices[3 * ti + 2] = (uint)top_s1r1;
                        ti++;

                        indices[3 * ti + 0] = (uint)bottomCapIndex;
                        indices[3 * ti + 1] = (uint)bot_s1r1;
                        indices[3 * ti + 2] = (uint)bot_s0r1;
                        ti++;
                    }
                    else
                    {
                        indices[3 * ti + 0] = (uint)top_s1r0;
                        indices[3 * ti + 1] = (uint)top_s0r0;
                        indices[3 * ti + 2] = (uint)top_s1r1;
                        ti++;

                        indices[3 * ti + 0] = (uint)top_s0r0;
                        indices[3 * ti + 1] = (uint)top_s0r1;
                        indices[3 * ti + 2] = (uint)top_s1r1;
                        ti++;

                        indices[3 * ti + 0] = (uint)bot_s0r1;
                        indices[3 * ti + 1] = (uint)bot_s0r0;
                        indices[3 * ti + 2] = (uint)bot_s1r1;
                        ti++;

                        indices[3 * ti + 0] = (uint)bot_s0r0;
                        indices[3 * ti + 1] = (uint)bot_s1r0;
                        indices[3 * ti + 2] = (uint)bot_s1r1;
                        ti++;
                    }
                }

                // Create side triangles
                int top_s1 = vi - 2;
                int top_s0 = top_s1 - 2 * rings;
                int bot_s1 = vi - 1;
                int bot_s0 = bot_s1 - 2 * rings;
                
                if (s == 0)
                {
                    top_s0 += vertexCount - 2;
                    bot_s0 += vertexCount - 2;
                }

                indices[3 * ti + 0] = (uint)top_s0;
                indices[3 * ti + 1] = (uint)bot_s1;
                indices[3 * ti + 2] = (uint)top_s1;
                ti++;

                indices[3 * ti + 0] = (uint)bot_s0;
                indices[3 * ti + 1] = (uint)bot_s1;
                indices[3 * ti + 2] = (uint)top_s0;
                ti++;
            }

            SetScale(vertices, scale);
            Mesh mesh = new Mesh(vertices, indices, true);
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

        private static Vector3 PointOnSpheroid(float radius, float height, float horizontalAngle, float verticalAngle)
        {
            float horizontalRadians = MathHelper.DegreesToRadians(horizontalAngle);
            float verticalRadians = MathHelper.DegreesToRadians(verticalAngle);
            float cosVertical = (float)Math.Cos(verticalRadians);

            return new Vector3( radius * (float)Math.Sin(horizontalRadians) * cosVertical,
                                height * (float)Math.Sin(verticalRadians),
                                radius * (float)Math.Cos(horizontalRadians) * cosVertical);
        }

        private static Vector3 PointOnSphere(float radius, float horizontalAngle, float verticalAngle)
        {
            return PointOnSpheroid(radius, radius, horizontalAngle, verticalAngle);
        }
    }
}