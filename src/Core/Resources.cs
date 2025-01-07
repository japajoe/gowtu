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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Gowtu
{
    public static class Resources
    {
        private static Dictionary<string,Shader> shaders = new Dictionary<string, Shader>();
        private static Dictionary<string,Texture2D> textures = new Dictionary<string, Texture2D>();
        private static Dictionary<string,Mesh> meshes = new Dictionary<string, Mesh>();
        private static Dictionary<string,UniformBufferObject> uniformBuffers = new Dictionary<string, UniformBufferObject>();
        private static ConcurrentQueue<Resource> resourceQueue = new ConcurrentQueue<Resource>();

        internal static void LoadDefault()
        {
            AddTexture("Default", new Texture2D(2, 2, Color.White));
            
            var diffuseShader = CreateShader("Diffuse", DiffuseShader.vertexSource, DiffuseShader.fragmentSource);
            var skyboxShader = CreateShader("Skybox", SkyboxShader.vertexSource, SkyboxShader.fragmentSource);
            var terrainShader = CreateShader("Terrain", DiffuseShader.vertexSource, TerrainShader.fragmentSource);

            AddMesh("Cube", MeshGenerator.CreateCube(new Vector3(1, 1, 1)));
            AddMesh("Plane", MeshGenerator.CreatePlane(new Vector3(1, 1, 1)));
            AddMesh("Quad", MeshGenerator.CreateQuad(new Vector3(1, 1, 1)));
            AddMesh("Sphere", MeshGenerator.CreateSphere(new Vector3(1, 1, 1)));
            AddMesh("Skybox", MeshGenerator.CreateSkybox(new Vector3(1, 1, 1)));

            var uboLights = CreateUniformBuffer<UniformLightInfo>(Light.UBO_BINDING_INDEX, Light.MAX_LIGHTS, "Lights");
            var uboCamera = CreateUniformBuffer<UniformCameraInfo>(Camera.UBO_BINDING_INDEX, 1, "Camera");
            var uboWorld = CreateUniformBuffer<UniformWorldInfo>(World.UBO_BINDING_INDEX, 1, "World");

            uboLights.BindBlockToShader(diffuseShader, Light.UBO_BINDING_INDEX, "Lights");
            uboLights.BindBlockToShader(terrainShader, Light.UBO_BINDING_INDEX, "Lights");

            uboCamera.BindBlockToShader(diffuseShader, Camera.UBO_BINDING_INDEX, "Camera");
            uboCamera.BindBlockToShader(terrainShader, Camera.UBO_BINDING_INDEX, "Camera");

            uboWorld.BindBlockToShader(diffuseShader, World.UBO_BINDING_INDEX, "World");
            uboWorld.BindBlockToShader(terrainShader, World.UBO_BINDING_INDEX, "World");
            uboWorld.BindBlockToShader(skyboxShader, World.UBO_BINDING_INDEX, "World");
        }

        private static Shader CreateShader(string name, string vertexSource, string fragmentSource)
        {
            return AddShader(name, new Shader(vertexSource, fragmentSource));
        }

        public static Shader AddShader(string name, Shader shader)
        {
            if(shaders.ContainsKey(name))
            {
                Console.WriteLine(string.Format("[SHADER] can't add {0} with ID: {1} because it already exists", name, shader.Id));
                return null;
            }

            if(shader.Id == 0)
            {
                Console.WriteLine(string.Format("[SHADER] can't add {0} because it's not initialized", name));
                return null;
            }

            shaders[name] = shader;

            Console.WriteLine("[SHADER] {0} added with ID: {1}", name, shader.Id);
            
            return shaders[name];
        }

        public static Texture2D AddTexture(string name, Texture2D texture)
        {
            if(textures.ContainsKey(name))
            {
                Console.WriteLine(string.Format("[TEXTURE2D] can't add {0} with ID: {1} because it already exists", name, texture.Id));
                return null;
            }

            if(texture.Id == 0)
            {
                Console.WriteLine(string.Format("[TEXTURE2D] can't add {0} because it's not initialized", name));
                return null;
            }

            textures[name] = texture;

            Console.WriteLine("[TEXTURE2D] {0} added with ID: {1}", name, texture.Id);
            
            return textures[name];
        }

        public static Mesh AddMesh(string name, Mesh mesh)
        {
            if(meshes.ContainsKey(name))
            {
                Console.WriteLine(string.Format("[MESH] can't add {0} with ID: {1} because it already exists", name, mesh.VAO.Id));
                return null;
            }

            if(mesh.VAO.Id == 0)
            {
                Console.WriteLine(string.Format("[MESH] can't add {0} because it's not initialized", name));
                return null;
            }

            meshes[name] = mesh;

            Console.WriteLine("[MESH] {0} added with ID: {1}", name, mesh.VAO.Id);
            
            return meshes[name];
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

            return AddUniformBuffer(name, ubo);
        }

        public static UniformBufferObject AddUniformBuffer(string name, UniformBufferObject ubo)
        {
            if(uniformBuffers.ContainsKey(name))
            {
                Console.WriteLine(string.Format("[UNIFORMBUFFER] can't add {0} with ID: {1} because it already exists", name, ubo.Id));
                return null;
            }

            if(ubo.Id == 0)
            {
                Console.WriteLine(string.Format("[UNIFORMBUFFER] can't add {0} because it's not initialized", name));
                return null;
            }

            uniformBuffers[name] = ubo;

            Console.WriteLine("[UNIFORMBUFFER] {0} added with ID: {1}", name, ubo.Id);
            
            return uniformBuffers[name];
        }

        public static Shader FindShader(string name)
        {
            if(!shaders.ContainsKey(name))
                return null;

            return shaders[name];
        }

        public static Texture2D FindTexture(string name)
        {
            if(!textures.ContainsKey(name))
                return null;

            return textures[name];
        }

        public static Mesh FindMesh(string name)
        {
            if(!meshes.ContainsKey(name))
                return null;

            return meshes[name];
        }

        public static UniformBufferObject FindUniformBuffer(string name)
        {
            if(!uniformBuffers.ContainsKey(name))
                return null;

            return uniformBuffers[name];
        }

        public static void RemoveShader(string name)
        {
            Shader shader = FindShader(name);
            
            if(shader != null)
            {
                Console.WriteLine("[SHADER] {0} removed with ID: {1}", name, shader.Id);
                shader.Delete();
                shaders.Remove(name);
            }
        }

        public static void RemoveTexture(string name)
        {
            Texture2D texture = FindTexture(name);
            
            if(texture != null)
            {
                Console.WriteLine("[TEXTURE2D] {0} removed with ID: {1}", name, texture.Id);
                texture.Delete();
                textures.Remove(name);
            }
        }

        public static void RemoveMesh(string name)
        {
            Mesh mesh = FindMesh(name);
            
            if(mesh != null)
            {
                Console.WriteLine("[MESH] {0} removed with ID: {1}", name, mesh.VAO.Id);
                mesh.Delete();
                meshes.Remove(name);
            }
        }

        public static void RemoveUniformBuffer(string name)
        {
            UniformBufferObject ubo = FindUniformBuffer(name);
            
            if(ubo != null)
            {
                Console.WriteLine("[UNIFORMBUFFER] {0} removed with ID: {1}", name, ubo.Id);
                ubo.Delete();
                uniformBuffers.Remove(name);
            }
        }

        internal static void NewFrame()
        {
            if(resourceQueue.Count > 0)
            {
                //Only dispatch 1 resource per frame in order not to lock the thread
                if(resourceQueue.TryDequeue(out Resource resource))
                {
                    GameBehaviour.OnResourceLoaded(resource);
                }
            }
        }

        public static void LoadAsyncFromFile(List<ResourceInfo> resources)
        {
            Task.Run(async () =>
            {
                for(int i = 0; i < resources.Count; i++)
                {
                    Resource asset = new Resource();
                    asset.info = new ResourceInfo();
                    asset.info.filePath = resources[i].filePath;
                    asset.info.type = resources[i].type;

                    if(!System.IO.File.Exists(resources[i].filePath))
                    {
                        await Task.Delay(1);
                        asset.data = null;
                        asset.info.result = ResourceLoadResult.Error;
                        resourceQueue.Enqueue(asset);
                    }
                    else
                    {
                        asset.data = await System.IO.File.ReadAllBytesAsync(resources[i].filePath);
                        asset.info.result = ResourceLoadResult.Ok;
                        resourceQueue.Enqueue(asset);
                    }
                }
            });            
        }

        public static void LoadAsyncFromAssetPack(string pathToAssetPack, string assetPackKey, List<ResourceInfo> resources)
        {
            if(!System.IO.File.Exists(pathToAssetPack))
            {
                System.Console.WriteLine("The file does not exist: " + pathToAssetPack);
                return;
            }

            Task.Run(async () =>
            {
                using(AssetPack pack = new AssetPack(pathToAssetPack, assetPackKey))
                {
                    if(pack.Loaded)
                    {
                        for(int i = 0; i < resources.Count; i++)
                        {
                            Resource asset = new Resource();
                            asset.info = new ResourceInfo();
                            asset.info.filePath = resources[i].filePath;
                            asset.info.type = resources[i].type;

                            if(!pack.FileExists(resources[i].filePath))
                            {
                                await Task.Delay(1);
                                asset.data = null;
                                asset.info.result = ResourceLoadResult.Error;
                                resourceQueue.Enqueue(asset);
                            }
                            else
                            {
                                asset.data = await pack.GetFileBufferAsync(resources[i].filePath);
                                asset.info.result = ResourceLoadResult.Ok;
                                resourceQueue.Enqueue(asset);
                            }
                        }
                    }
                }                
            });            
        }
    }

    public enum ResourceType
    {
        AudioClip,
        Texture,
        Shader,
        Font,
        Blob
    }

    public enum ResourceLoadResult
    {
        Ok,
        Error
    }

    public class ResourceInfo
    {
        public ResourceType type;
        public ResourceLoadResult result;
        public string filePath;

        public ResourceInfo()
        {
            this.type = ResourceType.Blob;
            this.filePath = string.Empty;
        }

        public ResourceInfo(ResourceType type, string filePath)
        {
            this.type = type;
            this.filePath = filePath;
        }
    }

    public class Resource
    {
        public ResourceInfo info;
        public byte[] data;
    }
}