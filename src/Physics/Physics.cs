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

using OpenTK.Mathematics;
using BulletSharp;
using System.Collections.Generic;
using System;

namespace Gowtu
{
    public static class Physics
    {
        class RigidbodyInfo
        {
            public Rigidbody body = null;
            public RigidBody rigidBody = null;
            public CollisionShape collisionShape = null;
        }

        private static bool isInitialized = false;
        private static DbvtBroadphase broadphase;
        private static DefaultCollisionConfiguration collisionConfiguration;
        private static CollisionDispatcher dispatcher;
        private static SequentialImpulseConstraintSolver solver;
        private static DiscreteDynamicsWorld dynamicsWorld;
        private static System.Numerics.Vector3 gravity;
        private static readonly double fixedUpdateTimeStep = 1.0 / 50;
        private static double fixedUpdateTimer;
        private static List<RigidbodyInfo> rigidbodyInfo;

        public static DiscreteDynamicsWorld DynamicsWorld
        {
            get
            {
                return dynamicsWorld;
            }
        }

        internal static void Initialize()
        {
            if (isInitialized)
                return;

            rigidbodyInfo = new List<RigidbodyInfo>();

            gravity = new System.Numerics.Vector3(0, -9.81f, 0);

            broadphase = new DbvtBroadphase();
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            solver = new SequentialImpulseConstraintSolver();
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collisionConfiguration);
            //dynamicsWorld.SetInternalTickCallback(WorldPreTickCallback);
            dynamicsWorld.SetGravity(ref gravity);

            isInitialized = true;
        }

        internal static void Deinitialize()
        {
            if (!isInitialized)
                return;

            //cleanup in the reverse order of creation/initialization
            for(int i = rigidbodyInfo.Count -1; i >= 0; i--)
            {
                dynamicsWorld.RemoveRigidBody(rigidbodyInfo[i].rigidBody);
                rigidbodyInfo[i].rigidBody.MotionState.Dispose();
                rigidbodyInfo[i].rigidBody.Dispose();
                rigidbodyInfo[i].collisionShape.Dispose();
            }

            dynamicsWorld.Dispose();
            solver.Dispose();
            broadphase.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();

            isInitialized = false;
        }

        internal static void Add(Rigidbody rb)
        {
            if (rb == null)
            {
                System.Console.WriteLine("Failed to add to physics world because there is no rigidbody: " + rb.instanceId);
                return;
            }

            GameObject gameObject = rb.gameObject;

            RigidbodyInfo rbInfo = new RigidbodyInfo();
            rbInfo.body = rb;

            var components = gameObject.GetComponents();

            Collider collider = null;

            if (components != null)
            {
                for (int i = 0; i < components.Count; i++)
                {
                    if (components[i].GetType().IsSubclassOf(typeof(Collider)))
                    {
                        collider = components[i] as Collider;
                        break;
                    }
                }
            }

            if(collider == null)
            {
                System.Console.WriteLine("Failed to add to physics world because there is no collider: " + rb.instanceId);
                return;
            }

            var localInertia = new System.Numerics.Vector3();

            if (rb.mass > float.Epsilon)
                collider.shape.CalculateLocalInertia(rb.mass, out localInertia);

            var rot = gameObject.transform.rotation;
            var rotation = new System.Numerics.Quaternion(rot.X, rot.Y, rot.Z, rot.W);

            var orientation = System.Numerics.Matrix4x4.CreateFromQuaternion(rotation);
            var position = System.Numerics.Matrix4x4.CreateTranslation(gameObject.transform.position.X, gameObject.transform.position.Y, gameObject.transform.position.Z);

            var t = gameObject.transform.GetModelMatrix();

            var transformation = new System.Numerics.Matrix4x4(t.M11, t.M12, t.M13, t.M14, t.M21, t.M22, t.M23, t.M24,
                                               t.M31, t.M32, t.M33, t.M34, t.M41, t.M42, t.M43, t.M44);

            var motionState = new DefaultMotionState(transformation);
            var rigidBodyCI = new RigidBodyConstructionInfo(rb.mass, motionState, collider.shape, localInertia);
            var rigidBody = new RigidBody(rigidBodyCI);

            if(rb.isKinematic)
                rigidBody.CollisionFlags |= CollisionFlags.KinematicObject;

            rigidBody.SetDamping(rb.drag, rb.angularDrag);
                
            rbInfo.rigidBody = rigidBody;
            rbInfo.collisionShape = collider.shape;
            dynamicsWorld.AddRigidBody(rigidBody);
            rigidBody.UserObject = rb;
            rb.SetRigidBody(rigidBody);
            rigidbodyInfo.Add(rbInfo);

            System.Console.WriteLine("Added to physics world: " + rb.instanceId);
        }

