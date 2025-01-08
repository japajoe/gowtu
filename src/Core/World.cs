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

namespace Gowtu
{
    public static class World
    {
        public static readonly uint UBO_BINDING_INDEX = 2;
        public static readonly string UBO_NAME = "World";

        private static Color fogColor = new Color(247, 169, 90, 255);
        private static float fogDensity = 0.0005f;
        private static float fogGradient = 1.5f;
        private static bool fogEnabled = true;
        private static UniformBufferObject ubo = null;

        public static Color FogColor
        {
            get
            {
                return fogColor;
            }
            set
            {
                fogColor = value;
            }
        }

        public static float FogDensity
        {
            get
            {
                return fogDensity;
            }
            set
            {
                fogDensity = value;
            }
        }

        public static float FogGradient
        {
            get
            {
                return fogGradient;
            }
            set
            {
                fogGradient = value;
            }
        }

        public static bool FogEnabled
        {
            get
            {
                return fogEnabled;
            }
            set
            {
                fogEnabled = value;
            }
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

            ubo.Bind();

            UniformWorldInfo info = new UniformWorldInfo(); 
            info.fogColor = fogColor;
            info.fogDensity = fogDensity;
            info.fogGradient = fogGradient;
            info.fogEnabled = fogEnabled ? 1 : 0;
            info.time = Time.Elapsed;

            ReadOnlySpan<UniformWorldInfo> s = new ReadOnlySpan<UniformWorldInfo>(info);
            ubo.BufferSubData<UniformWorldInfo>(s, 0);

            ubo.Unbind();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UniformWorldInfo
    {
        public Color fogColor;     //16 don't use vec3 because the alignment causes issues
        public float fogDensity;   //4
        public float fogGradient;  //4
        public int fogEnabled;     //4
        public float time;         //4
        public float padding1;     //4
        public float padding2;     //4
        public float padding3;     //4
        public float padding4;     //4
    }
}