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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Gowtu
{
    public sealed class ShadowMap
    {
        public static readonly uint UBO_BINDING_INDEX = 3;
        public static readonly string UBO_NAME = "Shadow";

        private int m_lightFBO;
        private Shader m_shader;
        private Texture2DArray m_depthMap;
        private UniformBufferObject m_ubo;
        private UniformShadowInfo shadowData;
        private List<float> shadowCascadeLevels;
        private static bool m_enabled = true;

        public static bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;
            }
        }

        public ShadowMap()
        {
            m_depthMap = Resources.FindTexture<Texture2DArray>("Depth");
            m_shader = Resources.FindShader("Depth");
            float farPlane = 1000.0f;

            shadowCascadeLevels = new List<float>()
            { 
                farPlane / 50.0f, 
                farPlane / 25.0f, 
                farPlane / 10.0f, 
                farPlane / 2.0f 
            };

            shadowData = new UniformShadowInfo();
            shadowData.cascadeCount = shadowCascadeLevels.Count;
            shadowData.farPlane = farPlane;
            shadowData.shadowBias = 0.005f;
            shadowData.enabled = m_enabled ? 1 : 0;

            m_lightFBO = 0;
            m_ubo = Resources.FindUniformBuffer(UBO_NAME);

            GL.GenFramebuffers(1, ref m_lightFBO);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, m_lightFBO);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, m_depthMap.Id, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);

            FramebufferStatus status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

            if (status != FramebufferStatus.FramebufferComplete)
            {
                System.Console.WriteLine("CascadedShadowMap framebuffer is not complete");
                throw new System.Exception("CascadedShadowMap framebuffer is not complete");
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Bind()
        {
            Camera camera = Camera.mainCamera;
            
            if(camera == null)
                return;

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, m_lightFBO);
            GL.Viewport(0, 0, (int)m_depthMap.Width, (int)m_depthMap.Height);
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.CullFace(TriangleFace.Front);  // peter panning
        }

        public void Unbind()
        {
            var viewportRect = Graphics.GetViewport();
            GL.CullFace(TriangleFace.Back);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, (int)viewportRect.width, (int)viewportRect.height);
        }

        public void UpdateUniformBuffer()
        {
            Camera camera = Camera.mainCamera;
            
            if(camera == null)
                return;

            shadowData = new UniformShadowInfo();
            shadowData.farPlane = camera.farClippingPlane;
            shadowData.shadowBias = 0.005f;
            shadowData.cascadeCount = shadowCascadeLevels.Count;
            shadowData.enabled = m_enabled ? 1 : 0;

            var lightMatrices = GetLightSpaceMatrices();

            unsafe
            {
                for(int i = 0; i < lightMatrices.Count; i++)
                {
                    int index = i * Marshal.SizeOf<Matrix4>();
                    var matrix = lightMatrices[i];
                    float *pSrc = &matrix.Row0.X;
                    fixed(byte *pDst = &shadowData.lightSpaceMatrices[index])
                    {
                        Unsafe.CopyBlock(pDst, pSrc, (uint)Marshal.SizeOf<Matrix4>());
                    }
                }
            }

            // OpenGL adds 12 bytes padding for each float in an array
            // The alignment will therefor be on a 16 byte boundary
            unsafe
            {
                for(int i = 0; i < shadowCascadeLevels.Count; i++)
                {
                    int index = i * Marshal.SizeOf<Vector4>();

                    fixed(byte *pData = &shadowData.cascadePlaneDistances[index])
                    {
                        float *pFloat = (float*)pData;
                        *pFloat = shadowCascadeLevels[i];
                    }
                    
                }
            }

            m_ubo.Bind();
            ReadOnlySpan<UniformShadowInfo> s = new ReadOnlySpan<UniformShadowInfo>(shadowData);
            m_ubo.BufferSubData<UniformShadowInfo>(s, 0);
            m_ubo.Unbind();
        }

        private Matrix4 GetLightSpaceMatrix(float nearPlane, float farPlane)
        {
            Viewport viewportRect = Graphics.GetViewport();
            Camera camera = Camera.mainCamera;

            var proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(camera.fieldOfView), viewportRect.width / viewportRect.height, nearPlane, farPlane);
            
            var corners = GetFrustumCornersWorldSpace(proj, camera.GetViewMatrix());

            Vector3 center = new Vector3(0, 0, 0);

            foreach (var v in corners)
            {
                center += v.Xyz;
            }
            
            center /= corners.Count;

            var light = Light.mainLight;

            Vector3 lightDir = light.transform.forward;

            var lightView = Matrix4.LookAt(center + lightDir, center, new Vector3(0.0f, 1.0f, 0.0f));

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;
            
            foreach (var v in corners)
            {
                var trf = v * lightView;
                minX = System.Math.Min(minX, trf.X);
                maxX = System.Math.Max(maxX, trf.X);
                minY = System.Math.Min(minY, trf.Y);
                maxY = System.Math.Max(maxY, trf.Y);
                minZ = System.Math.Min(minZ, trf.Z);
                maxZ = System.Math.Max(maxZ, trf.Z);
            }

            // Tune this parameter according to the scene
            float zMult = 1.0f;

            if (minZ < 0)
                minZ *= zMult;
            else
                minZ /= zMult;
            if (maxZ < 0)
                maxZ /= zMult;
            else
                maxZ *= zMult;

            Matrix4 lightProjection = Matrix4.CreateOrthographicOffCenter(minX, maxX, minY, maxY, minZ, maxZ);
            return lightView * lightProjection;
        }

        private List<Matrix4> GetLightSpaceMatrices()
        {
            Camera camera = Camera.mainCamera;

            if(camera == null)
                return null;

            float cameraNearPlane = camera.nearClippingPlane;
            float cameraFarPlane = camera.farClippingPlane;

            List<Matrix4> ret = new List<Matrix4>();

            for (int i = 0; i < shadowCascadeLevels.Count + 1; ++i)
            {
                if (i == 0)
                {
                    ret.Add(GetLightSpaceMatrix(cameraNearPlane, shadowCascadeLevels[i]));
                }
                else if (i < shadowCascadeLevels.Count)
                {
                    ret.Add(GetLightSpaceMatrix(shadowCascadeLevels[i - 1], shadowCascadeLevels[i]));
                }
                else
                {
                    ret.Add(GetLightSpaceMatrix(shadowCascadeLevels[i - 1], cameraFarPlane));
                }
            }
            return ret;
        }

        private List<Vector4> GetFrustumCornersWorldSpace(Matrix4 projview)
        {
            var inv = Matrix4.Invert(projview);

            List<Vector4> frustumCorners = new List<Vector4>();

            for (int x = 0; x < 2; ++x)
            {
                for (int y = 0; y < 2; ++y)
                {
                    for (int z = 0; z < 2; ++z)
                    {
                        Vector4 pt = new Vector4(2.0f * x - 1.0f, 2.0f * y - 1.0f, 2.0f * z - 1.0f, 1.0f) * inv;
                        frustumCorners.Add(pt / pt.W);
                    }
                }
            }

            return frustumCorners;
        }

        private List<Vector4> GetFrustumCornersWorldSpace(Matrix4 proj, Matrix4 view)
        {
            return GetFrustumCornersWorldSpace(view * proj);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct UniformShadowInfo
    {
        public int cascadeCount;
        public float shadowBias;
        public float farPlane;
        public int enabled;
        public fixed byte lightSpaceMatrices[16*16*4]; // 16 matrices of 4x4 (16 floats each)
        public fixed byte cascadePlaneDistances[16*16]; //16 floats with 12 bytes padding each
    }
}

