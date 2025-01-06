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
using OpenTK.Mathematics;

namespace Gowtu
{
    public delegate void TransformChangedEvent(Transform sender);

    public sealed class Transform : Component
    {
        public event TransformChangedEvent Changed;

        private Transform m_parent;
        private Transform m_root;        
        private List<Transform> m_children;        
        private Matrix4 m_translationMatrix;
        private Matrix4 m_rotationMatrix;
        private Matrix4 m_scaleMatrix;
        private Matrix4 m_modelMatrix;
        private Vector3 m_localPosition;
        private Vector3 m_localScale;
        private Quaternion m_localRotation;
        private Vector3 m_velocity;
        private Vector3 m_oldPosition;
        private Vector3 m_newRotation;
        private Vector3 m_rotationAccumulator;

        public Vector3 position
        {
            get
            {
                return GetPosition();
            }
            set
            {
                SetPosition(value);
            }
        }

        public Vector3 localPosition
        {
            get
            {
                return GetLocalPosition();
            }
            set
            {
                SetLocalPosition(value);
            }
        }


        public Vector3 scale
        {
            get
            {
                return GetScale();
            }
            set
            {
                SetScale(value);
            }
        }

        public Vector3 localScale
        {
            get
            {
                return GetLocalScale();
            }
            set
            {
                SetLocalScale(value);
            }
        }

        public Quaternion rotation
        {
            get
            {
                return GetRotation();
            }
            set
            {
                SetRotation(value);
            }
        }

        public Quaternion localRotation
        {
            get
            {
                return GetLocalRotation();
            }
            set
            {
                SetLocalRotation(value);
            }
        }

        public Vector3 velocity
        {
            get
            {
                return m_velocity;
            }
        }

        public Vector3 right
        {
            get
            {
                return rotation * Vector3.UnitX;
            }
            set
            {
                rotation = Quaternion.FromAxisAngle(Vector3.Cross(Vector3.UnitX, value), Vector3.CalculateAngle(Vector3.UnitX, value));
            }
        }

        public Vector3 up
        {
            get
            {
                return rotation * Vector3.UnitY;
            }
            set
            {
                rotation = Quaternion.FromAxisAngle(Vector3.Cross(Vector3.UnitY, value), Vector3.CalculateAngle(Vector3.UnitY, value));
            }
        }

        public Vector3 forward
        {
            get
            {
                return rotation * -Vector3.UnitZ;
            }
            set
            {
                Vector3 currentForward = rotation * -Vector3.UnitZ;
                float angle = (float)Math.Acos(Vector3.Dot(currentForward.Normalized(), value.Normalized()));
                Vector3 axis = Vector3.Cross(currentForward, value).Normalized();
                rotation = Quaternion.FromAxisAngle(axis, angle);
            }
        }

        public Transform parent
        {
            get
            {
                return m_parent;
            }
            set
            {
                SetParent(value);
            }
        }

        public Transform root
        {
            get
            {
                return m_root;
            }
        }

        public List<Transform> children
        {
            get
            {
                return m_children;
            }
        }

        public Transform() : base()
        {
            m_children = new List<Transform>();
            m_parent = null;
            m_root = this;

            m_localPosition = new Vector3(0, 0, 0);
            m_localScale = new Vector3(1, 1, 1);
            m_localRotation = Quaternion.Identity;
            m_velocity = new Vector3();
            m_oldPosition = new Vector3(0, 0, 0);
            m_newRotation = new Vector3(0, 0, 0);
            m_rotationAccumulator = new Vector3(0, 0, 0);

            m_translationMatrix = Matrix4.CreateTranslation(0, 0, 0);
            m_rotationMatrix = Matrix4.CreateFromQuaternion(Quaternion.Identity);
            m_scaleMatrix = Matrix4.CreateScale(1, 1, 1);
            
            SetTranslationMatrix();
            SetRotationMatrix();
            SetScaleMatrix();
            //SetModelMatrix();
        }

        public Matrix4 GetModelMatrix()
        {
            if (m_parent != null)
                return m_modelMatrix * m_parent.GetModelMatrix();
            else
                return m_modelMatrix;
        }

        Transform GetChild(int index)
        {
            if(index >= m_children.Count)
                return null;
            return m_children[index];
        }

