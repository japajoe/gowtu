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
using OpenTK.Mathematics;

namespace Gowtu
{
    public enum LightType : int
    {
        Directional = 0,
        Point = 1,
    };

    public sealed class Light : Component
    {
        public static readonly uint UBO_BINDING_INDEX = 0;
        public static readonly string UBO_NAME = "Lights";
        public static readonly uint MAX_LIGHTS = 32;

        private LightType m_type;
        private float m_strength;
        private float m_constant;
        private float m_linear;
        private float m_quadratic;
        private Color m_color;
        private Color m_ambient;
        private Color m_diffuse;
        private Color m_specular;

        private static UniformBufferObject m_ubo;
        private static List<Light> m_lights;
        private static Light m_mainLight;

        public LightType Type
        {
            get
            {
                return m_type;
            }
            set
            {
                m_type = value;
            }
        }

        public float Strength
        {
            get
            {
                return m_strength;
            }
            set
            {
                m_strength = value;
            }
        }

        public Color Color
        {
            get
            {
                return m_color;
            }
            set
            {
                m_color = value;
            }
        }

        public Color Ambient
        {
            get
            {
                return m_ambient;
            }
            set
            {
                m_ambient = value;
            }
        }

        public Color Diffuse
        {
            get
            {
                return m_diffuse;
            }
            set
            {
                m_diffuse = value;
            }
        }

        public Color Specular
        {
            get
            {
                return m_specular;
            }
            set
            {
                m_specular = value;
            }
        }

        public static Light mainLight
        {
            get
            {
                return m_mainLight;
            }
        }

        public Light() : base()
        {
            m_type = LightType.Directional;
            m_color = Color.White;
            m_ambient = Color.White;
            m_diffuse = Color.White;
            m_specular = Color.White;
            m_strength = 1.0f;
            m_constant = 1.0f;
            m_linear = 0.09f;
            m_quadratic = 0.032f;

            InitializeLights();
        }

        internal override void OnInitializeComponent()
        {
            for(int i = 0; i < m_lights.Count; i++)
            {
                if(m_lights[i] == null)
                {
                    m_lights[i] = this;
                    break;
                }
            }

            if(m_mainLight == null)
            {
                m_mainLight = this;
            }

            UpdateUniformBuffer();
        }

        internal override void OnDestroyComponent()
        {
            for(int i = 0; i < m_lights.Count; i++)
            {
                if(m_lights[i] == this)
                {
                    m_lights[i] = null;
                    break;
                }
            }

            if(m_mainLight == this)
            {
                m_mainLight = null;
            }

            UpdateUniformBuffer();
        }

        private static void InitializeUniformBuffer()
        {
            if(m_ubo != null)
                return;

            m_ubo = Resources.FindUniformBuffer(UBO_NAME);
        }

        private static void InitializeLights()
        {
            if(m_lights == null)
            {
                m_lights = new List<Light>();
                
                for(int i = 0; i < MAX_LIGHTS; i++)
                {
                    m_lights.Add(null);
                }
            }
        }

        internal static void UpdateUniformBuffer()
        {
            InitializeUniformBuffer();
            InitializeLights();

            m_ubo.Bind();

            for(int i  = 0; i < m_lights.Count; i++)
            {
                Light light = m_lights[i];
                UniformLightInfo info = new UniformLightInfo();

                if(light != null)
                {
                    info.isActive = light.gameObject.isActive ? 1 : 0;
                    info.type = (int)light.m_type;
                    info.constant = light.m_constant;
                    info.linear = light.m_linear;
                    info.quadratic = light.m_quadratic;
                    info.strength = light.m_strength;
                    info.padding1 = 0;
                    info.padding2 = 0;
                    info.position = new Vector4(light.gameObject.transform.position, 1.0f);
                    info.direction = new Vector4(light.gameObject.transform.forward, 1.0f);
                    info.color = light.m_color;
                    info.ambient = light.m_ambient;
                    info.diffuse = light.m_diffuse;
                    info.specular = light.m_specular;
                }
                else
                {
                    info.isActive = 0;
                }

                ReadOnlySpan<UniformLightInfo> s = new ReadOnlySpan<UniformLightInfo>(ref info);
                m_ubo.BufferSubData<UniformLightInfo>(s, i * Marshal.SizeOf<UniformLightInfo>());
            }

            m_ubo.Unbind();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UniformLightInfo
    {
        public int isActive;       //4
        public int type;           //4
        public float constant;     //4
        public float linear;       //4
        public float quadratic;    //4
        public float strength;     //4
        public float padding1;     //4
        public float padding2;     //4
        public Vector4 position;   //16
        public Vector4 direction;  //16
        public Color color;        //16
        public Color ambient;      //16
        public Color diffuse;      //16
        public Color specular;     //16
    }
}