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

namespace Gowtu
{
    public abstract class Component : Object
    {
        private GameObject m_gameObject;
        private Transform m_transform;

        public GameObject gameObject
        {
            get => m_gameObject;
        }

        public Transform transform
        {
            get => m_transform;
        }

        public Component() : base()
        {

        }

        internal virtual void OnInitializeComponent()
        {

        }

        internal virtual void OnDestroyComponent()
        {
            
        }

        internal virtual void OnActivateComponent()
        {

        }

        internal virtual void OnDeactivateComponent()
        {
            
        }

        internal void SetGameObject(GameObject gameObject)
        {
            this.m_gameObject = gameObject;
        }

        internal void SetTransform(Transform transform)
        {
            this.m_transform = transform;            
        }
    }
}