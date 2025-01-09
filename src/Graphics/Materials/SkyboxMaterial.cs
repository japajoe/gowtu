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

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Gowtu
{
    public class SkyboxMaterial : Material
    {
        private int uModel;
        private int uView;
        private int uProjection;
        private int uTexture;
        private int uDiffuseColor;

        private TextureCubeMap texture;
        private Color diffuseColor;

        public TextureCubeMap Texture
        {
            get
            {
                return texture;
            }
            set
            {
                texture = value;
            }
        }

        public Color DiffuseColor
        {
            get
            {
                return diffuseColor;
            }
            set
            {
                diffuseColor = value;
            }
        }

        public SkyboxMaterial() : base()
        {
            shader = Resources.FindShader(Constants.GetString(ConstantString.ShaderSkybox));

            texture = Resources.FindTexture<TextureCubeMap>(Constants.GetString(ConstantString.TextureDefaultCubeMap));
            diffuseColor = Color.White;

            if(shader != null)
            {
                uModel = GL.GetUniformLocation(shader.Id, "uModel");
                uView = GL.GetUniformLocation(shader.Id, "uView");
                uProjection = GL.GetUniformLocation(shader.Id, "uProjection");
                uProjection = GL.GetUniformLocation(shader.Id, "uProjection");
                uTexture = GL.GetUniformLocation(shader.Id, "uTexture");
                uDiffuseColor = GL.GetUniformLocation(shader.Id, "uDiffuseColor");
            }
        }

        public override void Use(Transform transform, Camera camera)
        {
            if(shader == null || camera == null || transform == null)
                return;

            Matrix4 projection = camera.GetProjectionMatrix();
            Matrix4 view = camera.GetViewMatrix();
            Matrix4 model = transform.GetModelMatrix();

            shader.Use();

            shader.SetMat4(uModel, model);
            shader.SetMat4(uView, view);
            shader.SetMat4(uProjection, projection);
            shader.SetFloat4(uDiffuseColor, diffuseColor);

            int unit = 0;

            if(texture != null)
            {
                texture.Bind(unit);
                shader.SetInt(uTexture, unit);
                unit++;
            }
        }
    }
}