        internal static void Remove(Rigidbody rb)
        {
            if (rb == null)
                return;

            dynamicsWorld.RemoveRigidBody(rb.rigidBody);

            int index = -1;

            for(int i = 0; i < rigidbodyInfo.Count; i++)
            {
                var instanceId = rigidbodyInfo[i].body.instanceId;

                if(instanceId == rb.instanceId)
                {
                    rigidbodyInfo[i].rigidBody.MotionState.Dispose();
                    rigidbodyInfo[i].rigidBody.Dispose();
                    rigidbodyInfo[i].collisionShape.Dispose();
                    index = i;
                    break;
                }
            }

            if(index >= 0)
            {
                rigidbodyInfo.RemoveAt(index);
            }
        }

        internal static void NewFrame()
        {
            float deltaTime = Time.DeltaTime;

            // Cap delta time to prevent large jumps
            fixedUpdateTimer += Math.Min(deltaTime, fixedUpdateTimeStep);

            // Fixed Update Logic
            while (fixedUpdateTimer >= fixedUpdateTimeStep) 
            {
                FixedUpdate(); // Call the fixed update method
                fixedUpdateTimer -= fixedUpdateTimeStep; // Decrease the timer by the fixed time step
            }
        }

        private static void FixedUpdate()
        {
            GameBehaviour.OnBehaviourFixedUpdate();

            if (!isInitialized)
                return;

            dynamicsWorld.StepSimulation(Time.DeltaTime, 1, (float)fixedUpdateTimeStep);

            for (int i = 0; i < rigidbodyInfo.Count; i++)
            {
                CollisionObject obj = dynamicsWorld.CollisionObjectArray[i];
                RigidBody body = RigidBody.Upcast(obj);
                Rigidbody g = (Rigidbody)obj.UserObject;

                if(!g.gameObject.isActive)
                    continue;

                if (body != null && body.MotionState != null)
                {
                    body.MotionState.GetWorldTransform(out System.Numerics.Matrix4x4 transformation);

                    if (i >= 0)
                    {
                        var newPosition = new OpenTK.Mathematics.Vector3(transformation.Translation.X, transformation.Translation.Y, transformation.Translation.Z);
                        var rotation = transformation.GetRotation();
                        g.transform.position = newPosition;
                        g.transform.rotation = new OpenTK.Mathematics.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
                    }
                }
            }
        }

        public static bool Raycast(Ray ray, out RaycastHit hit, Layer layerMask = 0)
        {
            return Raycast(ray.origin, ray.direction, ray.length, out hit, layerMask);
        }

