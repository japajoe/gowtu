using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Gowtu
{
    public static class LineRenderer
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct LineVertex
        {
            public Vector3 position;
            public Color color;

            public LineVertex(Vector3 position, Color color)
            {
                this.position = position;
                this.color = color;
            }
        }

        private static LineVertex[] lines;
        private static int numLines = 0;
        private static int pointIndex = 0;
        private static int maxLines = 128;
        private static VertexArrayObject VAO;
        private static VertexBufferObject VBO;
        private static Shader shader;
        private static int uMVP;

        static LineRenderer()
        {
            Initialize();
        }

        private static void Initialize()
        {
            string vertexSource = @"#version 330 core
            layout (location = 0) in vec3 aPosition;
            layout (location = 1) in vec4 aColor;

            uniform mat4 uMVP;

            out vec4 oColor;

            void main() {
                gl_Position = uMVP * vec4(aPosition, 1.0);
                oColor = aColor;
            }";

            string fragmentSource = @"#version 330 core
            in vec4 oColor;
            out vec4 FragColor;

            vec4 gamma_correction(vec4 color) {
                return vec4(pow(vec3(color.xyz), vec3(1.0/2.2)), color.a);
            }

            void main() {
                FragColor = gamma_correction(oColor);
            }";

            shader = new Shader(vertexSource, fragmentSource);

            uMVP = GL.GetUniformLocation(shader.Id, "uMVP");

            int maxVertices = maxLines * 2;

            lines = new LineVertex[maxVertices];

            for(int i = 0; i < lines.Length; i++)
            {
                lines[i] = new LineVertex(Vector3.Zero, Color.White);
            }

            VAO = new VertexArrayObject();
            VBO = new VertexBufferObject();

            VAO.Generate();
            VBO.Generate();

            VAO.Bind();

            VBO.Bind();
            VBO.BufferData<LineVertex>(lines, BufferUsageARB.DynamicDraw);

            VAO.EnableVertexAttribArray(0);
            VAO.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf(typeof(LineVertex)), Marshal.OffsetOf(typeof(LineVertex), "position"));

            VAO.EnableVertexAttribArray(1);
            VAO.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf(typeof(LineVertex)), Marshal.OffsetOf(typeof(LineVertex), "color"));

            VAO.Unbind();
            VBO.Unbind();
        }        
        
        internal static void AddToDrawList(Vector3 p1, Vector3 p2, Color color)
        {
            if(lines == null)
            {
                Initialize();
            }

            if(numLines >= maxLines)
            {
                //Double the capacity if we run out of space
                maxLines = maxLines * 2;
                int maxVertices = maxLines * 2;
                var newLines = new LineVertex[maxVertices];
                for(int i = 0; i < lines.Length; i++)
                {
                    newLines[i] = lines[i];
                }

                lines = newLines;
                
                VBO.Bind();
                VBO.BufferData<LineVertex>(lines, BufferUsageARB.DynamicDraw);
                VBO.Unbind();
            }

            var lp1 = new LineVertex(p1, color);
            var lp2 = new LineVertex(p2, color);

            lines[pointIndex+0] = lp1;
            lines[pointIndex+1] = lp2;
            
            pointIndex += 2;
            numLines++;
        }

        internal static void OnRender()
        {
            if(numLines == 0)
                return;

            int numVertices = numLines * 2;

            var pData = new ReadOnlySpan<LineVertex>(lines).Slice(0, numVertices);
            VBO.Bind();
            VBO.BufferSubData<LineVertex>(pData, 0);
            VBO.Unbind();

            Matrix4 mvp = Matrix4.Identity * Camera.mainCamera.GetViewMatrix() * Camera.mainCamera.GetProjectionMatrix();

            GL.Enable(EnableCap.DepthTest);

            shader.Use();

            shader.SetMat4(uMVP, mvp);

            VAO.Bind();
            GL.DrawArrays(OpenTK.Graphics.OpenGL.PrimitiveType.Lines, 0, numVertices);
            VAO.Unbind();

            GL.UseProgram(0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2d, 0);
            GL.Disable(EnableCap.DepthTest);

            //Done rendering, clear lines
            pointIndex = 0;
            numLines = 0;
        }
    }
}