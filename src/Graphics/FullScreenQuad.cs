using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Gowtu
{
    public sealed class FullScreenQuad
    {
        private VertexArrayObject vao;
        private VertexBufferObject vbo;
        private Shader shader;

        private static readonly float[] vertices = new float[]
        {
            // Positions          // Texture Coords
            -1.0f,  1.0f, 0.0f,   0.0f, 1.0f,
            -1.0f, -1.0f, 0.0f,   0.0f, 0.0f,
            1.0f, -1.0f, 0.0f,   1.0f, 0.0f,

            -1.0f,  1.0f, 0.0f,   0.0f, 1.0f,
            1.0f, -1.0f, 0.0f,   1.0f, 0.0f,
            1.0f,  1.0f, 0.0f,   1.0f, 1.0f
        };

        private string vertexSource = @"#version 330 core
        layout(location = 0) in vec3 aPosition;
        layout(location = 1) in vec2 aTexCoord;

        uniform mat4 uProjection;
        out vec2 oTexCoord;

        void main()
        {
            gl_Position = uProjection * vec4(aPosition, 1.0);
            oTexCoord = aTexCoord;
        }";

        private string fragmentSource = @"#version 330 core
        uniform sampler2D uTexture;        
        in vec2 oTexCoord;
        out vec4 FragColor;

        void main() {
            //FragColor = texture(uTexture, oTexCoord) * vec4(1, 0, 0, 1);
            FragColor = vec4(1, 0, 0, 1);
        }";

        public FullScreenQuad()
        {
            vao = new VertexArrayObject();
            vbo = new VertexBufferObject();
        }

        public void Generate()
        {
            vao.Generate();
            vbo.Generate();

            vao.Bind();
            
            vbo.Bind();
            vbo.BufferData<float>(vertices, BufferUsageARB.StaticDraw);

            vao.EnableVertexAttribArray(0);
            vao.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), IntPtr.Zero);

            vao.EnableVertexAttribArray(1);
            vao.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), Marshal.SizeOf<Vector3>());

            vao.Unbind();
            vbo.Unbind();

            shader = new Shader(vertexSource, fragmentSource);
        }

        public void Render(FrameBufferObject frameBufferObject)
        {
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

            shader.Use();

            unsafe 
            {
                fixed(float *pProjectionMatrix = &projectionMatrix[0,0])
                {
                    GL.UniformMatrix4fv(GL.GetUniformLocation(shader.Id, "uProjection"), 1, false, pProjectionMatrix);
                }
            }

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2d, frameBufferObject.ColorTexture);
            shader.SetInt("uTexture", 0);

            vao.Bind();
            GL.DrawArrays(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, 0, 6);
            vao.Unbind();
        }
    }
}