        public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, out RaycastHit hit, Layer layerMask = 0)
        {
            hit = new RaycastHit();

            TriangleIntersection intersection = new TriangleIntersection();

            Renderer renderer = null;
            int currentIndex = 0;

            while((renderer = Graphics.GetRendererByIndex(currentIndex++)) != null)
            {
                if(!renderer.gameObject.isActive)
                    continue;

                Layer layer = (Layer)renderer.gameObject.layer;

                if(layer.HasFlag(Layer.IgnoreRaycast))
                        continue;

                if(layerMask != 0)
                {
                    if(layer.HasFlag(layerMask))
                        continue;
                }

                Transform transform = renderer.transform;
                Matrix4 transformation = transform.GetModelMatrix();
                Ray ray = new Ray(transform.WorldToLocal(origin), transform.WorldToLocalVector(direction), float.MaxValue);

                Mesh mesh = null;
                int meshIndex = 0;
                
                while((mesh = renderer.GetMesh(meshIndex++)) != null)
                {
                    var indices = mesh.Indices;
                    
                    if(indices.Length == 0)
                        continue;

                    var bounds = mesh.Bounds;

                    float distance = 0;
                    
                    if(!bounds.Intersects(ray, out distance))
                        continue;
                    
                    var vertices = mesh.Vertices;

                    for(int j = 0; j < indices.Length / 3; j++)
                    {
                        float currIntersectionPos = 0.0f;

                        Vector3 v1 = vertices[(int)indices[j * 3]].position;
                        Vector3 v2 = vertices[(int)indices[j * 3 + 1]].position;
                        Vector3 v3 = vertices[(int)indices[j * 3 + 2]].position;

                        Vector4 v1t = new Vector4(v1.X, v1.Y, v1.Z, 1.0f) * transformation;
                        Vector4 v2t = new Vector4(v2.X, v2.Y, v2.Z, 1.0f) * transformation;
                        Vector4 v3t = new Vector4(v3.X, v3.Y, v3.Z, 1.0f) * transformation;

                        v1 = new Vector3(v1t.X, v1t.Y, v1t.Z);
                        v2 = new Vector3(v2t.X, v2t.Y, v2t.Z);
                        v3 = new Vector3(v3t.X, v3t.Y, v3t.Z);

                        if (RayIntersectsTriangle(origin, direction, v1, v2, v3, out currIntersectionPos))
                        {
                            if (currIntersectionPos < intersection.lastPos)
                            {
                                intersection.lastPos = currIntersectionPos;
                                intersection.triangleIndex1 = indices[j*3];
                                intersection.triangleIndex2 = indices[j*3+1];
                                intersection.triangleIndex3 = indices[j*3+2];
                                intersection.transform = renderer.transform;
                                hit.normal = SurfaceNormalFromIndices(v1, v2, v3);
                            }
                        }
                    }
                }
            }

            if(intersection.triangleIndex1 >= 0)
            {
                float totalDistance = Vector3.Distance(origin, origin + (direction * intersection.lastPos));

                if(totalDistance <= maxDistance)
                {
                    hit.point = origin + (direction * intersection.lastPos);
                    hit.distance = Vector3.Distance(origin, hit.point);
                    hit.triangleIndex1 = intersection.triangleIndex1;
                    hit.triangleIndex2 = intersection.triangleIndex2;
                    hit.triangleIndex3 = intersection.triangleIndex3;
                    hit.transform = intersection.transform;
                    return true;
                }
            }

            return false;
        }

        private static bool RayIntersectsTriangle(Vector3 origin, Vector3 dir, Vector3 v0, Vector3 v1, Vector3 v2, out float intersection)
        {
            intersection = 0;

            // Triangle edges
            Vector3 e1 = (v1 -v0);
            Vector3 e2 = (v2 -v0);

            const float epsilon = 0.000001f;

            Vector3 P, Q;
            //float i;
            float t;

            // Calculate determinant
            P = Vector3.Cross(dir, e2);
            float det = Vector3.Dot(e1, P);
            // If determinant is (close to) zero, the ray lies in the plane of the triangle or parallel it's plane
            if ((det > -epsilon) && (det < epsilon))
            {
                return false;
            }
            float invDet = 1.0f / det;

            // Distance from first vertex to ray origin
            Vector3 T = origin - v0;

            // Calculate u parameter
            float u = Vector3.Dot(T, P) * invDet;
            // Intersection point lies outside of the triangle
            if ((u < 0.0f) || (u > 1.0f))
            {
                return false;
            }

            //Prepare to test v parameter
            Q = Vector3.Cross(T, e1);

            // Calculate v parameter
            float v = Vector3.Dot(dir, Q) * invDet;
            // Intersection point lies outside of the triangle
            if (v < 0.0f || u + v > 1.0f) 
                return false;

            // Calculate t
            t = Vector3.Dot(e2, Q) * invDet;

            if (t > epsilon)
            {
                // Triangle interesected
                intersection = t;
                return true;
            }

            // No intersection
            return false;
        }

        private static Vector3 SurfaceNormalFromIndices(Vector3 pA, Vector3 pB, Vector3 pC)
        {
            Vector3 sideAB = pB - pA;
            Vector3 sideAC = pC - pA;
            return Vector3.Normalize(Vector3.Cross(sideAB, sideAC));
        }
    }

    public struct TriangleIntersection
    {
        public object userData;
        public Transform transform;
        public uint triangleIndex1;
        public uint triangleIndex2;
        public uint triangleIndex3;
        public float lastPos;
        public Vector3 normal;

        public TriangleIntersection()
        {
            userData = null;
            transform = null;
            triangleIndex1 = 0;
            triangleIndex2 = 0;
            triangleIndex3 = 0;
            lastPos = float.MaxValue;
            normal = Vector3.Zero;
        }
    }

    public struct RaycastHit
    {
        public float distance;
        public Vector3 point;
        public Vector3 normal;
        public Transform transform;
        public uint triangleIndex1;
        public uint triangleIndex2;
        public uint triangleIndex3;
    }

    public struct Ray
    {
        public Vector3 origin;
        public Vector3 direction;
        public float length;
        
        public Ray()
        {
            origin = Vector3.Zero;
            direction = -Vector3.UnitZ;
            length = 10000.0f;
        }

        public Ray(Vector3 origin, Vector3 direction, float length)
        {
            this.origin = origin;
            this.direction = direction;
            this.length = length;
        }

        public static Ray FromMousePosition(Vector2 mousePosition, Viewport viewportRect)
        {
            Vector2 viewportPosition = new Vector2(viewportRect.x, viewportRect.y);
            Vector2 relativeMousePosition = mousePosition - viewportPosition;

            float mouseX = relativeMousePosition.X;
            float mouseY = viewportRect.height - relativeMousePosition.Y;

            Vector4 rayStartNDC = new Vector4(
                (mouseX / viewportRect.width - 0.5f) * 2.0f,  // [0,1024] -> [-1,1]
                (mouseY / viewportRect.height - 0.5f) * 2.0f, // [0, 768] -> [-1,1]
                -1.0f, // The near plane maps to Z=-1 in Normalized Device Coordinates
                1.0f
            );

            Vector4 rayEndNDC = new Vector4(
                (mouseX /  viewportRect.width - 0.5f) * 2.0f,
                (mouseY /  viewportRect.height - 0.5f) * 2.0f,
                0.0f,
                1.0f
            );

            Camera mainCamera = Camera.mainCamera;

            if(mainCamera == null)
                return default(Ray);

            Matrix4 viewMatrix = mainCamera.GetViewMatrix();
            Matrix4 projectionMatrix = mainCamera.GetProjectionMatrix();

            Matrix4 m = Matrix4.Invert(viewMatrix * projectionMatrix);
            Vector4 rayStartWorld = rayStartNDC * m; 
            rayStartWorld /= rayStartWorld.W;
            Vector4 rayEndWorld = rayEndNDC * m;
            rayEndWorld /= rayEndWorld.W;

            Vector3 rayDirWorld = (rayEndWorld.Xyz - rayStartWorld.Xyz);
            rayDirWorld = Vector3.Normalize(rayDirWorld);

            Vector3 origin = rayStartWorld.Xyz;
            Vector3 direction = Vector3.Normalize(rayDirWorld);

            return new Ray(origin, direction, mainCamera.farClippingPlane);
        }
    };
}