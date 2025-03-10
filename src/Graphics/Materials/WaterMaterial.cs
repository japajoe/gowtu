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
    public class WaterMaterial : Material
    {
        private struct WaveUniform
        {
            public int direction;
            public int steepness;
            public int waveLength;
            public int amplitude;
            public int speed;
        }

        public struct Wave
        {
            public Vector2 direction;
            public float steepness;
            public float waveLength;
            public float amplitude;
            public float speed;
        }

        private int uModel;
        private int uModelInverted;
        private int uMVP;
        private int uDiffuseTexture;
        private int uDiffuseColor;
        private int uAmbientStrength;
        private int uShininess;
        private int uUVScale;
        private int uUVOffset;
        private int uDepthMap;
        private int uReceiveShadows;
        private const int MAX_WAVES = 10;
        private WaveUniform[] uWaves;
        private int uNumWaves;

        private Texture2D diffuseTexture;
        private Texture2DArray depthMap;
        private Color diffuseColor;
        private float ambientStrength;
        private float shininess;
        private Vector2 uvScale;
        private Vector2 uvOffset;
        private bool receiveShadows;
        private Wave[] waves;
        private int numWaves;

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

        public int WaveCount
        {
            get
            {
                return numWaves;
            }
            set
            {
                numWaves = Math.Clamp(value, 1, MAX_WAVES);
            }
        }

        public int MaxWaveCount
        {
            get
            {
                return MAX_WAVES;
            }
        }

        public Wave[] Waves
        {
            get
            {
                return waves;
            }
            set
            {
                waves = value;
            }
        }

        public WaterMaterial() : base()
        {
            shader = Resources.FindShader(Constants.GetString(ConstantString.ShaderWater));
            diffuseTexture = Resources.FindTexture<Texture2D>(Constants.GetString(ConstantString.TextureDefault));
            depthMap = Resources.FindTexture<Texture2DArray>(Constants.GetString(ConstantString.TextureDepth));
            diffuseColor = Color.White;
            ambientStrength = 0.5f;
            shininess = 16.0f;
            uvScale = new Vector2(1, 1);
            uvOffset = new Vector2(0, 0);
            receiveShadows = true;
            waves = new Wave[MAX_WAVES];
            numWaves = 3;

            for(int i = 0; i < waves.Length; i++)
            {
                float rx = Gowtu.Random.Range(-1.0f, 1.0f);
                float ry = Gowtu.Random.Range(-1.0f, 1.0f);
                waves[i].direction = new Vector2(rx, ry);
                waves[i].amplitude = 1.0f;
                waves[i].steepness = 0.1f + (i * 0.1f);
                waves[i].waveLength = 0.2f + (i * 0.2f);
                waves[i].speed = 0.25f;
            }

            if(shader != null)
            {
                uModel = GL.GetUniformLocation(shader.Id, "uModel");
                uModelInverted = GL.GetUniformLocation(shader.Id, "uModelInverted");
                uMVP = GL.GetUniformLocation(shader.Id, "uMVP");
                uDiffuseTexture = GL.GetUniformLocation(shader.Id, "uDiffuseTexture");
                uDiffuseColor = GL.GetUniformLocation(shader.Id, "uDiffuseColor");
                uAmbientStrength = GL.GetUniformLocation(shader.Id, "uAmbientStrength");
                uShininess = GL.GetUniformLocation(shader.Id, "uShininess");
                uUVScale = GL.GetUniformLocation(shader.Id, "uUVScale");
                uUVOffset = GL.GetUniformLocation(shader.Id, "uUVOffset");
                uDepthMap = GL.GetUniformLocation(shader.Id, "uDepthMap");
                uReceiveShadows = GL.GetUniformLocation(shader.Id, "uReceiveShadows");

                uWaves = new WaveUniform[MAX_WAVES];

                for (int i = 0; i < uWaves.Length; ++i) 
                {
                    uWaves[i].direction = GL.GetUniformLocation(shader.Id, "uWaves[" + i + "].direction");
                    uWaves[i].steepness = GL.GetUniformLocation(shader.Id, "uWaves[" + i + "].steepness");
                    uWaves[i].waveLength = GL.GetUniformLocation(shader.Id, "uWaves[" + i + "].waveLength");
                    uWaves[i].amplitude = GL.GetUniformLocation(shader.Id, "uWaves[" + i + "].amplitude");
                    uWaves[i].speed = GL.GetUniformLocation(shader.Id, "uWaves[" + i + "].speed");
                }

                uNumWaves = GL.GetUniformLocation(shader.Id, "uNumWaves");
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

            shader.SetMat4(uModel, model);
            shader.SetMat3(uModelInverted, modelInverted);
            shader.SetMat4(uMVP, MVP);
            shader.SetFloat4(uDiffuseColor, diffuseColor);
            shader.SetFloat(uAmbientStrength, ambientStrength);
            shader.SetFloat(uShininess, shininess);
            shader.SetFloat2(uUVScale, uvScale);
            shader.SetFloat2(uUVOffset, uvOffset);
            shader.SetInt(uReceiveShadows, receiveShadows ? 1 : 0);
            shader.SetInt(uNumWaves, numWaves);

            for(int i = 0; i < numWaves; i++)
            {
                shader.SetFloat2(uWaves[i].direction, waves[i].direction);
                shader.SetFloat(uWaves[i].amplitude, waves[i].amplitude);
                shader.SetFloat(uWaves[i].steepness, waves[i].steepness);
                shader.SetFloat(uWaves[i].waveLength, waves[i].waveLength);
                shader.SetFloat(uWaves[i].speed, waves[i].speed);
            }
        }
    }
}