        public List<Transform> GetChildrenRecursive()
        {
            List<Transform> allChildren = new List<Transform>();

            foreach (var child in m_children)
            {
                allChildren.Add(child); // Add the current child
                List<Transform> childChildren = child.GetChildrenRecursive(); // Recursively get children's children
                allChildren.AddRange(childChildren); // Add the child children to the list
            }

            return allChildren;
        }

        public void SetParent(Transform newParent)
        {
            if(newParent == this)
                return; //Can't set parent to self

            if (m_parent != null)
            {
                int index = m_parent.m_children.FindIndex(child => child == this);

                if (index != -1)
                {
                    m_parent.m_children.RemoveAt(index);
                }
            }

            m_parent = newParent;

            if (m_parent != null)
                m_parent.m_children.Add(this);

            Transform currentRoot = this;
            
            while (currentRoot.m_parent != null)
            {
                currentRoot = currentRoot.m_parent;
            }

            m_root = currentRoot;
        }

        public void Rotate(Quaternion rotation)
        {
            SetRotation(rotation);
        }

        public void Rotate(Vector3 rotation)
        {
            var oldRotation = m_rotationAccumulator;

            m_rotationAccumulator.X += rotation.X;
            m_rotationAccumulator.Y += rotation.Y;
            m_rotationAccumulator.Z += rotation.Z;

            var rotationDelta = m_rotationAccumulator - oldRotation;

            var rott = this.rotation;

            const float Deg2Rad = (float)Math.PI / 180.0f;

            rott = rott * Quaternion.FromAxisAngle(Vector3.UnitX, rotationDelta.X * Deg2Rad);
            rott = rott * Quaternion.FromAxisAngle(Vector3.UnitY, rotationDelta.Y * Deg2Rad);
            rott = rott * Quaternion.FromAxisAngle(Vector3.UnitZ, -rotationDelta.Z * Deg2Rad);

            SetRotation(rott);
        }

        public void LookAt(Transform target)
        {
            LookAt(target, Vector3.UnitY);
        }

        public void LookAt(Transform target, Vector3 worldUp)
        {
            if (target == null)
                return;
            LookAt(target.GetPosition(), worldUp);
        }

        public void LookAt(Vector3 worldPosition, Vector3 worldUp)
        {
            var pos = GetPosition();
            Matrix4 mat = Matrix4.LookAt(pos, worldPosition, worldUp);
            Quaternion rot = Quaternion.FromMatrix(new Matrix3(mat));
            Rotate(rot);
        }

        public Vector3 InverseTransformDirection(Vector3 direction)
        {
            var dir = direction * new Matrix3(GetModelMatrix().Inverted());
            return new Vector3(dir.X, dir.Y, dir.Z);
        }

        public Vector3 TransformDirection(Vector3 direction)
        {
            Vector3 v = rotation * direction;
            return v;
        }

        public static void TransformDirection(ref Vector3 vector, ref Quaternion rotation, out Vector3 result)
        {
            float x = rotation.X + rotation.X;
            float y = rotation.Y + rotation.Y;
            float z = rotation.Z + rotation.Z;
            float wx = rotation.W * x;
            float wy = rotation.W * y;
            float wz = rotation.W * z;
            float xx = rotation.X * x;
            float xy = rotation.X * y;
            float xz = rotation.X * z;
            float yy = rotation.Y * y;
            float yz = rotation.Y * z;
            float zz = rotation.Z * z;

            result = new Vector3(vector.X * (1.0f - yy - zz) + vector.Y * (xy - wz) + vector.Z * (xz + wy),
                                 vector.X * (xy + wz) + vector.Y * (1.0f - xx - zz) + vector.Z * (yz - wx),
                                 vector.X * (xz - wy) + vector.Y * (yz + wx) + vector.Z * (1.0f - xx - yy));
        }

        public Vector3 WorldToLocal(Vector3 v)
        {
            var invScale = GetScale();
            if (invScale.X != 0.0f)
                invScale.X = 1.0f / invScale.X;
            if (invScale.Y != 0.0f)
                invScale.Y = 1.0f / invScale.Y;
            if (invScale.Z != 0.0f)
                invScale.Z = 1.0f / invScale.Z;
            Quaternion invRotation = Quaternion.Conjugate(GetRotation());
            Vector3 result = v - GetPosition();
            result = Vector3.Transform(result, invRotation);
            result *= invScale;
            return result;
        }

