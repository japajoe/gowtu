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
    public abstract class Renderer : Component
    {
        protected uint m_renderQueue;
        protected bool m_castShadows;
        protected bool m_receiveShadows;

        public uint renderQueue
        {
            get
            {
                return m_renderQueue;
            }
            set
            {
                m_renderQueue = value;
            }
        }

        public bool castShadows
        {
            get
            {
                return m_castShadows;
            }
            set
            {
                m_castShadows = value;
            }
        }
        
        public bool receiveShadows
        {
            get
            {
                return m_receiveShadows;
            }
            set
            {
                m_receiveShadows = value;
            }
        }

        public Renderer() : base()
        {
            m_renderQueue = 1000;
            castShadows = true;
            receiveShadows = true;
        }

        internal override void OnInitializeComponent()
        {
            Graphics.Add(this);
        }

        internal override void OnDestroyComponent()
        {
            Graphics.Remove(this);
        }

        virtual internal void OnRender(Material material)
        {

        }

        virtual internal void OnRender()
        {

        }

        public virtual Mesh GetMesh(int index)
        {
            return null;
        }
    }

    public class RenderSettings
    {
        public bool wireframe;
        public bool depthTest;
        public bool cullFace;
        public bool alphaBlend;
        public DepthFunction depthFunc;
        
        public RenderSettings()
        {
            this.wireframe = false;
            this.depthTest = true;
            this.cullFace = true;
            this.alphaBlend = false;
            this.depthFunc = DepthFunction.Less;
        }
    };
}