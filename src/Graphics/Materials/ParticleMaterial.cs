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
    public sealed class ParticleMaterial : Material
    {
        private int uDiffuseTexture;
        private int uUVOffset;
        private int uUVScale;
        private int uAmbientStrength;
        private int uShininess;
        private int uAlphaCutOff;
        private int uDepthMap;
        private int uReceiveShadows;

        private Texture2D diffuseTexture;
        private Texture2DArray depthMap;
        private Vector2 uvScale;
        private Vector2 uvOffset;
        private float ambientStrength;
        private float shininess;
        private float alphaCutOff;
        private bool receiveShadows;

        public Texture2D DiffuseTexture
        {
            get
            {
                return diffuseTexture;
            }
            set
            {
                diffuseTexture = value;
            }
        }

        public Vector2 UVScale
        {
            get
            {
                return uvScale;
            }
            set
            {
                uvScale = value;
            }
        }

        public Vector2 UVOffset
        {
            get
            {
                return uvOffset;
            }
            set
            {
                uvOffset = value;
            }
        }

        public float AlphaCutOff
        {
            get
            {
                return alphaCutOff;
            }
            set
            {
                alphaCutOff = value;
            }
        }

        public ParticleMaterial() : base()
        {
            shader = Resources.FindShader(Constants.GetString(ConstantString.ShaderParticle));

            diffuseTexture = Resources.FindTexture<Texture2D>(Constants.GetString(ConstantString.TextureDefault));
            depthMap = Resources.FindTexture<Texture2DArray>(Constants.GetString(ConstantString.TextureDepth));
            uvScale = new Vector2(1, 1);
            uvOffset = new Vector2(0, 0);
            alphaCutOff = 0.0f;
            ambientStrength = 0.5f;
            shininess = 16.0f;
            receiveShadows = false;

            if(shader != null)
            {
                uDiffuseTexture = GL.GetUniformLocation(shader.Id, "uDiffuseTexture");
                uUVOffset = GL.GetUniformLocation(shader.Id, "uUVOffset");
                uUVScale = GL.GetUniformLocation(shader.Id, "uUVScale");
                uAmbientStrength = GL.GetUniformLocation(shader.Id, "uAmbientStrength");
                uShininess = GL.GetUniformLocation(shader.Id, "uShininess");
                uAlphaCutOff = GL.GetUniformLocation(shader.Id, "uAlphaCutOff");
                uDepthMap = GL.GetUniformLocation(shader.Id, "uDepthMap");
                uReceiveShadows = GL.GetUniformLocation(shader.Id, "uReceiveShadows");
            }
        }

        public override void Use(Transform transform, Camera camera)
        {
            if(shader == null || camera == null || transform == null)
                return;

            shader.Use();

            int unit = 0;

            if(diffuseTexture != null)
            {
                diffuseTexture.Bind(unit);
                shader.SetInt(uDiffuseTexture, unit);
                unit++;
            }

            if(depthMap != null)
            {
                depthMap.Bind(unit);
                shader.SetInt(uDepthMap, unit);
                unit++;
            }

            shader.SetFloat2(uUVScale, uvScale);
            shader.SetFloat2(uUVOffset, uvOffset);
            shader.SetFloat(uAlphaCutOff, alphaCutOff);
            shader.SetFloat(uAmbientStrength, ambientStrength);
            shader.SetFloat(uShininess, shininess);
            shader.SetInt(uReceiveShadows, receiveShadows ? 1 : 0);
        }
    }
}