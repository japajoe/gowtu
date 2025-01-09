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
using OpenTK.Mathematics;

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
        private static ShadowMap shadowMap = null;
        private static DepthMaterial depthMaterial;

        internal static void Initialize()
        {
            CreateResources();
            
            GameObject camera = new GameObject();
            camera.AddComponent<Camera>();

            GameObject light = new GameObject();
            light.AddComponent<Light>();

            imgui = new ImGuiController();
            shadowMap = new ShadowMap();
            depthMaterial = new DepthMaterial();
        }

        internal static void Deinitialize()
        {
            imgui.Dispose();
        }

        private static void CreateResources()
        {
            Resources.AddTexture("Default", new Texture2D(2, 2, Color.White));
            var depthmap = Resources.AddTexture("Depth", new Texture2DArray(2048, 2048, 5));

            GL.ObjectLabel(ObjectIdentifier.Texture, (uint)depthmap.Id, -1, "DepthTexture");
            
            var diffuseShader = Resources.AddShader("Diffuse", new Shader(DiffuseShader.vertexSource, DiffuseShader.fragmentSource));
            var skyboxShader = Resources.AddShader("Skybox", new Shader(SkyboxShader.vertexSource, SkyboxShader.fragmentSource));
            var terrainShader = Resources.AddShader("Terrain", new Shader(DiffuseShader.vertexSource, TerrainShader.fragmentSource));
            var depthShader = Resources.AddShader("Depth", new Shader(DepthShader.vertexSource, DepthShader.fragmentSource, DepthShader.geometrySource));

            Resources.AddMesh("Cube", MeshGenerator.CreateCube(new Vector3(1, 1, 1)));
            Resources.AddMesh("Plane", MeshGenerator.CreatePlane(new Vector3(1, 1, 1)));
            Resources.AddMesh("Quad", MeshGenerator.CreateQuad(new Vector3(1, 1, 1)));
            Resources.AddMesh("Sphere", MeshGenerator.CreateSphere(new Vector3(1, 1, 1)));
            Resources.AddMesh("Skybox", MeshGenerator.CreateSkybox(new Vector3(1, 1, 1)));

            var uboLights = CreateUniformBuffer<UniformLightInfo>(Light.UBO_BINDING_INDEX, Light.MAX_LIGHTS, Light.UBO_NAME);
            var uboCamera = CreateUniformBuffer<UniformCameraInfo>(Camera.UBO_BINDING_INDEX, 1, Camera.UBO_NAME);
            var uboWorld = CreateUniformBuffer<UniformWorldInfo>(World.UBO_BINDING_INDEX, 1, World.UBO_NAME);
            var uboShadow = CreateUniformBuffer<UniformShadowInfo>(ShadowMap.UBO_BINDING_INDEX, 1, ShadowMap.UBO_NAME);

            uboLights.BindBlockToShader(diffuseShader, Light.UBO_BINDING_INDEX, Light.UBO_NAME);
            uboLights.BindBlockToShader(terrainShader, Light.UBO_BINDING_INDEX, Light.UBO_NAME);

            uboCamera.BindBlockToShader(diffuseShader, Camera.UBO_BINDING_INDEX, Camera.UBO_NAME);
            uboCamera.BindBlockToShader(terrainShader, Camera.UBO_BINDING_INDEX, Camera.UBO_NAME);

            uboWorld.BindBlockToShader(diffuseShader, World.UBO_BINDING_INDEX, World.UBO_NAME);
            uboWorld.BindBlockToShader(terrainShader, World.UBO_BINDING_INDEX, World.UBO_NAME);
            uboWorld.BindBlockToShader(skyboxShader, World.UBO_BINDING_INDEX, World.UBO_NAME);

            uboShadow.BindBlockToShader(diffuseShader, ShadowMap.UBO_BINDING_INDEX, ShadowMap.UBO_NAME);
            uboShadow.BindBlockToShader(terrainShader, ShadowMap.UBO_BINDING_INDEX, ShadowMap.UBO_NAME);
            uboShadow.BindBlockToShader(depthShader, ShadowMap.UBO_BINDING_INDEX, ShadowMap.UBO_NAME);
        }

        private static UniformBufferObject CreateUniformBuffer<T>(uint bindingIndex, uint numItems, string name) where T : unmanaged
        {
            UniformBufferObject ubo = new UniformBufferObject();
            ubo.Generate();
            ubo.Bind();

            T[] data = new T[numItems];
            
            ubo.BufferData<T>(data, BufferUsageARB.DynamicDraw);
            ubo.BindBufferBase(bindingIndex);
            ubo.Unbind();

            ubo.ObjectLabel(name);

            return Resources.AddUniformBuffer(name, ubo);
        }

        internal static void NewFrame()
        {
            UpdateUniformBuffers();

            if(Camera.mainCamera == null)
            {
                Console.WriteLine("Can't render because no main camera has been set");
            }
            else
            {
                RenderShadowPass();
                Render3DPass();
            }
            
            Render2DPass();
        }

        private static void Clear()
        {
            Color clearColor = Camera.mainCamera == null ? Color.White : Camera.mainCamera.clearColor;
            GL.ClearColor(clearColor.r, clearColor.g, clearColor.b, clearColor.a);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        private static void UpdateUniformBuffers()
        {
            Light.UpdateUniformBuffer();
            Camera.UpdateUniformBuffer();
            World.UpdateUniformBuffer();
            shadowMap.UpdateUniformBuffer();
        }

        private static void RenderShadowPass()
        {
            for(int i = 0; i < renderers.Count; i++)
            {
                renderQueue.Enqueue(renderers[i], renderers[i].renderQueue);
            }

            if(renderQueue.Count > 0)
            {
                shadowMap.Bind();

                while(renderQueue.Count > 0)
                {
                    Renderer renderer = renderQueue.Dequeue();
                    if(renderer.castShadows)
                        renderer.OnRender(depthMaterial);
                }

                shadowMap.Unbind();
            }
        }

        private static void Render3DPass()
        {
            Clear();

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

        private static void Render2DPass()
        {
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