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

namespace Gowtu
{
    public enum PrimitiveType
    {
        Capsule,
        Cube,
        Plane,
        ParticleSystem,
        ProceduralSkybox,
        Skybox,
        Quad,
        Sphere,
        Terrain
    }

    [Flags]
    public enum Layer
    {
        Default = 1 << 0,
        IgnoreCulling = 1 << 1,
        IgnoreRaycast = 1 << 2
    }

    public sealed class GameObject : Object
    {
        private Transform m_transform;
        private List<Component> m_components;
        private bool m_isActive;
        private int m_layer;
        private static List<GameObject> m_objects = new List<GameObject>();
        private static List<GameObject> m_destroyQueue = new List<GameObject>();

        public Transform transform
        {
            get
            {
                return m_transform;
            }
        }

        public bool isActive
        {
            get
            {
                return m_isActive;
            }
            set
            {
                SetActive(value);
            }
        }

        public int layer
        {
            get
            {
                return m_layer;
            }
            set
            {
                m_layer = value;
            }
        }
        
        public GameObject() : base()
        {
            m_transform = new Transform();
            m_transform.SetGameObject(this);
            m_components = new List<Component>();
            m_isActive = true;
            m_layer = 0;
            m_objects.Add(this);
        }

        public GameObject(string name) : base()
        {
            this.name = name;
            m_transform = new Transform();
            m_transform.SetGameObject(this);
            m_components = new List<Component>();
            m_isActive = true;
            m_layer = 0;
            m_objects.Add(this);
        }

        ~GameObject()
        {
            for(int i = 0; i < m_components.Count; i++)
            {
                m_components[i].OnDestroyComponent();
            }
            
            m_components.Clear();
        }

        public T AddComponent<T>() where T : Component, new()
        {
            T instance = new T();
            instance.SetGameObject(this);
            instance.SetTransform(m_transform);
            m_components.Add(instance);
            instance.OnInitializeComponent();
            return instance;
        }

        public T GetComponent<T>() where T : Component
        {
            for(int i = 0; i < m_components.Count; i++)
            {
                if(m_components[i].GetType() == typeof(T))
                {
                    return m_components[i] as T;
                }
            }
            return null;
        }

        public T GetComponentOfSubType<T>() where T : Component
        {
            for (int i = 0; i < m_components.Count; i++)
            {
                if (m_components[i].GetType().IsSubclassOf(typeof(T)))
                {
                    return m_components[i] as T;
                }
            }

            return null;
        }

        public List<T> GetComponentsOfSubType<T>() where T : Component
        {
            List<T> c = new List<T>();

            for (int i = 0; i < m_components.Count; i++)
            {
                if (m_components[i].GetType().IsSubclassOf(typeof(T)))
                {
                    c.Add(m_components[i] as T);
                }
            }

            return c;
        }

        public List<Component> GetComponents()
        {
            return m_components;
        }

        public void SetLayer(Layer layer, bool recursive)
        {
            if(recursive)
                SetLayerRecursive(transform, layer);
            else
                this.m_layer = (int)layer;
        }

        private void SetLayerRecursive(Transform root, Layer layer)
        {
            var queue = new Queue<Transform>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                Transform current = queue.Dequeue();

                m_transform.gameObject.m_layer = (int)layer;

                // Enqueue all the children for further processing
                foreach (Transform child in current.children)
                {
                    queue.Enqueue(child);
                }
            }
        }

        private void SetActive(bool isActive)
        {
            SetActiveRecursive(m_transform, isActive);
        }