        public Vector3 WorldToLocalVector(Vector3 v)
        {
            var invScale = GetScale();
            if (invScale.X != 0.0f)
                invScale.X = 1.0f / invScale.X;
            if (invScale.Y != 0.0f)
                invScale.Y = 1.0f / invScale.Y;
            if (invScale.Z != 0.0f)
                invScale.Z = 1.0f / invScale.Z;
            Quaternion invRotation = Quaternion.Conjugate(GetRotation());
            Vector3 result = Vector3.Transform(v, invRotation);
            result *= invScale;
            return result;
        }

        public Vector3 LocalToWorld(Vector3 v)
        {
            Vector3 tmp = v * GetScale();
            tmp = Vector3.Transform(tmp, GetRotation());
            return tmp + GetPosition();
        }

        public Vector3 LocalToWorldVector(Vector3 v)
        {
            Vector3 tmp = v * GetScale();
            return Vector3.Transform(tmp, GetRotation());
        }

        private void SetTranslationMatrix()
        {
            m_translationMatrix = Matrix4.CreateTranslation(m_localPosition.X, m_localPosition.Y, m_localPosition.Z);
            SetModelMatrix();
        }

        private void SetRotationMatrix()
        {
            m_rotationMatrix = Matrix4.CreateFromQuaternion(m_localRotation);
            SetModelMatrix();
        }

        private void SetScaleMatrix()
        {
            m_scaleMatrix = Matrix4.CreateScale(m_localScale.X, m_localScale.Y, m_localScale.Z);
            SetModelMatrix();
        }

        private void SetModelMatrix()
        {
            m_modelMatrix = m_scaleMatrix * m_rotationMatrix * m_translationMatrix;
            Changed?.Invoke(this);
        }

        private void SetPosition(Vector3 value)
        {
            if (m_parent != null)
            {
                var localToWorld = Matrix4.Invert(GetModelMatrix());
                var localPosition = GetLocalPosition() + Vector3.TransformPosition(value, localToWorld);
                SetLocalPosition(localPosition);
            }
            else
            {
                SetLocalPosition(value);
            }
        }

        private Vector3 GetPosition()
        {
            if(m_parent != null)
            {
                var localMatrix = GetModelMatrix();
                return localMatrix.ExtractTranslation();
            }
            else
            {
                return GetLocalPosition();
            }
        }

        private void SetLocalPosition(Vector3 value)
        {
            m_oldPosition = m_localPosition;    
            m_localPosition = value;

            float dx = m_localPosition.X - m_oldPosition.X;
            float dy = m_localPosition.Y - m_oldPosition.Y;
            float dz = m_localPosition.Z - m_oldPosition.Z;
            
            float deltaTime = Time.DeltaTime;

            m_velocity = new Vector3(dx / deltaTime, dy / deltaTime, dz / deltaTime);

            SetTranslationMatrix();
        }

        private Vector3 GetLocalPosition()
        {
            return m_localPosition;
        }

        private void SetRotation(Quaternion value)
        {
            if(m_parent != null)
            {
                SetLocalRotation(Quaternion.Invert(m_parent.GetRotation()) * value);
            }
            else
            {
                SetLocalRotation(value);
            }
        }

        private Quaternion GetRotation()
        {
            if(m_parent != null)
            {
                var localToWorld = GetModelMatrix();
                return localToWorld.ExtractRotation();
            }
            else
            {
                return GetLocalRotation();
            }
        }

        private void SetLocalRotation(Quaternion value)
        {
            m_localRotation = value;
            SetRotationMatrix();
        }

        private Quaternion GetLocalRotation()
        {
            return m_localRotation;
        }

        private void SetScale(Vector3 value)
        {
            SetLocalScale(value);
        }

        private Vector3 GetScale()
        {
            if (m_parent != null)
            {
                var localToWorld = GetModelMatrix();
                return localToWorld.ExtractScale();
            }
            else
            {
                return GetLocalScale();
            }
        }

        private void SetLocalScale(Vector3 value)
        {
            m_localScale = value;
            SetScaleMatrix();
        }

        private Vector3 GetLocalScale()
        {
            return m_localScale;
        }
    }
}