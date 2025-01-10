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

namespace Gowtu
{
    public static class GLState
    {
        private static DepthFunction lastDepthFunc = DepthFunction.Less;
        private static bool lastDepthTest = true;
        private static bool lastCullFace = true;
        private static PolygonMode lastPolygonMode = PolygonMode.Fill;
        private static bool lastBlendMode = false;
        private static bool lastDepthMask = true;

        public static void SetDepthFunc(DepthFunction func)
        {
            if (func != lastDepthFunc)
            {
                GL.DepthFunc(func);
                lastDepthFunc = func;
            }
        }

        public static void SetPolygonMode(PolygonMode mode)
        {
            if (mode != lastPolygonMode)
            {
                GL.PolygonMode(TriangleFace.FrontAndBack, mode);
                lastPolygonMode = mode;
            }
        }

        public static void DepthTest(bool enabled)
        {
            if(enabled)
                EnableDepthTest();
            else
                DisableDepthTest();
        }

        public static void CullFace(bool enabled)
        {
            if(enabled)
                EnableCullFace();
            else
                DisableCullFace();
        }

        public static void BlendMode(bool enabled)
        {
            if(enabled)
                EnableBlendMode();
            else
                DisableBlendMode();
        }

        public static void DepthMask(bool enabled)
        {
            if(enabled)
                EnableDepthMask();
            else
                DisableDepthMask();
        }

        private static void EnableDepthTest()
        {
            if (!lastDepthTest)
            {
                GL.Enable(EnableCap.DepthTest);
                lastDepthTest = true;
            }
        }

        private static void DisableDepthTest()
        {
            if (lastDepthTest)
            {
                GL.Disable(EnableCap.DepthTest);
                lastDepthTest = false;
            }
        }

        private static void EnableCullFace()
        {
            if (!lastCullFace)
            {
                GL.Enable(EnableCap.CullFace);
                lastCullFace = true;
            }
        }

        private static void DisableCullFace()
        {
            if (lastCullFace)
            {
                GL.Disable(EnableCap.CullFace);
                lastCullFace = false;
            }
        }

        private static void EnableBlendMode()
        {
            //if(!lastBlendMode)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                lastBlendMode = true;
            }
        }

        private static void DisableBlendMode()
        {
            //if(lastBlendMode)
            {
                GL.Disable(EnableCap.Blend);
                lastBlendMode = true;
            }
        }

        private static void EnableDepthMask()
        {
            if (!lastDepthMask)
            {
                GL.DepthMask(true);
                lastDepthMask = true;
            }
        }

        private static void DisableDepthMask()
        {
            if (lastDepthMask)
            {
                GL.DepthMask(false);
                lastDepthMask = false;
            }
        }
    }
}