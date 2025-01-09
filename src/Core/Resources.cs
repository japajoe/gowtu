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
using System.Threading.Tasks;

namespace Gowtu
{
    public static class Resources
    {
        private static Dictionary<string,Shader> shaders = new Dictionary<string, Shader>();
        private static Dictionary<string,Texture> textures = new Dictionary<string, Texture>();
        private static Dictionary<string,Mesh> meshes = new Dictionary<string, Mesh>();
        private static Dictionary<string,Font> fonts = new Dictionary<string, Font>();
        private static Dictionary<string,AudioClip> audioClips = new Dictionary<string, AudioClip>();
        private static Dictionary<string,UniformBufferObject> uniformBuffers = new Dictionary<string, UniformBufferObject>();
        private static ConcurrentQueue<Resource> resourceQueue = new ConcurrentQueue<Resource>();
        private static ConcurrentQueue<ResourceBatch> resourceBatchQueue = new ConcurrentQueue<ResourceBatch>();

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

        public static Texture AddTexture(string name, Texture texture)
        {
            if(textures.ContainsKey(name))
            {
                Console.WriteLine(string.Format("[TEXTURE] can't add {0} with ID: {1} because it already exists", name, texture.Id));
                return null;
            }

            if(texture.Id == 0)
            {
                Console.WriteLine(string.Format("[TEXTURE] can't add {0} because it's not initialized", name));
                return null;
            }

            textures[name] = texture;

            Console.WriteLine("[TEXTURE] {0} added with ID: {1}", name, texture.Id);
            
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

        public static Font AddFont(string name, Font font)
        {
            if(fonts.ContainsKey(name))
            {
                Console.WriteLine(string.Format("[FONT] can't add {0} with ID: {1} because it already exists", name, font.TextureId));
                return null;
            }

            if(font.TextureId == 0)
            {
                Console.WriteLine(string.Format("[FONT] can't add {0} because it's not initialized", name));
                return null;
            }

            fonts[name] = font;

            Console.WriteLine("[FONT] {0} added with ID: {1}", name, font.TextureId);
            
            return fonts[name];
        }

        public static AudioClip AddAudioClip(string name, AudioClip audioClip)
        {
            if(audioClips.ContainsKey(name))
            {
                Console.WriteLine(string.Format("[AUDIOCLIP] can't add {0} with ID: {1} because it already exists", name, audioClip.GetHashCode()));
                return null;
            }

            audioClips[name] = audioClip;

            Console.WriteLine("[AUDIOCLIP] {0} added with ID: {1}", name, audioClip.GetHashCode());
            
            return audioClips[name];
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

        public static T FindTexture<T>(string name) where T : Texture
        {
            if(!textures.ContainsKey(name))
                return null;

            return textures[name] as T;
        }

        public static Mesh FindMesh(string name)
        {
            if(!meshes.ContainsKey(name))
                return null;

            return meshes[name];
        }

        public static Font FindFont(string name)
        {
            if(!fonts.ContainsKey(name))
                return null;

            return fonts[name];
        }

        public static AudioClip FindAudioClip(string name)
        {
            if(!audioClips.ContainsKey(name))
                return null;

            return audioClips[name];
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
            Texture texture = FindTexture<Texture>(name);
            
            if(texture != null)
            {
                Console.WriteLine("[TEXTURE] {0} removed with ID: {1}", name, texture.Id);
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

        public static void RemoveFont(string name)
        {
            Font font = FindFont(name);
            
            if(font != null)
            {
                Console.WriteLine("[FONT] {0} removed with ID: {1}", name, font.TextureId);
                font.Delete();
                fonts.Remove(name);
            }
        }

        public static void RemoveAudioClip(string name)
        {
            AudioClip audioClip = FindAudioClip(name);
            
            if(audioClip != null)
            {
                Console.WriteLine("[AUDIOCLIP] {0} removed with ID: {1}", name, audioClip.GetHashCode());
                audioClip.Dispose();
                audioClips.Remove(name);
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
                    GameBehaviour.OnBehaviourResourceLoaded(resource);
                }
            }

            if(resourceBatchQueue.Count > 0)
            {
                //Only dispatch 1 resource batch per frame in order not to lock the thread
                if(resourceBatchQueue.TryDequeue(out ResourceBatch batch))
                {
                    GameBehaviour.OnBehaviourResourceBatchLoaded(batch);
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

        public static void LoadAsyncBatchFromAssetPack(string pathToAssetPack, string assetPackKey, ResourceType type, List<string> resources)
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
                        ResourceBatch batch = new ResourceBatch(type);

                        for(int i = 0; i < resources.Count; i++)
                        {
                            Resource asset = new Resource();
                            asset.info = new ResourceInfo();
                            asset.info.filePath = resources[i];
                            asset.info.type = type;

                            if(!pack.FileExists(resources[i]))
                            {
                                await Task.Delay(1);
                                asset.data = null;
                                asset.info.result = ResourceLoadResult.Error;
                                batch.resources.Add(asset);
                            }
                            else
                            {
                                asset.data = await pack.GetFileBufferAsync(resources[i]);
                                asset.info.result = ResourceLoadResult.Ok;
                                batch.resources.Add(asset);
                            }
                        }

                        resourceBatchQueue.Enqueue(batch);
                    }
                }                
            });            
        }
    }

    public enum ResourceType
    {
        AudioClip,
        Texture2D,
        Texture2DArray,
        TextureCubeMap,
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

    public class ResourceBatch
    {
        public ResourceType type;
        public List<Resource> resources;

        public ResourceBatch(ResourceType type)
        {
            this.type = type;
            this.resources = new List<Resource>();
        }
    }
}