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
using OpenTK.Graphics.OpenGL;

namespace Gowtu
{
    public delegate void WindowResizeEvent(int x, int y, int width, int height);

    [StructLayout(LayoutKind.Sequential)]
    public struct Viewport
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public Viewport()
        {
            x = 0;
            y = 0;
            width = 0;
            height = 0;
        }

        public Viewport(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }

    public static class Graphics
    {
        public static WindowResizeEvent Resize;

        private static Viewport viewport = new Viewport(0, 0, 512, 512);
        private static List<Renderer> renderers = new List<Renderer>();
        private static PriorityQueue<Renderer, uint> renderQueue = new PriorityQueue<Renderer, uint>();
        private static ImGuiController imgui = null;

        internal static void Initialize()
        {
            imgui = new ImGuiController();
        }

        internal static void Deinitialize()
        {
            imgui.Dispose();
        }

        internal static void NewFrame()
        {
            Camera mainCamera = Camera.mainCamera;
            Color clearColor = mainCamera == null ? Color.White : mainCamera.clearColor;
            GL.ClearColor(clearColor.r, clearColor.g, clearColor.b, clearColor.a);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Light.UpdateUniformBuffer();
            Camera.UpdateUniformBuffer();
            World.UpdateUniformBuffer();

            if(mainCamera == null)
            {
                Console.WriteLine("Can't render because no main camera has been set");
            }
            else
            {
                for(int i = 0; i < renderers.Count; i++)
                {
                    renderQueue.Enqueue(renderers[i], renderers[i].renderQueue);
                }

                if(renderQueue.Count > 0)
                {
                    while(renderQueue.Count > 0)
                    {
                        Renderer renderer = renderQueue.Dequeue();
                        renderer.OnRender();
                    }
                }
            }

            Graphics2D.NewFrame();

            imgui.NewFrame();
            GameBehaviour.OnBehaviourGUI();
            imgui.EndFrame();
        }

        internal static void SetViewport(int x, int y, int width, int height)
        {
            viewport = new Viewport(x, y, width, height);
            GL.Viewport(x, y, width, height);
            Resize?.Invoke(x, y, width, height);
        }

        public static Viewport GetViewport()
        {
            return viewport;
        }

        internal static Renderer GetRendererByIndex(int index)
        {
            if(index < 0 || index >= renderers.Count)
                return null;
            
            return renderers[index];
        }

        internal static void Add(Renderer renderer)
        {
            for(int i = 0; i < renderers.Count; i++)
            {
                if(renderers[i].instanceId == renderer.instanceId)
                    return;
            }

            renderers.Add(renderer);
        }

        internal static void Remove(Renderer renderer)
        {
            int index = 0;
            bool found = false;
            
            for(int i = 0; i < renderers.Count; i++)
            {
                if(renderers[i].instanceId == renderer.instanceId)
                {
                    index = i;
                    found = true;
                    break;
                }
            }

            if(found)
            {
                renderers.RemoveAt(index);
            }
        }
    }
}