        private void SetActiveRecursive(Transform root, bool active)
        {
            var queue = new Queue<Transform>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                Transform current = queue.Dequeue();

                m_transform.gameObject.m_isActive = active;

                var components = current.gameObject.m_components;

                for(int i = 0; i < components.Count; i++)
                {
                    if(components[i] is GameBehaviour)
                    {
                        if(active)
                            GameBehaviour.OnBehaviourEnable(components[i].instanceId);
                        else
                            GameBehaviour.OnBehaviourDisable(components[i].instanceId);
                    }
                    else
                    {
                        if(active)
                            components[i].OnActivateComponent();
                        else
                            components[i].OnDeactivateComponent();
                    }
                }

                // Enqueue all the children for further processing
                foreach (Transform child in current.children)
                {
                    queue.Enqueue(child);
                }
            }
        }

        public static GameObject CreatePrimitive(PrimitiveType type)
        {
            GameObject g = new GameObject();
            g.SetLayer(Layer.Default, true);

            switch(type)
            {
                case PrimitiveType.Capsule:
                {
                    var mesh = Resources.FindMesh(Constants.GetString(ConstantString.MeshCapsule));
                    var material = new DiffuseMaterial();
                    var renderer = g.AddComponent<MeshRenderer>();
                    renderer.Add(mesh, material);
                    var settings = renderer.GetSettings(0);
                    settings.cullFace = true;
                    settings.depthTest = true;
                    break;
                }
                case PrimitiveType.Cube:
                {
                    var mesh = Resources.FindMesh(Constants.GetString(ConstantString.MeshCube));
                    var material = new DiffuseMaterial();
                    var renderer = g.AddComponent<MeshRenderer>();
                    renderer.Add(mesh, material);
                    var settings = renderer.GetSettings(0);
                    settings.cullFace = true;
                    settings.depthTest = true;
                    break;
                }
                case PrimitiveType.Plane:
                {
                    var mesh = Resources.FindMesh(Constants.GetString(ConstantString.MeshPlane));
                    var material = new DiffuseMaterial();
                    var renderer = g.AddComponent<MeshRenderer>();
                    renderer.Add(mesh, material);
                    var settings = renderer.GetSettings(0);
                    settings.cullFace = true;
                    settings.depthTest = true;
                    break;
                }
                case PrimitiveType.Quad:
                {
                    var mesh = Resources.FindMesh(Constants.GetString(ConstantString.MeshQuad));
                    var material = new DiffuseMaterial();
                    var renderer = g.AddComponent<MeshRenderer>();
                    renderer.Add(mesh, material);
                    var settings = renderer.GetSettings(0);
                    settings.cullFace = true;
                    settings.depthTest = true;
                    break;
                }
                case PrimitiveType.Sphere:
                {
                    var mesh = Resources.FindMesh(Constants.GetString(ConstantString.MeshSphere));
                    var material = new DiffuseMaterial();
                    var renderer = g.AddComponent<MeshRenderer>();
                    renderer.Add(mesh, material);
                    var settings = renderer.GetSettings(0);
                    settings.cullFace = true;
                    settings.depthTest = true;
                    break;
                }
                case PrimitiveType.ParticleSystem:
                {
                    g.SetLayer(Layer.Default | Layer.IgnoreCulling | Layer.IgnoreRaycast, true);
                    var renderer = g.AddComponent<ParticleSystem>();
                    renderer.renderQueue = 1002;
                    break;
                }
                case PrimitiveType.ProceduralSkybox:
                {
                    g.SetLayer(Layer.Default | Layer.IgnoreCulling | Layer.IgnoreRaycast, true);
                    //A cube doesn't work so well with the procedural skybox shader
                    var mesh = Resources.FindMesh(Constants.GetString(ConstantString.MeshSphere));
                    var material = new ProceduralSkyboxMaterial();
                    var renderer = g.AddComponent<MeshRenderer>();
                    renderer.castShadows = false;
                    renderer.receiveShadows = false;
                    renderer.Add(mesh, material);
                    renderer.renderQueue = 999;
                    var settings = renderer.GetSettings(0);
                    settings.cullFace = false;
                    settings.depthTest = false;
                    break;
                }
                case PrimitiveType.Skybox:
                {
                    g.SetLayer(Layer.Default | Layer.IgnoreCulling | Layer.IgnoreRaycast, true);
                    var mesh = Resources.FindMesh(Constants.GetString(ConstantString.MeshSkybox));
                    var material = new SkyboxMaterial();
                    var renderer = g.AddComponent<MeshRenderer>();
                    renderer.castShadows = false;
                    renderer.receiveShadows = false;
                    renderer.Add(mesh, material);
                    renderer.renderQueue = 999;
                    var settings = renderer.GetSettings(0);
                    settings.cullFace = false;
                    settings.depthTest = false;
                    break;
                }
                case PrimitiveType.Terrain:
                {
                    g.AddComponent<Terrain>();
                    break;
                }
            }

            return g;
        }

        internal static void EndFrame()
        {
            if(m_destroyQueue.Count == 0)
                return;

            for(int i = 0; i < m_destroyQueue.Count; i++)
            {
                GameObject currentObject = m_destroyQueue[i];

                if(currentObject == null)
                    continue;

                DestroyImmediate(currentObject);
            }

            m_destroyQueue.Clear();
        }

        public static void Destroy(GameObject g)
        {
            if(g == null)
            {
                System.Console.WriteLine("Cant destroy a GameObject that is null");
                return;
            }

            m_destroyQueue.Add(g);
        }

        private static void DestroyImmediate(GameObject g)
        {
            for(int i = 0; i < g.m_components.Count; i++)
            {
                g.m_components[i].OnDestroyComponent();
            }

            var children = g.transform.GetChildrenRecursive();

            for(int i = 0; i < children.Count; i++)
            {
                GameObject currentObject = children[i].gameObject;
                
                for(int j = 0; j < currentObject.m_components.Count; j++)
                {
                    currentObject.m_components[j].OnDestroyComponent();
                }
            }

            for(int i = 0; i < children.Count; i++)
            {
                RemoveObject(children[i].gameObject);
            }

            RemoveObject(g);
        }

        private static void RemoveObject(GameObject g)
        {
            if(g == null)
                return;
            
            int index = 0;
            bool found = false;

            for(int i = 0; i < m_objects.Count; i++)
            {
                if(g.instanceId == m_objects[i].instanceId)
                {
                    index = i;
                    found = true;
                    break;
                }
            }

            if(found)
            {
                g.transform.SetParent(null);
                m_objects.RemoveAt(index);
            }
        }
    }
}