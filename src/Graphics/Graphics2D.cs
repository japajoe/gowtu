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
    public static class Graphics2D
    {
        private static int VAO;
        private static int VBO;
        private static int EBO;
        private static int shaderId;
        private static int textureId;
        private static int[] uniforms;
        private static List<DrawListItem> items;
        private static List<Vertex2D> vertices;
        private static List<uint> indices;
        private static int itemCount;
        private static int vertexCount;
        private static int indiceCount;
        private static List<Vertex2D> vertexBufferTemp; //Temporary buffer used by some 'add' functions with dynamic size requirements
        private static List<uint> indexBufferTemp; //Temporary buffer used by some 'add' functions with dynamic size requirements
        private static GLStateInfo glState;
        private static float elapsedTime;
        private static int numDrawCalls;

        public static event UniformUpdateCallback UniformUpdate;

        static Graphics2D()
        {
            Initialize();
        }

        private static void Initialize()
        {
            if(items != null)
                return;

            VAO = 0;
            VBO = 0;
            EBO = 0;
            shaderId = 0;
            textureId = 0;
            uniforms = new int[(int)Uniform.COUNT];
            items = new List<DrawListItem>();
            vertices = new List<Vertex2D>();
            indices = new List<uint>();
            itemCount = 0;
            vertexCount = 0;
            indiceCount = 0;
            vertexBufferTemp = new List<Vertex2D>();
            indexBufferTemp = new List<uint>();
            elapsedTime = 0.0f;
            numDrawCalls = 0;

            CreateBuffers();
            CreateShader();
            CreateTexture();
        }

        internal static unsafe void NewFrame()
        {
            numDrawCalls = itemCount;

            if(itemCount == 0) 
            {
                elapsedTime += Time.DeltaTime;
                return;
            }

            var viewport = Graphics.GetViewport();
            float L = viewport.x;
            float R = viewport.x + viewport.width;
            float T = viewport.y;
            float B = viewport.y + viewport.height;

            float near = -1.0f;
            float far = 1.0f;

            float[,] projectionMatrix = new float[4, 4]
            {
                { 2.0f / (R - L),    0.0f,            0.0f,              0.0f },
                { 0.0f,              2.0f / (T - B),  0.0f,              0.0f },
                { 0.0f,              0.0f,           -1.0f / (far - near), 0.0f },
                { -(R + L) / (R - L), -(T + B) / (T - B), (far + near) / (far - near), 1.0f }
            };

            StoreState();

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.BlendEquation(BlendEquationModeEXT.FuncAdd);

            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);
            ReadOnlySpan<Vertex2D> pVertices = CollectionsMarshal.AsSpan(vertices).Slice(0, (int)vertexCount);
            GL.BufferSubData(BufferTargetARB.ArrayBuffer, IntPtr.Zero, pVertices);

            GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, EBO);
            ReadOnlySpan<uint> pIndices = CollectionsMarshal.AsSpan(indices).Slice(0, (int)indiceCount);
            GL.BufferSubData(BufferTargetARB.ElementArrayBuffer, IntPtr.Zero, pIndices);

            int lastShaderId = items[0].shaderId;
            GL.UseProgram(lastShaderId);
            GL.ActiveTexture(TextureUnit.Texture0);

            int lastTextureId = items[0].textureId;
            GL.BindTexture(TextureTarget.Texture2d, lastTextureId);

            int drawOffset = 0; // Offset for the draw call

            for(int i = 0; i < itemCount; i++) 
            {
                Rectangle rect = items[i].clippingRect;
                bool scissorEnabled = false;

                if(!rect.IsZero()) 
                {
                    GL.Enable(EnableCap.ScissorTest);
                    GL.Scissor((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
                    scissorEnabled = true;
                }

                if(items[i].shaderId != lastShaderId) 
                {
                    GL.UseProgram(items[i].shaderId);
                    lastShaderId = items[i].shaderId;
                }

                if(items[i].textureId != lastTextureId) 
                {
                    GL.BindTexture(TextureTarget.Texture2d, items[i].textureId);
                    lastTextureId = items[i].textureId;
                }

                if(lastShaderId == shaderId) 
                {
                    GL.Uniform1i(uniforms[(int)Uniform.Texture], 0);

                    unsafe 
                    {
                        fixed(float *pProjectionMatrix = &projectionMatrix[0,0])
                        {
                            GL.UniformMatrix4fv(uniforms[(int)Uniform.Projection], 1, false, pProjectionMatrix);
                        }
                    }
                    
                    GL.Uniform1f(uniforms[(int)Uniform.Time], elapsedTime);

                    // //This uniform is only mandatory on default shader
                    GL.Uniform1i(uniforms[(int)Uniform.IsFont], items[i].textureIsFont ? 1 : 0);
                }
                else 
                {
                    // Only dispatch callback for custom shaders
                    //These 3 uniforms are mandatory on any shader
                    GL.Uniform1i(GL.GetUniformLocation(lastShaderId, "uTexture"), 0);
                    
                    unsafe 
                    {
                        fixed(float *pProjectionMatrix = &projectionMatrix[0,0])
                        {
                            GL.UniformMatrix4fv(GL.GetUniformLocation(lastShaderId, "uProjection"), 1, false, pProjectionMatrix);
                        }
                    }
                    
                    GL.Uniform1f(GL.GetUniformLocation(lastShaderId, "uTime"), elapsedTime);
                    
                    UniformUpdate?.Invoke(lastShaderId, items[i].userData);
                }

                IntPtr offset = new IntPtr(drawOffset * Marshal.SizeOf<uint>());
                GL.DrawElements(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, items[i].indiceCount, DrawElementsType.UnsignedInt, offset);

                drawOffset += items[i].indiceCount;

                if(scissorEnabled) 
                {
                    GL.Disable(EnableCap.ScissorTest);
                }
            }

            GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            RestoreState();

            GL.Disable(EnableCap.ScissorTest);

            // Reset counts for the next render
            itemCount = 0;
            vertexCount = 0;
            indiceCount = 0;

            elapsedTime += Time.DeltaTime;
        }

        public static void AddRectangle(Vector2 position, Vector2 size, float rotationDegrees, Color color, Rectangle clippingRect = default(Rectangle), int shaderId = 0, object userData = null) 
        {
            Vertex2D v1 = new Vertex2D(new Vector2(position.X, position.Y), new Vector2(0, 1), color);
            Vertex2D v2 = new Vertex2D(new Vector2(position.X, position.Y + size.Y), new Vector2(0, 0), color);
            Vertex2D v3 = new Vertex2D(new Vector2(position.X + size.X, position.Y + size.Y), new Vector2(1, 0), color);
            Vertex2D v4 = new Vertex2D(new Vector2(position.X + size.X, position.Y), new Vector2(1, 1), color);

            Vertex2D[] vertices =  
            {
                v1, //Top left
                v2, //Bottom left
                v3, //Bottom right
                v4  //Top right
            };

            if(rotationDegrees != 0.0f)
                RotateVertices(vertices, 4, rotationDegrees);

            uint[] indices = 
            {
                0, 1, 2, 
                0, 2, 3
            };

            DrawCommand command = new DrawCommand();
            command.vertices = vertices;
            command.indices = indices;
            command.numVertices = 4;
            command.numIndices = 6;
            command.textureId = textureId;
            command.textureIsFont = false;
            command.shaderId = shaderId;
            command.clippingRect = clippingRect;
            command.userData = userData;

            AddVertices(command);
        }

        public static void AddCircle(Vector2 position, float radius, int segments, float rotationDegrees, Color color, Rectangle clippingRect = default(Rectangle), int shaderId = 0, object userData = null)
        {
            if(segments < 3)
                segments = 3;

            int requiredVertices = segments;
            int requiredIndices = segments * 3; // 3 indices per segment

            CheckTemporaryVertexBuffer(requiredVertices);
            CheckTemporaryIndexBuffer(requiredIndices);

            for (int i = 0; i < segments; ++i) 
            {
                float angle = 2.0f * (float)Math.PI * i / segments;
                Vertex2D v = vertexBufferTemp[i];
                
                v.position = new Vector2(radius * (float)Math.Cos(angle), radius * (float)Math.Sin(angle));
                v.uv = new Vector2(0.5f + 0.5f * (float)Math.Cos(angle), 0.5f + 0.5f * (float)Math.Sin(angle));
                v.color = color;
                v.position.X += position.X;
                v.position.Y += position.Y;

                vertexBufferTemp[i] = v;
            }

            Span<Vertex2D> pVertexBufferTemp = CollectionsMarshal.AsSpan<Vertex2D>(vertexBufferTemp).Slice(0, requiredVertices);
            Span<uint> pIndexBufferTemp = CollectionsMarshal.AsSpan<uint>(indexBufferTemp).Slice(0, requiredIndices);

            if(rotationDegrees != 0.0f)
                RotateVertices(pVertexBufferTemp, segments, rotationDegrees);


            for (int i = 0; i < segments; ++i) 
            {
                indexBufferTemp[i * 3] = 0; // Center vertex (if added at 0 index)
                indexBufferTemp[i * 3 + 1] = (uint)i;
                indexBufferTemp[i * 3 + 2] = (uint)((i + 1) % segments); // Wrap around to form a circle
            }

            DrawCommand command = new DrawCommand();
            command.vertices = pVertexBufferTemp;
            command.indices = pIndexBufferTemp;
            command.numVertices = segments;
            command.numIndices = segments * 3;
            command.textureId = textureId;
            command.textureIsFont = false;
            command.shaderId = shaderId;
            command.clippingRect = clippingRect;
            command.userData = userData;

            AddVertices(command);
        }

        public static void AddTriangle(Vector2 position, Vector2 size, float rotationDegrees, Color color, Rectangle clippingRect = default(Rectangle), int shaderId = 0, object userData = null) 
        {
            float radius = (float)(size.X / Math.Sqrt(3));

            Vector2 vertex1 = new Vector2(position.X, position.Y + radius); // Top vertex
            Vector2 vertex2 = new Vector2(position.X - radius * (float)Math.Sin(Math.PI / 3), position.Y - radius * (float)Math.Cos(Math.PI / 3)); // Bottom-left vertex
            Vector2 vertex3 = new Vector2(position.X + radius * (float)Math.Sin(Math.PI / 3), position.Y - radius * (float)Math.Cos(Math.PI / 3)); // Bottom-right vertex

            Vector2 uv1 = new Vector2(0.5f, 1.0f); // Top vertex UV
            Vector2 uv2 = new Vector2(0.0f, 0.0f); // Bottom-left vertex UV
            Vector2 uv3 = new Vector2(1.0f, 0.0f); // Bottom-right vertex UV

            Vertex2D[] vertices = 
            {
                new Vertex2D(vertex1, uv1, color),
                new Vertex2D(vertex2, uv2, color),
                new Vertex2D(vertex3, uv3, color)
            };

            if(rotationDegrees != 0.0f)
                RotateVertices(vertices, 3, rotationDegrees);

            uint[] indices = 
            {
                0, 1, 2, 
            };

            DrawCommand command = new DrawCommand();
            command.vertices = vertices;
            command.indices = indices;
            command.numVertices = 3;
            command.numIndices = 3;
            command.textureId = textureId;
            command.textureIsFont = false;
            command.shaderId = shaderId;
            command.clippingRect = clippingRect;
            command.userData = userData;

            AddVertices(command);
        }

        public static void AddBorder(Vector2 position, Vector2 size, float thickness, BorderOptions borderOptions, Color color, Rectangle clippingRect = default(Rectangle), int shaderId = 0, object userData = null) 
        {
            Rectangle outerRect = new Rectangle(position.X, position.Y, size.X, size.Y);
            float innerOffset = 0.0f;

            Vector2 innerTopLeft = new Vector2(outerRect.x + innerOffset, outerRect.y + innerOffset);
            Vector2 innerTopRight = new Vector2(outerRect.x + outerRect.width - innerOffset, outerRect.y + innerOffset);
            Vector2 innerBottomLeft = new Vector2(outerRect.x + innerOffset, outerRect.y + outerRect.height - innerOffset);
            Vector2 innerBottomRight = new Vector2(outerRect.x + outerRect.width - innerOffset, outerRect.y + outerRect.height - innerOffset);

            // Fixed size for maximum lines (up to 8 for 4 borders)
            Vector2[] lines = new Vector2[8];
            int lineCount = 0;

            // Check each border option and add the corresponding lines
            if (borderOptions.HasFlag(BorderOptions.Top)) 
            {
                lines[lineCount++] = innerTopLeft;
                lines[lineCount++] = innerTopRight;
            }
            if (borderOptions.HasFlag(BorderOptions.Right)) 
            {
                lines[lineCount++] = innerTopRight;
                lines[lineCount++] = innerBottomRight;
            }
            if (borderOptions.HasFlag(BorderOptions.Bottom)) 
            {
                lines[lineCount++] = innerBottomRight;
                lines[lineCount++] = innerBottomLeft;
            }
            if (borderOptions.HasFlag(BorderOptions.Left)) 
            {
                lines[lineCount++] = innerBottomLeft;
                lines[lineCount++] = innerTopLeft;
            }

            AddLines(lines, lineCount / 2, thickness, color, clippingRect);
        }

        public static void AddLine(Vector2 p1, Vector2 p2, float thickness, Color color, Rectangle clippingRect = default(Rectangle), int shaderId = 0, object userData = null)
        {
            Vector2 direction = new Vector2(p2.X - p1.X, p2.Y - p1.Y);
            float length = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);

            if (length == 0) 
                return;

            direction.X /= length;
            direction.Y /= length;

            Vector2 perpendicular = new Vector2(-direction.Y * thickness * 0.5f, 
                                                 direction.X * thickness * 0.5f);

            Vertex2D v1 = new Vertex2D(new Vector2(p1.X + perpendicular.X, p1.Y + perpendicular.Y), new Vector2(0, 1), color);
            Vertex2D v2 = new Vertex2D(new Vector2(p1.X - perpendicular.X, p1.Y - perpendicular.Y), new Vector2(0, 0), color);
            Vertex2D v3 = new Vertex2D(new Vector2(p2.X - perpendicular.X, p2.Y - perpendicular.Y), new Vector2(1, 0), color);
            Vertex2D v4 = new Vertex2D(new Vector2(p2.X + perpendicular.X, p2.Y + perpendicular.Y), new Vector2(1, 1), color);

            Vertex2D[] vertices = 
            {
                v1, //Bottom left
                v2, //Top left
                v3, //Top right
                v4  //Bottom right
            };

            uint[] indices = 
            {
                0, 1, 2, 
                0, 2, 3
            };

            DrawCommand command = new DrawCommand();
            command.vertices = vertices;
            command.indices = indices;
            command.numVertices = 4;
            command.numIndices = 6;
            command.textureId = textureId;
            command.textureIsFont = false;
            command.shaderId = shaderId;
            command.clippingRect = clippingRect;
            command.userData = userData;

            AddVertices(command);
        }

        public static void AddLines(ReadOnlySpan<Vector2> segments, int count, float thickness, Color color, Rectangle clippingRect = default(Rectangle), int shaderId = 0, object userData = null) 
        {
            if (count == 0) 
                return;

            if (segments == null) 
                return;

            int requiredVertices = count * 4; // 4 vertices per line
            int requiredIndices = count * 6; // 6 indices per line

            CheckTemporaryVertexBuffer(requiredVertices);
            CheckTemporaryIndexBuffer(requiredIndices);

            int pointCount = count * 2;
            int vertexIndex = 0;
            int indiceIndex = 0;

            for(int i = 0; i < pointCount; i+=2) 
            {
                Vector2 p1 = segments[i+0];
                Vector2 p2 = segments[i+1];

                Vector2 direction = new Vector2(p2.X - p1.X, p2.Y - p1.Y);

                float length = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);

                if (length == 0) 
                    return;

                direction.X /= length;
                direction.Y /= length;

                Vector2 perpendicular = new Vector2(-direction.Y * thickness * 0.5f, 
                                                     direction.X * thickness * 0.5f);

                vertexBufferTemp[vertexIndex+0] = new Vertex2D(new Vector2(p1.X + perpendicular.X, p1.Y + perpendicular.Y), new Vector2(0, 1), color);
                vertexBufferTemp[vertexIndex+1] = new Vertex2D(new Vector2(p1.X - perpendicular.X, p1.Y - perpendicular.Y), new Vector2(0, 0), color);
                vertexBufferTemp[vertexIndex+2] = new Vertex2D(new Vector2(p2.X - perpendicular.X, p2.Y - perpendicular.Y), new Vector2(1, 0), color);
                vertexBufferTemp[vertexIndex+3] = new Vertex2D(new Vector2(p2.X + perpendicular.X, p2.Y + perpendicular.Y), new Vector2(1, 1), color);

                indexBufferTemp[indiceIndex+0] = (uint)(0 + vertexIndex);
                indexBufferTemp[indiceIndex+1] = (uint)(1 + vertexIndex);
                indexBufferTemp[indiceIndex+2] = (uint)(2 + vertexIndex);
                indexBufferTemp[indiceIndex+3] = (uint)(0 + vertexIndex);
                indexBufferTemp[indiceIndex+4] = (uint)(2 + vertexIndex);
                indexBufferTemp[indiceIndex+5] = (uint)(3 + vertexIndex);

                vertexIndex += 4;
                indiceIndex += 6;
            }

            Span<Vertex2D> pVertexBufferTemp = CollectionsMarshal.AsSpan<Vertex2D>(vertexBufferTemp).Slice(0, requiredVertices);
            Span<uint> pIndexBufferTemp = CollectionsMarshal.AsSpan<uint>(indexBufferTemp).Slice(0, requiredIndices);

            DrawCommand command = new DrawCommand();
            command.vertices = pVertexBufferTemp;
            command.indices = pIndexBufferTemp;
            command.numVertices = requiredVertices;
            command.numIndices = requiredIndices;
            command.textureId = textureId;
            command.textureIsFont = false;
            command.shaderId = shaderId;
            command.clippingRect = clippingRect;
            command.userData = userData;

            AddVertices(command);
        }

        public static void AddPlotLines(Vector2 position, Vector2 size, ReadOnlySpan<float> data, int valuesCount, float thickness, Color color, float scaleMin = 3.402823466e+38F, float scaleMax = 3.402823466e+38F, Rectangle clippingRect = default(Rectangle), int shaderId = 0, object userData = null) 
        {
            if (valuesCount < 2) 
                return;

            if (data == null) 
                return;

            int count = valuesCount - 1;
            int requiredVertices = count * 4; // 4 vertices per line
            int requiredIndices = count * 6; // 6 indices per line
            float plotWidth = size.X;
            float plotHeight = size.Y;
            float step = plotWidth / valuesCount;

            if (scaleMin == float.MaxValue || scaleMax == float.MaxValue) {
                float minValue = float.MaxValue;
                float maxValue = float.MinValue;

                for(int i = 0; i < valuesCount; i++) {
                    float v = data[i];
                    
                    if (float.IsNaN(v)) // Ignore NaN values
                        continue;
                    minValue = Math.Min(minValue, v);
                    maxValue = Math.Max(maxValue, v);
                }

                if (scaleMin == float.MaxValue)
                    scaleMin = minValue;
                if (scaleMax == float.MaxValue)
                    scaleMax = maxValue;
            }

            var normalize = (float x, float scaleMin, float scaleMax) => {
                if (scaleMax == scaleMin) {
                    return scaleMin; // Handle case where all values are the same
                }
                return (x - scaleMin) / (scaleMax - scaleMin); // Normalized to [0, 1]
            };

            CheckTemporaryVertexBuffer(requiredVertices);
            CheckTemporaryIndexBuffer(requiredIndices);

            int pointCount = count * 2;
            int vertexIndex = 0;
            int indiceIndex = 0;

            for(int i = 0; i < valuesCount -1; i++) 
            {
                float x1 = position.X + ((i+0) * step);
                float x2 = position.X + ((i+1) * step);
                float y1 = position.Y + (normalize(data[i], scaleMin, scaleMax) * plotHeight);
                float y2 = position.Y + (normalize(data[i+1], scaleMin, scaleMax) * plotHeight);

                Vector2 p1 = new Vector2(x1, y1);
                Vector2 p2 = new Vector2(x2, y2);

                Vector2 direction = new Vector2(p2.X - p1.X, p2.Y - p1.Y);

                float length = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);

                if (length == 0) 
                    return;

                direction.X /= length;
                direction.Y /= length;

                Vector2 perpendicular = new Vector2(-direction.Y * thickness * 0.5f, 
                                                     direction.X * thickness * 0.5f);

                vertexBufferTemp[vertexIndex+0] = new Vertex2D(new Vector2(p1.X + perpendicular.X, p1.Y + perpendicular.Y), new Vector2(0, 1), color);
                vertexBufferTemp[vertexIndex+1] = new Vertex2D(new Vector2(p1.X - perpendicular.X, p1.Y - perpendicular.Y), new Vector2(0, 0), color);
                vertexBufferTemp[vertexIndex+2] = new Vertex2D(new Vector2(p2.X - perpendicular.X, p2.Y - perpendicular.Y), new Vector2(1, 0), color);
                vertexBufferTemp[vertexIndex+3] = new Vertex2D(new Vector2(p2.X + perpendicular.X, p2.Y + perpendicular.Y), new Vector2(1, 1), color);

                indexBufferTemp[indiceIndex+0] = (uint)(0 + vertexIndex);
                indexBufferTemp[indiceIndex+1] = (uint)(1 + vertexIndex);
                indexBufferTemp[indiceIndex+2] = (uint)(2 + vertexIndex);
                indexBufferTemp[indiceIndex+3] = (uint)(0 + vertexIndex);
                indexBufferTemp[indiceIndex+4] = (uint)(2 + vertexIndex);
                indexBufferTemp[indiceIndex+5] = (uint)(3 + vertexIndex);

                vertexIndex += 4;
                indiceIndex += 6;
            }

            Span<Vertex2D> pVertexBufferTemp = CollectionsMarshal.AsSpan<Vertex2D>(vertexBufferTemp).Slice(0, requiredVertices);
            Span<uint> pIndexBufferTemp = CollectionsMarshal.AsSpan<uint>(indexBufferTemp).Slice(0, requiredIndices);

            DrawCommand command = new DrawCommand();
            command.vertices = pVertexBufferTemp;
            command.indices = pIndexBufferTemp;
            command.numVertices = requiredVertices;
            command.numIndices = requiredIndices;
            command.textureId = textureId;
            command.textureIsFont = false;
            command.shaderId = shaderId;
            command.clippingRect = clippingRect;
            command.userData = userData;

            AddVertices(command);
        }

        public static void AddImage(Vector2 position, Vector2 size, float rotationDegrees, int textureId, Color color, Vector2 uv0 = default(Vector2), Vector2 uv1 = default(Vector2), Rectangle clippingRect = default(Rectangle), int shaderId = 0, object userData = null)
        {
            if(uv0 == Vector2.Zero && uv1 == Vector2.Zero)
            {
                uv0 = new Vector2(0, 0);
                uv1 = new Vector2(1, 1);
            }

            Vector2 uvTopLeft = new Vector2(uv0.X, uv0.Y);
            Vector2 uvBottomLeft = new Vector2(uv0.X, uv1.Y);
            Vector2 uvBottomRight = new Vector2(uv1.X, uv1.Y);
            Vector2 uvTopRight = new Vector2(uv1.X, uv0.Y);

            Vertex2D[] vertices = 
            {
                new Vertex2D(new Vector2(position.X, position.Y), uvTopLeft, color), // top left
                new Vertex2D(new Vector2(position.X, position.Y + size.Y), uvBottomLeft, color), // bottom left
                new Vertex2D(new Vector2(position.X + size.X, position.Y + size.Y), uvBottomRight, color), // bottom right
                new Vertex2D(new Vector2(position.X + size.X, position.Y), uvTopRight, color)  // top right
            };

            if(rotationDegrees != 0.0f)
                RotateVertices(vertices, 4, rotationDegrees);

            uint[] indices = 
            {
                0, 1, 2, 
                0, 2, 3
            };

            DrawCommand command = new DrawCommand();
            command.vertices = vertices;
            command.indices = indices;
            command.numVertices = 4;
            command.numIndices = 6;
            command.textureId = textureId;
            command.textureIsFont = false;
            command.shaderId = shaderId;
            command.clippingRect = clippingRect;
            command.userData = userData;

            AddVertices(command);
        }

        public static void AddText(Vector2 position, Font font, string text, float fontSize, Color color, Rectangle clippingRect = default(Rectangle)) 
        {
            if(font == null || string.IsNullOrEmpty(text))
                return;

            int requiredVertices = text.Length * 4; // 4 vertices per character
            int requiredIndices = text.Length * 6; // 6 indices per character

            CheckTemporaryVertexBuffer(requiredVertices);
            CheckTemporaryIndexBuffer(requiredIndices);
            
            int vertexIndex = 0;
            int indiceIndex = 0;
            Color currentColor = color;
            
            Vector2 pos = new Vector2(position.X, position.Y);
            pos.Y += font.CalculateYOffset(text, text.Length, fontSize);    
            float originX = pos.X;
            float originY = pos.Y;
            float scale = font.GetPixelScale(fontSize);

            for(int i = 0; i < text.Length; i++) {
                char ch = text[i];

                if(ch == '\n') 
                {
                    pos.X = originX;
                    pos.Y += font.MaxHeight * scale;
                    continue;
                }

                if(!font.GetGlyph(ch, out Glyph glyph))
                    continue;

                float xpos = pos.X + glyph.bearingX * scale;
                float ypos = pos.Y - glyph.bearingY * scale;
                float w = glyph.sizeX * scale;
                float h = glyph.sizeY * scale;

                // top-right, top-left, bottom-left, bottom-right
                Vector2[] glyphVertices = 
                {
                    new Vector2(xpos + w, ypos + h),
                    new Vector2(xpos,     ypos + h),
                    new Vector2(xpos,     ypos),
                    new Vector2(xpos + w, ypos)
                };

                Vector2[] glyphTextureCoords = {
                    new Vector2(glyph.u1,  glyph.v1),
                    new Vector2(glyph.u0,  glyph.v1),
                    new Vector2(glyph.u0,  glyph.v0),
                    new Vector2(glyph.u1,  glyph.v0)
                };

                vertexBufferTemp[vertexIndex+0] = new Vertex2D(new Vector2(glyphVertices[0].X, glyphVertices[0].Y), glyphTextureCoords[0], currentColor);
                vertexBufferTemp[vertexIndex+1] = new Vertex2D(new Vector2(glyphVertices[1].X, glyphVertices[1].Y), glyphTextureCoords[1], currentColor);
                vertexBufferTemp[vertexIndex+2] = new Vertex2D(new Vector2(glyphVertices[2].X, glyphVertices[2].Y), glyphTextureCoords[2], currentColor);
                vertexBufferTemp[vertexIndex+3] = new Vertex2D(new Vector2(glyphVertices[3].X, glyphVertices[3].Y), glyphTextureCoords[3], currentColor);

                // indexBufferTemp[indiceIndex+0] = (uint)(0 + vertexIndex);
                // indexBufferTemp[indiceIndex+1] = (uint)(1 + vertexIndex);
                // indexBufferTemp[indiceIndex+2] = (uint)(2 + vertexIndex);
                // indexBufferTemp[indiceIndex+3] = (uint)(0 + vertexIndex);
                // indexBufferTemp[indiceIndex+4] = (uint)(2 + vertexIndex);
                // indexBufferTemp[indiceIndex+5] = (uint)(3 + vertexIndex);

                indexBufferTemp[indiceIndex+0] = (uint)(0 + vertexIndex); // Bottom-right
                indexBufferTemp[indiceIndex+1] = (uint)(2 + vertexIndex); // Top-left
                indexBufferTemp[indiceIndex+2] = (uint)(1 + vertexIndex); // Top-right
                indexBufferTemp[indiceIndex+3] = (uint)(0 + vertexIndex); // Bottom-right
                indexBufferTemp[indiceIndex+4] = (uint)(3 + vertexIndex); // Bottom-left
                indexBufferTemp[indiceIndex+5] = (uint)(2 + vertexIndex); // Top-left

                vertexIndex += 4;
                indiceIndex += 6;

                pos.X += glyph.advanceX * scale;
            }

            Span<Vertex2D> pVertexBufferTemp = CollectionsMarshal.AsSpan<Vertex2D>(vertexBufferTemp).Slice(0, requiredVertices);
            Span<uint> pIndexBufferTemp = CollectionsMarshal.AsSpan<uint>(indexBufferTemp).Slice(0, requiredIndices);

            DrawCommand command = new DrawCommand();
            command.vertices = pVertexBufferTemp;
            command.indices = pIndexBufferTemp;
            command.numVertices = vertexIndex;
            command.numIndices = indiceIndex;
            command.textureId = font.TextureId;
            command.textureIsFont = true;
            command.shaderId = shaderId;
            command.clippingRect = clippingRect;
            command.userData = null;

            AddVertices(command);
        }

        private static void AddVertices(DrawCommand command) 
        {
            CheckVertexBuffer(command.numVertices);
            CheckIndexBuffer(command.numIndices);
            CheckItemBuffer(1);

            Span<Vertex2D> pVertices = CollectionsMarshal.AsSpan(vertices).Slice(vertexCount, command.numVertices);

            command.vertices.CopyTo(pVertices);
            
            for(int i = 0; i < command.numIndices; i++) 
                indices[indiceCount+i] = command.indices[i] + (uint)vertexCount;

            DrawListItem drawListItem = items[itemCount];

            drawListItem.vertexCount = command.numVertices;
            drawListItem.indiceCount = command.numIndices;
            drawListItem.vertexOffset = vertexCount;
            drawListItem.indiceOffset = indiceCount;
            drawListItem.shaderId = command.shaderId == 0 ? shaderId : command.shaderId;
            drawListItem.textureId = command.textureId;
            drawListItem.textureIsFont = command.textureIsFont;
            drawListItem.clippingRect = command.clippingRect;
            drawListItem.userData = command.userData;

            Rectangle rect = drawListItem.clippingRect;

            if(!rect.IsZero()) 
            {
                var viewport = Graphics.GetViewport();
                rect.y = viewport.height - rect.y - rect.height;
                drawListItem.clippingRect = rect;
            }
            
            items[itemCount] = drawListItem;

            itemCount++;
            vertexCount += command.numVertices;
            indiceCount += command.numIndices;
        }

        private static void RotateVertices(Span<Vertex2D> vertices, int numVertices, float angleDegrees) 
        {
            float centerX = 0.0f;
            float centerY = 0.0f;

            for (int i = 0; i < numVertices; ++i) 
            {
                centerX += vertices[i].position.X;
                centerY += vertices[i].position.Y;
            }

            centerX /= numVertices;
            centerY /= numVertices;

            float radians = angleDegrees * ((float)Math.PI / 180.0f);
            float cosAngle = (float)Math.Cos(radians);
            float sinAngle = (float)Math.Sin(radians);

            for (int i = 0; i < numVertices; ++i) 
            {
                float translatedX = vertices[i].position.X - centerX;
                float translatedY = vertices[i].position.Y - centerY;

                float rotatedX = translatedX * cosAngle - translatedY * sinAngle;
                float rotatedY = translatedX * sinAngle + translatedY * cosAngle;

                vertices[i].position.X = rotatedX + centerX;
                vertices[i].position.Y = rotatedY + centerY;
            }
        }

        private static void CheckVertexBuffer(int numRequiredVertices) 
        {
            int verticesNeeded = vertexCount + numRequiredVertices;
            
            if(verticesNeeded > vertices.Count) 
            {
                int newSize = vertices.Count * 2;

                while(newSize < verticesNeeded) 
                {
                    newSize *= 2;
                }

                vertices.Capacity = newSize;
                
                GL.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);
                
                IntPtr size = new IntPtr(newSize * Marshal.SizeOf<Vertex2D>());
                GL.BufferData(BufferTargetARB.ArrayBuffer, size, IntPtr.Zero, BufferUsageARB.DynamicDraw);
                
                GL.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            }
        }

        private static void CheckIndexBuffer(int numRequiredIndices) 
        {
            int indicesNeeded = indiceCount + numRequiredIndices;
            
            if(indicesNeeded > indices.Count) 
            {
                int newSize = indices.Count * 2;

                while(newSize < indicesNeeded) 
                {
                    newSize *= 2;
                }

                indices.Capacity = newSize;

                GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, EBO);
                
                IntPtr size = new IntPtr(newSize * Marshal.SizeOf<uint>());
                GL.BufferData(BufferTargetARB.ElementArrayBuffer, size, IntPtr.Zero, BufferUsageARB.DynamicDraw);
                
                GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
            }
        }

        private static void CheckItemBuffer(int numRequiredItems) 
        {
            int itemsNeeded = itemCount + numRequiredItems;

            if(itemsNeeded > items.Count) 
            {
                int newSize = items.Count * 2;
                
                while(newSize < itemsNeeded) 
                {
                    newSize *= 2;
                }

                items.Capacity = newSize;
            }
        }

        private static void CheckTemporaryVertexBuffer(int numRequiredVertices) 
        {
            if(vertexBufferTemp.Count < numRequiredVertices) 
            {
                int newSize = vertexBufferTemp.Count * 2;
                
                while(newSize < numRequiredVertices) 
                {
                    newSize *= 2;
                }

                vertexBufferTemp.Capacity = newSize;
            }
        }

        private static void CheckTemporaryIndexBuffer(int numRequiredIndices) 
        {
            if(indexBufferTemp.Count < (numRequiredIndices)) 
            {
                int newSize = indexBufferTemp.Count * 2;
            
                while(newSize < numRequiredIndices) 
                {
                    newSize *= 2;
                }

                indexBufferTemp.Capacity = newSize;
            }
        }

        private static unsafe void StoreState() 
        {
            glState.depthTestEnabled = GL.IsEnabled(EnableCap.DepthTest);
            glState.blendEnabled = GL.IsEnabled(EnableCap.Blend);
            
            fixed(int *pBlendSrcFactor = &glState.blendSrcFactor)
                GL.GetIntegerv(GetPName.BlendSrc, pBlendSrcFactor);

            fixed(int *pBlendDstFactor = &glState.blendDstFactor)
                GL.GetIntegerv(GetPName.BlendDst, pBlendDstFactor);

            fixed(int *pBlendEquation = &glState.blendEquation)
                GL.GetIntegerv(GetPName.BlendEquation, pBlendEquation);

            fixed(int *pDepthFunc = &glState.depthFunc)
                GL.GetIntegerv(GetPName.DepthFunc, pDepthFunc);
        }

        private static void RestoreState() 
        {
            if (glState.depthTestEnabled)
                GL.Enable(EnableCap.DepthTest);
            else
                GL.Disable(EnableCap.DepthTest);

            if (glState.blendEnabled)
                GL.Enable(EnableCap.Blend);
            else
                GL.Disable(EnableCap.Blend);

            GL.BlendFunc((BlendingFactor)glState.blendSrcFactor, (BlendingFactor)glState.blendDstFactor);
            GL.BlendEquation((BlendEquationModeEXT)glState.blendEquation);
            GL.DepthFunc((DepthFunction)glState.depthFunc);
        }

        private static void CreateBuffers() 
        {
            const uint size = 2 << 15;

            for(uint i = 0; i < size; i++)
            {
                items.Add(new DrawListItem());
                vertices.Add(new Vertex2D());
                indices.Add(0);
                vertexBufferTemp.Add(new Vertex2D());
                indexBufferTemp.Add(0);
            }

            GL.GenVertexArrays(1, ref VAO);
            GL.GenBuffers(1, ref VBO);
            GL.GenBuffers(1, ref EBO);

            GL.ObjectLabel(ObjectIdentifier.Buffer, (uint)VBO, -1, "Graphics2D_VBO");
            GL.ObjectLabel(ObjectIdentifier.Buffer, (uint)EBO, -1, "Graphics2D_EBO");

            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);

            IntPtr vSize = new IntPtr(vertices.Count * Marshal.SizeOf<Vertex2D>());
            GL.BufferData(BufferTargetARB.ArrayBuffer, vSize, IntPtr.Zero, BufferUsageARB.DynamicDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex2D>(), Marshal.OffsetOf<Vertex2D>("position"));
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex2D>(), Marshal.OffsetOf<Vertex2D>("uv"));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex2D>(), Marshal.OffsetOf<Vertex2D>("color"));
            GL.EnableVertexAttribArray(2);

            GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, EBO);
            
            vSize = new IntPtr(indices.Count * Marshal.SizeOf<uint>());
            GL.BufferData(BufferTargetARB.ElementArrayBuffer, vSize, IntPtr.Zero, BufferUsageARB.DynamicDraw);

            GL.BindVertexArray(0);
        }

        private static void CreateShader()
        {
            string vertexSource = @"#version 330 core
            layout(location = 0) in vec2 aPosition;
            layout(location = 1) in vec2 aTexCoord;
            layout(location = 2) in vec4 aColor;

            uniform mat4 uProjection;
            out vec2 oTexCoord;
            out vec4 oColor;

            void main() {
                gl_Position = uProjection * vec4(aPosition.x, aPosition.y, 0.0, 1.0);
                oTexCoord = aTexCoord;
                oColor = aColor;
            }";

            string fragmentSource = @"#version 330 core
            uniform sampler2D uTexture;
            uniform float uTime;
            uniform int uIsFont;

            in vec2 oTexCoord;
            in vec4 oColor;
            out vec4 FragColor;

            void main() {
                if(uIsFont > 0) {
                    vec4 sample = texture(uTexture, oTexCoord);
                    float d = sample.r;
                    float aaf = fwidth(d);
                    float alpha = smoothstep(0.5 - aaf, 0.5 + aaf, d);
                    FragColor = vec4(oColor.rgb, alpha) * oColor;
                    //FragColor = vec4(1);
                } else {
                    FragColor = texture(uTexture, oTexCoord) * oColor;
                }
            }";

            int vert_handle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vert_handle, vertexSource);
            GL.CompileShader(vert_handle);
            CheckShader(vert_handle, "vertex shader");

            int frag_handle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(frag_handle, fragmentSource);
            GL.CompileShader(frag_handle);
            CheckShader(frag_handle, "fragment shader");

            shaderId = GL.CreateProgram();
            GL.AttachShader(shaderId, vert_handle);
            GL.AttachShader(shaderId, frag_handle);
            GL.LinkProgram(shaderId);
            CheckProgram(shaderId, "shader program");

            GL.DetachShader(shaderId, vert_handle);
            GL.DetachShader(shaderId, frag_handle);
            GL.DeleteShader(vert_handle);
            GL.DeleteShader(frag_handle);

            uniforms[(int)Uniform.Texture] = GL.GetUniformLocation(shaderId, "uTexture");
            uniforms[(int)Uniform.Projection] = GL.GetUniformLocation(shaderId, "uProjection");
            uniforms[(int)Uniform.IsFont] = GL.GetUniformLocation(shaderId, "uIsFont");
            uniforms[(int)Uniform.Time] = GL.GetUniformLocation(shaderId, "uTime");
        }

        private static void CreateTexture() 
        {
            byte[] textureData = new byte[16];
            Array.Fill<byte>(textureData, 255);

            GL.GenTextures(1, ref textureId);
            GL.BindTexture(TextureTarget.Texture2d, textureId);

            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, 2, 2, 0, PixelFormat.Rgba, PixelType.UnsignedByte, textureData);
        }

        private static unsafe bool CheckShader(int handle, string desc) 
        {
            int status = 0;
            int log_length = 0;

            GL.GetShaderiv(handle, ShaderParameterName.CompileStatus, &status);
            GL.GetShaderiv(handle, ShaderParameterName.InfoLogLength, &log_length);
            
            if(status == 0)
            {
                Console.WriteLine("ERROR: failed to compile " + desc);
            }
            if (log_length > 1) 
            {
                GL.GetShaderInfoLog(handle, out string infoLog);
                Console.WriteLine("ERROR: " + infoLog);
            }

            return status > 0;
        }

        private static unsafe bool CheckProgram(int handle, string desc) 
        {
            int status = 0;
            int log_length = 0;

            GL.GetProgramiv(handle, ProgramPropertyARB.LinkStatus, &status);
            GL.GetProgramiv(handle, ProgramPropertyARB.InfoLogLength, &log_length);
            
            if(status == 0)
            {
                Console.WriteLine("ERROR: failed to link " + desc);
            }
            if (log_length > 1) 
            {
                GL.GetShaderInfoLog(handle, out string infoLog);
                Console.WriteLine("ERROR: " + infoLog);
            }

            return status > 0;
        }
    }

    public struct DrawListItem 
    {
        public int shaderId;
        public int textureId;
        public int vertexOffset;
        public int vertexCount;
        public int indiceCount;
        public int indiceOffset;
        public bool textureIsFont;
        public Rectangle clippingRect;
        public object userData;
    }

    public ref struct DrawCommand 
    {
        public ReadOnlySpan<Vertex2D> vertices;
        public int numVertices;
        public ReadOnlySpan<uint> indices;
        public int numIndices;
        public int textureId;
        public int shaderId;
        public bool textureIsFont;
        public Rectangle clippingRect;
        public object userData;

        public DrawCommand()
        {
            this.vertices = null;
            this.numVertices = 0;
            this.indices = null;
            this.numIndices = 0; 
            this.textureId = 0; 
            this.textureIsFont = false;
            this.clippingRect = new Rectangle(0, 0, 0, 0);
            this.userData = null;
        }
    }

    public struct GLStateInfo 
    {
        public bool depthTestEnabled;
        public bool blendEnabled;
        public int blendSrcFactor;
        public int blendDstFactor;
        public int blendEquation;
        public int depthFunc;
    }

    public enum Uniform : int
    {
        Projection,
        Texture,
        Time,
        IsFont,
        COUNT
    }

    [Flags]
    public enum BorderOptions : int
    {
        Left = 1 << 0,
        Right = 1 << 1,
        Top = 1 << 2,
        Bottom = 1 << 3,
        All = Left | Right | Top | Bottom
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex2D 
    {
        public Vector2 position;
        public Vector2 uv;
        public Color color;
        
        public Vertex2D()
        {
            this.position = new Vector2(0, 0);
            this.uv = new Vector2(0, 0);
            this.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }

        public Vertex2D(Vector2 position, Vector2 uv)
        {
            this.position = position;
            this.uv = uv;
            this.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }

        public Vertex2D(Vector2 position, Vector2 uv, Color color)
        {
            this.position = position;
            this.uv = uv;
            this.color = color;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rectangle 
    {
        public float x;
        public float y;
        public float width;
        public float height;
        
        public Rectangle()
        {
            x = 0;
            y = 0;
            width = 0;
            height = 0;
        }

        public Rectangle(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
        
        public bool IsZero() 
        {
            return x == 0.0f && y == 0.0f && width == 0.0f && height == 0.0f;
        }
        
        public static Rectangle GetRectAtRowAndColumn(float leftIndent, float topIndent, float width, float height, int row, int column, int offsetX = 0, int offsetY = 0) 
        {
            float x = leftIndent + (column * (width + offsetX));
            float y = topIndent + (row * (height + offsetY));
            return new Rectangle(x, y, width, height);
        }
    }

    public delegate void UniformUpdateCallback(int shaderId, object userData);
}