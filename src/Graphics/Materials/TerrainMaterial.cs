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
    public sealed class TerrainMaterial : Material
    {
        private int uModel;
        private int uModelInverted;
        private int uMVP;
        private int uSplatMap;
        private int uTexture1;
        private int uTexture2;
        private int uTexture3;
        private int uTexture4;
        private int uUVScale1;
        private int uUVScale2;
        private int uUVScale3;
        private int uUVScale4;
        private int uAmbientStrength;
        private int uShininess;
        private int uDiffuseColor;
        private int uDepthMap;
        private int uReceiveShadows;

        private Texture2D splatMap;
        private Texture2D texture1;
        private Texture2D texture2;
        private Texture2D texture3;
        private Texture2D texture4;    
        private Texture2DArray depthMap;    
        private Vector2 uvScale1;
        private Vector2 uvScale2;
        private Vector2 uvScale3;
        private Vector2 uvScale4;
        private Color diffuseColor;
        private float ambientStrength;
        private float shininess;
        private bool receiveShadows;

        public Texture2D SplatMap
        {
            get
            {
                return splatMap;
            }
            set
            {
                splatMap = value;
            }
        }

        public Texture2D Texture1
        {
            get
            {
                return texture1;
            }
            set
            {
                texture1 = value;
            }
        }

        public Texture2D Texture2
        {
            get
            {
                return texture2;
            }
            set
            {
                texture2 = value;
            }
        }

        public Texture2D Texture3
        {
            get
            {
                return texture3;
            }
            set
            {
                texture3 = value;
            }
        }

        public Texture2D Texture4
        {
            get
            {
                return texture4;
            }
            set
            {
                texture4 = value;
            }
        }

        public Vector2 UvScale1
        {
            get
            {
                return uvScale1;
            }
            set
            {
                uvScale1 = value;
            }
        }

        public Vector2 UvScale2
        {
            get
            {
                return uvScale2;
            }
            set
            {
                uvScale2 = value;
            }
        }

        public Vector2 UvScale3
        {
            get
            {
                return uvScale3;
            }
            set
            {
                uvScale3 = value;
            }
        }

        public Vector2 UvScale4
        {
            get
            {
                return uvScale4;
            }
            set
            {
                uvScale4 = value;
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

        public float AmbientStrength
        {
            get
            {
                return ambientStrength;
            }
            set
            {
                ambientStrength = value;
            }
        }

        public float Shininess
        {
            get
            {
                return shininess;
            }
            set
            {
                shininess = value;
            }
        }

        public bool ReceiveShadows
        {
            get
            {
                return receiveShadows;
            }
            set
            {
                receiveShadows = value;
            }
        }

        public TerrainMaterial() : base()
        {
            var defaultTexture = Resources.FindTexture<Texture2D>(Constants.GetString(ConstantString.TextureDefault));
            
            splatMap = defaultTexture;
            texture1 = defaultTexture;
            texture2 = defaultTexture;
            texture3 = defaultTexture;
            texture4 = defaultTexture;
            
            depthMap = Resources.FindTexture<Texture2DArray>(Constants.GetString(ConstantString.TextureDepth));
            
            diffuseColor = Color.White;
            ambientStrength = 0.5f;
            shininess = 16.0f;
            uvScale1 = new Vector2(1, 1);
            uvScale2 = new Vector2(1, 1);
            uvScale3 = new Vector2(1, 1);
            uvScale4 = new Vector2(1, 1);
            receiveShadows = true;

            shader = Resources.FindShader(Constants.GetString(ConstantString.ShaderTerrain));

            if(shader != null)
            {
                uModel = GL.GetUniformLocation(shader.Id, "uModel");
                uModelInverted = GL.GetUniformLocation(shader.Id, "uModelInverted");
                uMVP = GL.GetUniformLocation(shader.Id, "uMVP");
                uSplatMap = GL.GetUniformLocation(shader.Id, "uSplatMap");
                uTexture1 = GL.GetUniformLocation(shader.Id, "uTexture1");
                uTexture2 = GL.GetUniformLocation(shader.Id, "uTexture2");
                uTexture3 = GL.GetUniformLocation(shader.Id, "uTexture3");
                uTexture4 = GL.GetUniformLocation(shader.Id, "uTexture4");
                uUVScale1 = GL.GetUniformLocation(shader.Id, "uUVScale1");
                uUVScale2 = GL.GetUniformLocation(shader.Id, "uUVScale2");
                uUVScale3 = GL.GetUniformLocation(shader.Id, "uUVScale3");
                uUVScale4 = GL.GetUniformLocation(shader.Id, "uUVScale4");
                uAmbientStrength = GL.GetUniformLocation(shader.Id, "uAmbientStrength");
                uShininess = GL.GetUniformLocation(shader.Id, "uShininess");
                uDiffuseColor = GL.GetUniformLocation(shader.Id, "uDiffuseColor");
                uDepthMap = GL.GetUniformLocation(shader.Id, "uDepthMap");
                uReceiveShadows = GL.GetUniformLocation(shader.Id, "uReceiveShadows");
            }
        }

        public override void Use(Transform transform, Camera camera)
        {
            if(shader == null || camera == null || transform == null)
                return;

            Matrix4 projection = camera.GetProjectionMatrix();
            Matrix4 view = camera.GetViewMatrix();
            Matrix4 model = transform.GetModelMatrix();
            Matrix4 MVP = model * view * projection;
            Matrix3 modelInverted = Matrix3.Transpose(Matrix3.Invert(new Matrix3(model)));

            shader.Use();

            int unit = 0;

            if(splatMap != null)
            {
                splatMap.Bind(unit);
                shader.SetInt(uSplatMap, unit);
                unit++;
            }

            if(texture1 != null)
            {
                texture1.Bind(unit);
                shader.SetInt(uTexture1, unit);
                unit++;
            }

            if(texture2 != null)
            {
                texture2.Bind(unit);
                shader.SetInt(uTexture2, unit);
                unit++;
            }

            if(texture3 != null)
            {
                texture3.Bind(unit);
                shader.SetInt(uTexture3, unit);
                unit++;
            }

            if(texture4 != null)
            {
                texture4.Bind(unit);
                shader.SetInt(uTexture4, unit);
                unit++;
            }

            if(depthMap != null)
            {
                depthMap.Bind(unit);
                shader.SetInt(uDepthMap, unit);
                unit++;
            }

            shader.SetMat4(uModel, model);
            shader.SetMat3(uModelInverted, modelInverted);
            shader.SetMat4(uMVP, MVP);
            shader.SetFloat4(uDiffuseColor, diffuseColor);
            shader.SetFloat(uAmbientStrength, ambientStrength);
            shader.SetFloat(uShininess, shininess);
            shader.SetFloat2(uUVScale1, uvScale1);
            shader.SetFloat2(uUVScale2, uvScale2);
            shader.SetFloat2(uUVScale3, uvScale3);
            shader.SetFloat2(uUVScale4, uvScale4);
            shader.SetInt(uReceiveShadows, receiveShadows ? 1 : 0);
        }
    }
}