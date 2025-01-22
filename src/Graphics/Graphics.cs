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

        private static FrameBufferObject fbo;
        private static FullScreenQuad screenQuad;
        private static Viewport viewport = new Viewport(0, 0, 512, 512);
        private static List<Renderer> renderers = new List<Renderer>();
        private static PriorityQueue<Renderer, uint> renderQueue = new PriorityQueue<Renderer, uint>();
        private static ImGuiController imgui = null;
        private static Shadow shadowMap = null;
        private static DepthMaterial depthMaterial;
        private static bool suspend3DPass = false;

        public static bool Suspend3DPass
        {
            get { return suspend3DPass; }
            set { suspend3DPass = value; }
        }

        internal static void Initialize()
        {
            fbo = new FrameBufferObject(512, 512);
            screenQuad = new FullScreenQuad();

            fbo.Generate();
            screenQuad.Generate();

            CreateResources();
            
            GameObject camera = new GameObject();
            camera.AddComponent<Camera>();

            GameObject light = new GameObject();
            light.AddComponent<Light>();

            imgui = new ImGuiController();
            shadowMap = new Shadow();
            depthMaterial = new DepthMaterial();
        }

        internal static void Deinitialize()
        {
            imgui.Dispose();
        }

        private static void CreateResources()
        {
            Resources.AddTexture(Constants.GetString(ConstantString.TextureDefault), new Texture2D(2, 2, Color.White));
            Resources.AddTexture(Constants.GetString(ConstantString.TextureDefaultCubeMap), new TextureCubeMap(2, 2, Color.White));
            Resources.AddTexture(Constants.GetString(ConstantString.TextureDepth), new Texture2DArray(2048, 2048, 5));
            
            var diffuseShader = Resources.AddShader(Constants.GetString(ConstantString.ShaderDiffuse), DiffuseShader.Create());
            var proceduralSkyboxShader = Resources.AddShader(Constants.GetString(ConstantString.ShaderProceduralSkybox), ProceduralSkyboxShader.Create());
            var skyboxShader = Resources.AddShader(Constants.GetString(ConstantString.ShaderSkybox), SkyboxShader.Create());
            var terrainShader = Resources.AddShader(Constants.GetString(ConstantString.ShaderTerrain), TerrainShader.Create());
            var depthShader = Resources.AddShader(Constants.GetString(ConstantString.ShaderDepth), DepthShader.Create());
            var diffuseInstancedShader = Resources.AddShader(Constants.GetString(ConstantString.ShaderDiffuseInstanced), DiffuseInstancedShader.Create());
            var particleShader = Resources.AddShader(Constants.GetString(ConstantString.ShaderParticle), ParticleShader.Create());
            var waterShader = Resources.AddShader(Constants.GetString(ConstantString.ShaderWater), WaterShader.Create());

            Resources.AddMesh(Constants.GetString(ConstantString.MeshCapsule), MeshGenerator.CreateCapsule(Vector3.One));
            Resources.AddMesh(Constants.GetString(ConstantString.MeshCube), MeshGenerator.CreateCube(Vector3.One));
            Resources.AddMesh(Constants.GetString(ConstantString.MeshPlane), MeshGenerator.CreatePlane(Vector3.One));
            Resources.AddMesh(Constants.GetString(ConstantString.MeshQuad), MeshGenerator.CreateQuad(Vector3.One));
            Resources.AddMesh(Constants.GetString(ConstantString.MeshSphere), MeshGenerator.CreateSphere(Vector3.One));
            Resources.AddMesh(Constants.GetString(ConstantString.MeshSkybox), MeshGenerator.CreateSkybox(Vector3.One));

            var uboLights = CreateUniformBuffer<UniformLightInfo>(Light.UBO_BINDING_INDEX, Light.MAX_LIGHTS, Light.UBO_NAME);
            var uboCamera = CreateUniformBuffer<UniformCameraInfo>(Camera.UBO_BINDING_INDEX, 1, Camera.UBO_NAME);
            var uboWorld = CreateUniformBuffer<UniformWorldInfo>(World.UBO_BINDING_INDEX, 1, World.UBO_NAME);
            var uboShadow = CreateUniformBuffer<UniformShadowInfo>(Shadow.UBO_BINDING_INDEX, 1, Shadow.UBO_NAME);

            BindShaderToUniformBuffers(diffuseShader);
            BindShaderToUniformBuffers(proceduralSkyboxShader);
            BindShaderToUniformBuffers(skyboxShader);
            BindShaderToUniformBuffers(terrainShader);
            BindShaderToUniformBuffers(depthShader);
            BindShaderToUniformBuffers(diffuseInstancedShader);
            BindShaderToUniformBuffers(particleShader);
            BindShaderToUniformBuffers(waterShader);

            Font font = new Font();
            if(font.LoadFromMemory(EmbeddedFont.data, EmbeddedFont.data.Length, 32, FontRenderMethod.SDF))
            {
                if(font.GenerateTexture())
                {
                    Resources.AddFont(Constants.GetString(ConstantString.FontDefault), font);
                }
            }
        }

        private static void BindShaderToUniformBuffers(Shader shader)
        {
            if(shader == null)
            {
                Console.WriteLine("Can't bind shader to uniform buffers because shader is null");
                return;
            }

            var uboLights = Resources.FindUniformBuffer(Light.UBO_NAME);
            var uboCamera = Resources.FindUniformBuffer(Camera.UBO_NAME);
            var uboWorld = Resources.FindUniformBuffer(Camera.UBO_NAME);
            var uboShadow = Resources.FindUniformBuffer(Shadow.UBO_NAME);

            uboLights.BindBlockToShader(shader, Light.UBO_BINDING_INDEX, Light.UBO_NAME);
            uboCamera.BindBlockToShader(shader, Camera.UBO_BINDING_INDEX, Camera.UBO_NAME);
            uboWorld.BindBlockToShader(shader, World.UBO_BINDING_INDEX, World.UBO_NAME);
            uboShadow.BindBlockToShader(shader, Shadow.UBO_BINDING_INDEX, Shadow.UBO_NAME);
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
            //fbo.Bind();

            
            Clear();

            UpdateUniformBuffers();

            if(Camera.mainCamera == null)
            {
                Console.WriteLine("Can't render because no main camera has been set");
            }
            else
            {
                RenderShadowPass();
                Render3DPass();
                GameBehaviour.OnBehaviourRender();
            }

            //fbo.Unbind();
            
            //screenQuad.Render(fbo);
            
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
            if(suspend3DPass)
                return;

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
                    {
                        if(renderer is BatchRenderer)
                            depthMaterial.HasInstanceData = true;
                        else
                            depthMaterial.HasInstanceData = false;
                        renderer.OnRender(depthMaterial);
                    }
                }

                shadowMap.Unbind();
            }
        }

        private static void Render3DPass()
        {
            if(suspend3DPass)
                return;

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

            LineRenderer.OnRender();
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
            fbo.Resize(width, height);
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