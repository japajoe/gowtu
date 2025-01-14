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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Gowtu
{
    public abstract class GameBehaviour : Component
    {
        private static List<Behaviour> behaviours = new List<Behaviour>();
        

        public GameBehaviour() : base()
        {

        }

        internal override void OnInitializeComponent()
        {
            Add(this);
        }

        internal override void OnDestroyComponent()
        {
            Remove(this);
        }

        internal static void Add(GameBehaviour m)
        {
            Type type = m.GetType();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            var behaviour = new Behaviour(m as Component);

            for (int i = 0; i < methods.Length; i++)
            {
                Delegate del = CreateDelegate(m, methods[i]);

                if (methods[i].Name == "Awake")
                {
                    behaviour.Awake += (Action)del;
                }
                else if (methods[i].Name == "Start")
                {
                    behaviour.Start += (Action)del;
                }
                else if (methods[i].Name == "Update")
                {
                    behaviour.Update += (Action)del;
                }
                else if (methods[i].Name == "LateUpdate")
                {
                    behaviour.LateUpdate += (Action)del;
                }
                else if (methods[i].Name == "FixedUpdate")
                {
                    behaviour.FixedUpdate += (Action)del;
                }
                else if (methods[i].Name == "OnEnable")
                {
                    behaviour.Enable += (Action)del;
                }
                else if (methods[i].Name == "OnDisable")
                {
                    behaviour.Disable += (Action)del;
                }
                else if (methods[i].Name == "OnGUI")
                {
                    behaviour.GUI += (Action)del;
                }
                else if (methods[i].Name == "OnCollision")
                {
                    behaviour.Collision += (Action<Rigidbody, Rigidbody>)del;
                }
                else if (methods[i].Name == "OnDestroy")
                {
                    behaviour.Destroy += (Action)del;
                }
                else if (methods[i].Name == "OnApplicationQuit")
                {
                    behaviour.ApplicationQuit += (Action)del;
                }
                else if (methods[i].Name == "OnRender")
                {
                    behaviour.Render += (Action)del;
                }
                else if (methods[i].Name == "OnResourceLoaded")
                {
                    behaviour.ResourceLoaded += (Action<Resource>)del;
                }
                else if (methods[i].Name == "OnResourceBatchLoaded")
                {
                    behaviour.ResourceBatchLoaded += (Action<ResourceBatch>)del;
                }
            }

            behaviour.instanceId = m.instanceId;
            behaviours.Add(behaviour);

            behaviour.OnEnable();
            behaviour.OnAwake();
            behaviour.OnStart();
        }

        internal static void Remove(GameBehaviour m)
        {
            for (int i = behaviours.Count; i >= 0; i--)
            {
                if (behaviours[i].instanceId == m.instanceId)
                {
                    behaviours.RemoveAt(i);
                    break;
                }
            }
        }

        private static Delegate CreateDelegate(object instance, MethodInfo method)
        {
            var parameters = method.GetParameters()
                       .Select(p => Expression.Parameter(p.ParameterType, p.Name))
                        .ToArray();

            var call = Expression.Call(Expression.Constant(instance), method, parameters);
            return Expression.Lambda(call, parameters).Compile();
        }

        internal static void NewFrame()
        {
            OnBehaviourUpdate();
            OnBehaviourLateUpdate();
        }

        internal static void OnBehaviourDisable(Guid instanceId)
        {
            for(int i = 0; i < behaviours.Count; i++)
            {
                if (behaviours[i].instanceId == instanceId)
                {
                    behaviours[i].OnDisable();
                    return;
                }
            }
        }

        internal static void OnBehaviourEnable(Guid instanceId)
        {
            for(int i = 0; i < behaviours.Count; i++)
            {
                if (behaviours[i].instanceId == instanceId)
                {
                    behaviours[i].OnEnable();
                    return;
                }
            }
        }

        internal static void OnApplicationClosing()
        {
            for(int i = 0; i < behaviours.Count; i++)
            {
                if(behaviours[i].GameObject.isActive)
                    behaviours[i].OnApplicationQuit();
            }

            behaviours.Clear();
        }

        internal static void OnBehaviourUpdate()
        {
            for(int i = 0; i < behaviours.Count; i++)
            {
                if(behaviours[i].GameObject.isActive)
                    behaviours[i].OnUpdate();
            }
        }

        internal static void OnBehaviourLateUpdate()
        {
            for(int i = 0; i < behaviours.Count; i++)
            {
                if(behaviours[i].GameObject.isActive)
                    behaviours[i].OnLateUpdate();
            }
        }

        internal static void OnBehaviourFixedUpdate()
        {
            for(int i = 0; i < behaviours.Count; i++)
            {
                if(behaviours[i].GameObject.isActive)
                    behaviours[i].OnFixedUpdate();
            }
        }

        internal static void OnBehaviourGUI()
        {
            for(int i = 0; i < behaviours.Count; i++)
            {
                if(behaviours[i].GameObject.isActive)
                    behaviours[i].OnGUI();
            }
        }

        internal static void OnBehaviourCollision(Rigidbody rb1, Rigidbody rb2)
        {
            for(int i = 0; i < behaviours.Count; i++)
            {
                if(behaviours[i].GameObject == rb1.gameObject || behaviours[i].GameObject == rb2.gameObject)
                {
                    behaviours[i].OnCollision(rb1, rb2);
                }
            }
        }

        internal static void OnBehaviourResourceLoaded(Resource resource)
        {
            for(int i = 0; i < behaviours.Count; i++)
            {
                //Dispatch to any behaviour implementing this method, 
                //regardless if it is active or not
                behaviours[i].OnResourceLoaded(resource);
            }
        }

        internal static void OnBehaviourResourceBatchLoaded(ResourceBatch batch)
        {
            for(int i = 0; i < behaviours.Count; i++)
            {
                //Dispatch to any behaviour implementing this method, 
                //regardless if it is active or not
                behaviours[i].OnResourceBatchLoaded(batch);
            }
        }
    }

    internal class Behaviour
    {
        public Guid instanceId;

        public event Action Update;
        public event Action LateUpdate;
        public event Action FixedUpdate;
        public event Action Start;
        public event Action Awake;
        public event Action Enable;
        public event Action Disable;
        public event Action GUI;
        public event Action<Rigidbody, Rigidbody> Collision;
        public event Action Destroy;
        public event Action ApplicationQuit;
        public event Action Render;
        public event Action<Resource> ResourceLoaded;
        public event Action<ResourceBatch> ResourceBatchLoaded;

        private Component behaviour;

        public GameObject GameObject
        {
            get { return behaviour.gameObject; }
        }

        public Behaviour(Component behaviour)
        {
            this.behaviour = behaviour;
        }

        public void OnDisable()
        {
            Disable?.Invoke();
        }

        public void OnEnable()
        {
            Enable?.Invoke();
        }

        public void OnAwake()
        {
            Awake?.Invoke();
        }

        public void OnStart()
        {
            Start?.Invoke();
        }

        public void OnUpdate()
        {
            Update?.Invoke();
        }

        public void OnLateUpdate()
        {
            LateUpdate?.Invoke();
        }

        public void OnFixedUpdate()
        {
            FixedUpdate?.Invoke();
        }

        public void OnGUI()
        {
            GUI?.Invoke();
        }

        public void OnCollision(Rigidbody rb1, Rigidbody rb2)
        {
            Collision?.Invoke(rb1, rb2);
        }

        public void OnDestroy()
        {
            Destroy?.Invoke();
        }

        public void OnApplicationQuit()
        {
            ApplicationQuit?.Invoke();
        }

        public void OnRender()
        {
            Render?.Invoke();
        }

        public void OnResourceLoaded(Resource resource)
        {
            ResourceLoaded?.Invoke(resource);
        }

        public void OnResourceBatchLoaded(ResourceBatch batch)
        {
            ResourceBatchLoaded?.Invoke(batch);
        }
    }
}