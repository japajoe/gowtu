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
using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace Gowtu
{
    public sealed class Camera : Component
    {
        public static readonly uint UBO_BINDING_INDEX = 1;
        public static readonly string UBO_NAME = "Camera";

        private static Camera m_mainCamera;
        private float m_fieldOfView;
        private float m_near;
        private float m_far;
        private Color m_clearColor;
        private Matrix4 m_projection;
        private Frustum m_frustum;

        private static UniformBufferObject ubo;

        public static Camera mainCamera
        {
            get
            {
                return m_mainCamera;
            }
        }

        public float fieldOfView
        {
            get
            {
                return m_fieldOfView;
            }
            set
            {
                m_fieldOfView = value;
                Initialize();
            }
        }

        public float nearClippingPlane
        {
            get
            {
                return m_near;
            }
            set
            {
                m_near = value;
                Initialize();
            }
        }

        public float farClippingPlane
        {
            get
            {
                return m_far;
            }
            set
            {
                m_far = value;
                Initialize();
            }
        }

        public Color clearColor
        {
            get
            {
                return m_clearColor;
            }
            set
            {
                m_clearColor = value;
            }
        }

        public Frustum frustum
        {
            get
            {
                return m_frustum;
            }
        }

        public Camera() : base()
        {
            m_fieldOfView = 70.0f;
            m_near = 0.1f;
            m_far = 1000.0f;
            m_clearColor = Color.White;
            m_frustum = new Frustum();
        }

        internal override void OnInitializeComponent()
        {
            Initialize();
            Graphics.Resize += OnWindowResize;
            transform.Changed += OnTransformChanged;

            if(m_mainCamera == null)
            {
                m_mainCamera = this;
            }
        }

        internal override void OnDestroyComponent()
        {
            Graphics.Resize -= OnWindowResize;
            transform.Changed -= OnTransformChanged;

            if(m_mainCamera == this)
            {
                m_mainCamera = null;
            }
        }

        public Matrix4 GetProjectionMatrix()
        {
            return m_projection;
        }

        public Matrix4 GetViewMatrix()
        {
            var m = transform.GetModelMatrix();
            m.Invert();
            return m;
        } 

        private void Initialize()
        {
            var viewport = Graphics.GetViewport();

            float fov = (float)((Math.PI / 180.0) * m_fieldOfView);
            float aspect = viewport.width / viewport.height;
            m_projection = Matrix4.CreatePerspectiveFieldOfView(fov, aspect, m_near, m_far);

            m_frustum.Initialize(GetViewMatrix() * GetProjectionMatrix());
        }

        private void OnWindowResize(int x, int y, int width, int height)
        {
            Initialize();            
        }

        private void OnTransformChanged(Transform sender)
        {
            m_frustum.Initialize(GetViewMatrix() * GetProjectionMatrix());
        }

        private static void InitializeUniformBuffer()
        {
            if(ubo != null)
                return;

            ubo = Resources.FindUniformBuffer(UBO_NAME);
        }

        internal static void UpdateUniformBuffer()
        {
            InitializeUniformBuffer();
            
            Camera camera = Camera.mainCamera;

            if(camera == null)
                return;

            ubo.Bind();

            UniformCameraInfo info = new UniformCameraInfo();
            info.view = camera.GetViewMatrix();
            info.projection = camera.GetProjectionMatrix();
            info.viewProjection = info.view * info.projection;
            info.position = new Vector4(camera.transform.position, 1.0f);

            ReadOnlySpan<UniformCameraInfo> s = new ReadOnlySpan<UniformCameraInfo>(info);
            ubo.BufferSubData<UniformCameraInfo>(s, 0);

            ubo.Unbind();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UniformCameraInfo
    {
        public Matrix4 view;
        public Matrix4 projection;
        public Matrix4 viewProjection;
        public Vector4 position;
    }

    public struct Frustum
    {
        private Vector4[] planes;

        public Frustum()
        {
            planes = new Vector4[6];
        }
        
        public void Initialize(Matrix4 viewProjection)
        {
            planes[0] = new Vector4(
                viewProjection[0,3] + viewProjection[0,0],
                viewProjection[1,3] + viewProjection[1,0],
                viewProjection[2,3] + viewProjection[2,0],
                viewProjection[3,3] + viewProjection[3,0]
            );

            // Right plane
            planes[1] = new Vector4(
                viewProjection[0,3] - viewProjection[0,0],
                viewProjection[1,3] - viewProjection[1,0],
                viewProjection[2,3] - viewProjection[2,0],
                viewProjection[3,3] - viewProjection[3,0]
            );
            // Bottom plane
            planes[2] = new Vector4(
                viewProjection[0,3] + viewProjection[0,1],
                viewProjection[1,3] + viewProjection[1,1],
                viewProjection[2,3] + viewProjection[2,1],
                viewProjection[3,3] + viewProjection[3,1]
            );

            // // Top plane
            planes[3] = new Vector4(
                viewProjection[0,3] - viewProjection[0,1],
                viewProjection[1,3] - viewProjection[1,1],
                viewProjection[2,3] - viewProjection[2,1],
                viewProjection[3,3] - viewProjection[3,1]
            );

            // // Near plane
            planes[4] = new Vector4(
                viewProjection[0,2],
                viewProjection[1,2],
                viewProjection[2,2],
                viewProjection[3,2]
            );

            // // Far plane
            planes[5] = new Vector4(
                viewProjection[0,3] - viewProjection[0,2],
                viewProjection[1,3] - viewProjection[1,2],
                viewProjection[2,3] - viewProjection[2,2],
                viewProjection[3,3] - viewProjection[3,2]
            );

            // Normalize the planes
            for (int i = 0; i < 6; ++i) 
            {
                planes[i] = Vector4.Normalize(planes[i]);
            }
        }

        public bool Contains(BoundingBox bounds)
        {
            var min = bounds.Min;
            var max = bounds.Max;

            Vector3[] corners = new Vector3[8];
            corners[0] = new Vector3(min.X, min.Y, min.Z);
            corners[1] = new Vector3(max.X, min.Y, min.Z);
            corners[2] = new Vector3(min.X, max.Y, min.Z);
            corners[3] = new Vector3(max.X, max.Y, min.Z);
            corners[4] = new Vector3(min.X, min.Y, max.Z);
            corners[5] = new Vector3(max.X, min.Y, max.Z);
            corners[6] = new Vector3(min.X, max.Y, max.Z);
            corners[7] = new Vector3(max.X, max.Y, max.Z);

            for (int i = 0; i < 6; i++) 
            {
                Vector4 plane = planes[i];
                int vOut = 0;

                // Check all corners against the current frustum plane
                for (int j = 0; j < 8; j++)
                {
                    if (Vector3.Dot(new Vector3(plane.X, plane.Y, plane.Z), corners[j]) + plane.W < 0.0f)
                    {
                        vOut++;
                    }
                    else
                    {
                        break; // Exit early if any corner is inside this plane
                    }
                }

                if (vOut == 8)
                {
                    return false; // All corners are outside this frustum plane
                }
            }

            return true;
        }
    };
}