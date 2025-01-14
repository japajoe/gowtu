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

using BulletSharp;
using OpenTK.Mathematics;

namespace Gowtu
{
    public sealed class CompoundCollider : Collider
    {
        public CompoundCollider() : base()
        {

        }

        public void AddShape(CollisionShape collisionShape, Vector3 position, Quaternion rotation)
        {
            if(shape == null)
                shape = new CompoundShape();

            CompoundShape compound = (CompoundShape)shape;

            var pos = new System.Numerics.Vector3(position.X, position.Y, position.Z);
            var rot = new System.Numerics.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
            var scale = new System.Numerics.Vector3(1, 1, 1);
                        
            var translationMatrix = System.Numerics.Matrix4x4.CreateTranslation(pos);
            var rotationMatrix = System.Numerics.Matrix4x4.CreateFromQuaternion(rot);
            var scaleMatrix = System.Numerics.Matrix4x4.CreateScale(scale);

            var m = scaleMatrix * rotationMatrix * translationMatrix;

            compound.AddChildShape(m, collisionShape);
        }

        public void Initialize()
        {
            OnInitializeComponent();
        }
        
        internal override void OnInitializeComponent()
        {
            if(shape == null)
                return;
            
            var rigidbody = gameObject.GetComponent<Rigidbody>();

            if(rigidbody != null)
            {
                rigidbody.OnInitializeComponent();                
            }
        }
    }
}
