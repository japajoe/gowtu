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
        public static readonly uint MAX_LIGHTS = 32;

        private LightType type;
        private float strength;
        private float constant;
        private float linear;
        private float quadratic;
        private Color color;
        private Color ambient;
        private Color diffuse;
        private Color specular;

        private static UniformBufferObject ubo;
        private static List<Light> lights;

        public LightType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }

        public float Strength
        {
            get
            {
                return strength;
            }
            set
            {
                strength = value;
            }
        }

        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }

        public Color Ambient
        {
            get
            {
                return ambient;
            }
            set
            {
                ambient = value;
            }
        }

        public Color Diffuse
        {
            get
            {
                return diffuse;
            }
            set
            {
                diffuse = value;
            }
        }

        public Color Specular
        {
            get
            {
                return specular;
            }
            set
            {
                specular = value;
            }
        }

        public Light() : base()
        {
            type = LightType.Directional;
            color = Color.White;
            ambient = Color.White;
            diffuse = Color.White;
            specular = Color.White;
            strength = 1.0f;
            constant = 1.0f;
            linear = 0.09f;
            quadratic = 0.032f;

            InitializeLights();
        }

        internal override void OnInitializeComponent()
        {
            for(int i = 0; i < lights.Count; i++)
            {
                if(lights[i] == null)
                {
                    lights[i] = this;
                    break;
                }
            }

            UpdateUniformBuffer();
        }

        internal override void OnDestroyComponent()
        {
            for(int i = 0; i < lights.Count; i++)
            {
                if(lights[i] == this)
                {
                    lights[i] = null;
                    break;
                }
            }

            UpdateUniformBuffer();
        }

        private static void InitializeUniformBuffer()
        {
            if(ubo != null)
                return;

            ubo = Resources.FindUniformBuffer("Lights");
        }

        private static void InitializeLights()
        {
            if(lights == null)
            {
                lights = new List<Light>();
                
                for(int i = 0; i < MAX_LIGHTS; i++)
                {
                    lights.Add(null);
                }
            }
        }

        internal static void UpdateUniformBuffer()
        {
            InitializeUniformBuffer();
            InitializeLights();

            ubo.Bind();

            for(int i  = 0; i < lights.Count; i++)
            {
                Light light = lights[i];
                UniformLightInfo info = new UniformLightInfo();

                if(light != null)
                {
                    info.isActive = light.gameObject.isActive ? 1 : 0;
                    info.type = (int)light.type;
                    info.constant = light.constant;
                    info.linear = light.linear;
                    info.quadratic = light.quadratic;
                    info.strength = light.strength;
                    info.padding1 = 0;
                    info.padding2 = 0;
                    info.position = new Vector4(light.gameObject.transform.position, 1.0f);
                    info.direction = new Vector4(light.gameObject.transform.forward, 1.0f);
                    info.color = light.color;
                    info.ambient = light.ambient;
                    info.diffuse = light.diffuse;
                    info.specular = light.specular;
                }
                else
                {
                    info.isActive = 0;
                }

                ReadOnlySpan<UniformLightInfo> s = new ReadOnlySpan<UniformLightInfo>(info);
                ubo.BufferSubData<UniformLightInfo>(s, i * Marshal.SizeOf<UniformLightInfo>());
            }

            ubo.Unbind();
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