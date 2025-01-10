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
        public static void SetDepthFunc(DepthFunction func)
        {
            GL.DepthFunc(func);
        }

        public static void SetPolygonMode(PolygonMode mode)
        {
            GL.PolygonMode(TriangleFace.FrontAndBack, mode);
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
            GL.Enable(EnableCap.DepthTest);
        }

        private static void DisableDepthTest()
        {
            GL.Disable(EnableCap.DepthTest);
        }

        private static void EnableCullFace()
        {
            GL.Enable(EnableCap.CullFace);
        }

        private static void DisableCullFace()
        {
            GL.Disable(EnableCap.CullFace);
        }

        private static void EnableBlendMode()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        private static void DisableBlendMode()
        {
            GL.Disable(EnableCap.Blend);
        }

        private static void EnableDepthMask()
        {
            GL.DepthMask(true);
        }

        private static void DisableDepthMask()
        {
            GL.DepthMask(false);
        }
    }
}