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
    public enum ForceMode
    {
        Force,          //Add a continuous force to the rigidbody, using its mass.
        Impulse        //Add an instant force impulse to the rigidbody, using its mass.
    }

    public sealed class Rigidbody : Component
    {
        private bool m_kinematic = false;
        private float m_mass = 1.0f;
        private float m_drag = 0;
        private float m_angularDrag = 0;
        private Vector3 m_velocity = Vector3.Zero;
        private Vector3 m_angularVelocity = Vector3.Zero;
        private RigidBody internalBody;

        public RigidBody rigidBody
        {
            get
            {
                return internalBody;
            }
        }

        public float mass
        {
            get
            {
                return m_mass;
            }
            set
            {
                m_mass = value;
                if(internalBody != null)
                {
                    SetMass();
                }
            }
        }

        public bool gravity
        {
            get
            {
                if(internalBody == null)
                    return false;
                return internalBody.Gravity.LengthSquared() > 0;
            }
            set
            {
                if(internalBody == null)
                    return;
                if(value == true)
                    internalBody.Gravity = new System.Numerics.Vector3(0, -9.81f, 0);
                else
                    internalBody.Gravity = new System.Numerics.Vector3(0, 0, 0);
            }
        }

        public bool isKinematic
        {
            get
            {
                return m_kinematic;
            }
            set
            {
                m_kinematic = value;
            }
        }

        public Vector3 velocity
        {
            get 
            {
                if (internalBody == null)
                    return Vector3.Zero;
                return new Vector3(internalBody.LinearVelocity.X, internalBody.LinearVelocity.Y, internalBody.LinearVelocity.Z);
            }
            set 
            { 
                m_velocity = value; 
                if(internalBody != null)
                {
                    SetVelocity();
                }
            }
        }

        public Vector3 angularVelocity
        {
            get 
            {
                if (internalBody == null)
                    return Vector3.Zero;
                return new Vector3(internalBody.AngularVelocity.X, internalBody.AngularVelocity.Y, internalBody.AngularVelocity.Z);
            }
            set 
            { 
                m_angularVelocity = value; 
                if(internalBody != null)
                {
                    SetAngularVelocity();
                }
            }            
        }

        public float angularDrag
        {
            get
            {
                if (internalBody == null)
                    return m_angularDrag;
                
                return internalBody.AngularDamping;
            }
            set
            {
                m_angularDrag = value;
                if (internalBody != null)
                {
                    internalBody.SetDamping(internalBody.LinearDamping, m_angularDrag);
                }
            }
        }

        public float drag
        {
            get
            {
                if (internalBody == null)
                    return m_drag;
                
                return internalBody.LinearDamping;
            }
            set
            {
                m_drag = value;
                if (internalBody != null)
                {
                    internalBody.SetDamping(m_drag, internalBody.AngularDamping);
                }
            }
        }

        public Rigidbody() : base()
        {
            
        }

        internal override void OnInitializeComponent()
        {
            var collider = gameObject.GetComponentOfSubType<Collider>();

            if(collider != null)
            {                
                Physics.Add(this);
            }
        }

        internal override void OnDestroyComponent()
        {
            Physics.Remove(this);
        }

        public void SetBounciness(float bounciness)
        {
            internalBody.Restitution = bounciness;
        }

        public void MovePosition(Vector3 position)
        {
            Activate();
            rigidBody.GetWorldTransform(out var transformation);
            if(System.Numerics.Matrix4x4.Decompose(transformation, out var scale, out var rotation, out var _))
            {
                var t = System.Numerics.Matrix4x4.CreateTranslation(new System.Numerics.Vector3(position.X, position.Y, position.Z));
                var r = System.Numerics.Matrix4x4.CreateFromQuaternion(rotation);
                var s = System.Numerics.Matrix4x4.CreateScale(scale);
                rigidBody.WorldTransform = r * s * t;
            }
        }

        public void MoveRotation(Quaternion rotation)
        {
            Activate();
            rigidBody.GetWorldTransform(out var transformation);
            if(System.Numerics.Matrix4x4.Decompose(transformation, out var scale, out var _, out var translation))
            {
                var t = System.Numerics.Matrix4x4.CreateTranslation(translation);
                var r = System.Numerics.Matrix4x4.CreateFromQuaternion(new System.Numerics.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W));
                var s = System.Numerics.Matrix4x4.CreateScale(scale);
                rigidBody.WorldTransform = r * s * t;
            }
        }

        public float GetFriction()
        {
            if (internalBody == null)
                return 0;
            return internalBody.Friction;
        }

        public void SetFriction(float f)
        {
            if (internalBody == null)
                return;
            internalBody.Friction = f;
        }

        internal void SetRigidBody(RigidBody rb)
        {
            this.internalBody = rb;
        }

        public void AddTorque(Vector3 torque, ForceMode forceMode = ForceMode.Impulse)
        {
            Activate();
            if(forceMode == ForceMode.Impulse)
                internalBody.ApplyTorqueImpulse(new System.Numerics.Vector3(torque.X, torque.Y, torque.Z));
            else if(forceMode == ForceMode.Force)
                internalBody.ApplyTorque(new System.Numerics.Vector3(torque.X, torque.Y, torque.Z));
            
        }

        public void AddRelativeTorque(Vector3 torque, ForceMode forceMode = ForceMode.Impulse)
        {
            Activate();

            torque = transform.TransformDirection(torque);

            if(forceMode == ForceMode.Impulse)
                internalBody.ApplyTorqueImpulse(new System.Numerics.Vector3(torque.X, torque.Y, torque.Z));
            else if(forceMode == ForceMode.Force)
                internalBody.ApplyTorque(new System.Numerics.Vector3(torque.X, torque.Y, torque.Z));
        }

        public void AddRelativeForce(Vector3 force)
        {
            Activate();
            internalBody.ApplyCentralForce(new System.Numerics.Vector3(force.X, force.Y, force.Z));
        }

        private void SetVelocity()
        {
            Activate();
            internalBody.LinearVelocity = new System.Numerics.Vector3(m_velocity.X, m_velocity.X, m_velocity.X);
        }

        private void SetAngularVelocity()
        {
            Activate();
            internalBody.AngularVelocity = new System.Numerics.Vector3(m_angularVelocity.X, m_angularVelocity.X, m_angularVelocity.X);
        }

        public Vector3 GetPointVelocity(Vector3 relativePosition)
        {
            var com = internalBody.CenterOfMassPosition;
            var relPos = new System.Numerics.Vector3(relativePosition.X - com.X, relativePosition.Y - com.Y, relativePosition.Z - com.Z);
            internalBody.GetVelocityInLocalPoint(ref relPos, out System.Numerics.Vector3 result);
            return new Vector3(result.X, result.Y, result.Z);
        }

        public void AddForce(Vector3 force, ForceMode forceMode = ForceMode.Impulse)
        {
            Activate();
            if(forceMode == ForceMode.Impulse)
                internalBody.ApplyCentralImpulse(new System.Numerics.Vector3(force.X, force.Y, force.Z));
            else if(forceMode == ForceMode.Force)
                internalBody.ApplyCentralForce(new System.Numerics.Vector3(force.X, force.Y, force.Z));
        }

        public void AddForceAtPosition(Vector3 force, Vector3 relativePosition)
        {
            Activate();
            internalBody.ApplyForce(new System.Numerics.Vector3(force.X, force.Y, force.Z), new System.Numerics.Vector3(relativePosition.X, relativePosition.Y, relativePosition.Z));
        }
        
        public void AddImpulse(Vector3 impulse, Vector3 relativePosition)
        {
            Activate();
            internalBody.ApplyImpulse(new System.Numerics.Vector3(impulse.X, impulse.Y, impulse.Z), new System.Numerics.Vector3(relativePosition.X, relativePosition.Y, relativePosition.Z));
        }

        private void SetMass()
        {
            var inertia = internalBody.LocalInertia;
            internalBody.SetMassProps(m_mass, inertia);
        }

        private void Activate()
        {
            switch (internalBody.ActivationState)
            {
                case ActivationState.IslandSleeping:
                    internalBody.Activate(true);
                    break;
                case ActivationState.Undefined:
                case ActivationState.ActiveTag:
                case ActivationState.WantsDeactivation:
                case ActivationState.DisableDeactivation:
                case ActivationState.DisableSimulation:
                default:
                    break;
            }
        }
    }
}