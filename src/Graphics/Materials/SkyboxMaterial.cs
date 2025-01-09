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
        private int uRayleighCoefficient;
        private int uMieCoefficient;
        private int uScatteringDirection;
        private int uCloudSpeed;
        private int uCirrus;
        private int uCumulus;
        private int uSunPosition;

        private float cloudSpeed;
        private float cirrus;
        private float cumulus;
        private float rayleighCoefficient;
        private float mieCoefficient;
        private float scatteringDirection;
        private Vector3 sunPosition;

        public float CloudSpeed
        {
            get
            {
                return cloudSpeed;
            }
            set
            {
                cloudSpeed = value;
            }
        }

        public float Cirrus
        {
            get
            {
                return cirrus;
            }
            set
            {
                cirrus = value;
            }
        }

        public float Cumulus
        {
            get
            {
                return cumulus;
            }
            set
            {
                cumulus = value;
            }
        }

        public float RayleighCoefficient
        {
            get
            {
                return rayleighCoefficient;
            }
            set
            {
                rayleighCoefficient = value;
            }
        }

        public float MieCoefficient
        {
            get
            {
                return mieCoefficient;
            }
            set
            {
                mieCoefficient = value;
            }
        }

        public float ScatteringDirection
        {
            get
            {
                return scatteringDirection;
            }
            set
            {
                scatteringDirection = value;
            }
        }

        public Vector3 SunPosition
        {
            get
            {
                return sunPosition;
            }
            set
            {
                sunPosition = value;
            }
        }
        
        public SkyboxMaterial() : base()
        {
            shader = Resources.FindShader(Constants.GetString(ConstantString.ShaderSkybox));
            cloudSpeed = 0.1f;
            cirrus = 0.677f;
            cumulus = 0.403f;
            rayleighCoefficient = 0.0075f;
            mieCoefficient = 0.0153f;
            scatteringDirection = 0.998f; // Mie scattering direction. Should be ALMOST 1.0f
            sunPosition = new Vector3(-0.007f, 0.954f, -1.563f);

            if(shader != null)
            {
                uModel = GL.GetUniformLocation(shader.Id, "uModel");
                uView = GL.GetUniformLocation(shader.Id, "uView");
                uProjection = GL.GetUniformLocation(shader.Id, "uProjection");
                uRayleighCoefficient = GL.GetUniformLocation(shader.Id, "uRayleighCoefficient");
                uMieCoefficient = GL.GetUniformLocation(shader.Id, "uMieCoefficient");
                uScatteringDirection = GL.GetUniformLocation(shader.Id, "uScatteringDirection");
                uCloudSpeed = GL.GetUniformLocation(shader.Id, "uCloudSpeed");
                uCirrus = GL.GetUniformLocation(shader.Id, "uCirrus");
                uCumulus = GL.GetUniformLocation(shader.Id, "uCumulus");
                uSunPosition = GL.GetUniformLocation(shader.Id, "uSunPosition");
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

            shader.SetFloat(uCloudSpeed, cloudSpeed);
            shader.SetFloat(uCirrus, cirrus);
            shader.SetFloat(uCumulus, cumulus);
            shader.SetFloat(uRayleighCoefficient, rayleighCoefficient);
            shader.SetFloat(uMieCoefficient, mieCoefficient);
            shader.SetFloat(uScatteringDirection, scatteringDirection);
            shader.SetFloat3(uSunPosition, sunPosition);
        }
    }
}