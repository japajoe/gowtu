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
using OpenTK.Mathematics;

namespace Gowtu
{
    public struct BoundingBox
    {
        private Vector3 min;
        private Vector3 max;
        private Vector3 center;
        private Vector3 extents;
        private bool hasPoint;

        public Vector3 Min
        {
            get
            {
                return min;
            }
        }

        public Vector3 Max
        {
            get
            {
                return max;
            }
        }

        public Vector3 Center
        {
            get
            {
                return center;
            }
        }

        public Vector3 Extents
        {
            get
            {
                return extents;
            }
        }

        public bool HasPoint
        {
            get
            {
                return hasPoint;
            }
        }

        public Vector3 Size
        {
            get
            {
                return max - min;
            }
        }

        public BoundingBox()
        {
            Clear();
        }

        public BoundingBox(Vector3 min, Vector3 max)
        {
            Clear();
            this.min = min;
            this.max = max;
            center = (min + max) * 0.5f;
            extents = max - center;
            hasPoint = true;
        }

        public void Clear()
        {
            min = Vector3.One * float.PositiveInfinity;
            max = Vector3.One * float.NegativeInfinity;
            center = (min + max) * 0.5f;
            extents = max - center;
            hasPoint = false;
        }

        public void Grow(Vector3 point)
        {
            min = Vector3Min(min, point);
            max = Vector3Max(max, point);
            center = (min + max) * 0.5f;
            extents = max - center;
            hasPoint = true;
        }

        public void Grow(Vector3 min, Vector3 max)
        {
            if (hasPoint)
            {
                this.min.X = min.X < this.min.X ? min.X : this.min.X;
                this.min.Y = min.Y < this.min.Y ? min.Y : this.min.Y;
                this.min.Z = min.Z < this.min.Z ? min.Z : this.min.Z;
                this.max.X = max.X > this.max.X ? max.X : this.max.X;
                this.max.Y = max.Y > this.max.Y ? max.Y : this.max.Y;
                this.max.Z = max.Z > this.max.Z ? max.Z : this.max.Z;
            }
            else
            {
                hasPoint = true;
                this.min = min;
                this.max = max;
            }
        }

        public void Transform(Matrix4 transformation)
        {
            var vMin = new Vector4(min.X, min.Y, min.Z, 1.0f) * transformation;
            var vMax = new Vector4(max.X, max.Y, max.Z, 1.0f) * transformation;
            min = vMin.Xyz;
            max = vMax.Xyz;
            center = (min + max) * 0.5f;
            extents = max - center;
        }

        private bool IsFloatIsZero(float value)
        {
            const float zeroTolerance = 1e-6f;
            return Math.Abs(value) < zeroTolerance;
        }

        public bool Intersects(Ray ray, out float distance)
        {
            distance = 0.0f;
            float tmax = float.MaxValue;

            if (IsFloatIsZero(ray.direction.X))
            {
                if (ray.origin.X < min.X || ray.origin.X > max.X)
                {
                    distance = 0.0f;
                    return false;
                }
            }
            else
            {
                float inverse = 1.0f / ray.direction.X;
                float t1 = (min.X - ray.origin.X) * inverse;
                float t2 = (max.X - ray.origin.X) * inverse;

                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                distance = Math.Max(t1, distance);
                tmax = Math.Min(t2, tmax);

                if (distance > tmax)
                {
                    distance = 0.0f;
                    return false;
                }
            }

            if (IsFloatIsZero(ray.direction.Y))
            {
                if (ray.origin.Y < min.Y || ray.origin.Y > max.Y)
                {
                    distance = 0.0f;
                    return false;
                }
            }
            else
            {
                float inverse = 1.0f / ray.direction.Y;
                float t1 = (min.Y - ray.origin.Y) * inverse;
                float t2 = (max.Y - ray.origin.Y) * inverse;

                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                distance = Math.Max(t1, distance);
                tmax = Math.Min(t2, tmax);

                if (distance > tmax)
                {
                    distance = 0.0f;
                    return false;
                }
            }

            if (IsFloatIsZero(ray.direction.Z))
            {
                if (ray.origin.Z < min.Z || ray.origin.Z > max.Z)
                {
                    distance = 0.0f;
                    return false;
                }
            }
            else
            {
                float inverse = 1.0f / ray.direction.Z;
                float t1 = (min.Z - ray.origin.Z) * inverse;
                float t2 = (max.Z - ray.origin.Z) * inverse;

                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                distance = Math.Max(t1, distance);
                tmax = Math.Min(t2, tmax);

                if (distance > tmax)
                {
                    distance = 0.0f;
                    return false;
                }
            }

            return true;
        }

        private static Vector3 Vector3Min(Vector3 a, Vector3 b)
        {
            return new Vector3(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
        }

        private static Vector3 Vector3Max(Vector3 a, Vector3 b)
        {
            return new Vector3(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
        }
    }
}