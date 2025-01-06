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
    public class FirstPersonCamera : GameBehaviour
    {
        public float speed = 1000.0f;
        public float rotationSpeed = 0.01f;
        public float zoomSpeed = 2000.0f;

        private Camera camera;
        private Vector2 newPosition;
        private Vector2 oldPosition;
        private Vector3 currentRotation;

        private float inputVertical;
        private float inputHorizontal;
        private float inputPanning;
        private float inputZoom;
        private bool mouseIsDown;

        public bool IsControllable
        {
            get;
            set;
        }

        void Awake()
        {
            camera = this.gameObject.GetComponent<Camera>();
            IsControllable = true;
        }

        void Start()
        {
            AxisInfo axisHorizontal = new AxisInfo("Horizontal");
            AxisInfo axisVertical = new AxisInfo("Vertical");
            AxisInfo axisPanning = new AxisInfo("Panning");

            axisHorizontal.AddKeys(KeyCode.D, KeyCode.A);
            axisVertical.AddKeys(KeyCode.W, KeyCode.S);
            axisPanning.AddKeys(KeyCode.R, KeyCode.F);
            
            Input.RegisterAxis(axisHorizontal);
            Input.RegisterAxis(axisVertical);
            Input.RegisterAxis(axisPanning);
        }

        void Update()
        {
            if(!IsControllable)
                return;
            
            inputVertical = Input.GetAxis("Vertical");
            inputHorizontal = Input.GetAxis("Horizontal");
            inputPanning = Input.GetAxis("Panning");
            inputZoom = Input.GetScrollDirection().Y;
            mouseIsDown = Input.GetButton(ButtonCode.Right);
        }

        void LateUpdate()
        {
            if(!IsControllable)
                return;

            if (Math.Abs(inputVertical) > float.Epsilon)
            {
                Move(transform.forward, inputVertical * speed * Time.DeltaTime);
            }

            if (Math.Abs(inputHorizontal) > float.Epsilon)
            {
                Move(transform.right, inputHorizontal * speed * Time.DeltaTime);
            }

            if (Math.Abs(Input.GetScrollDirection().Y) > float.Epsilon)
            {
                Move(transform.forward, inputZoom * zoomSpeed * Time.DeltaTime);
            }

            if (Math.Abs(inputPanning) > float.Epsilon)
            {
                Move(Vector3.UnitY, inputPanning * speed * Time.DeltaTime);
            }

            if (mouseIsDown)
            {
                Rotate();
            }
        }

        void Move(Vector3 direction, float movementSpeed)
        {
            transform.position += direction * movementSpeed;
        }

        void Rotate()
        {
            newPosition = Input.GetMousePosition();

            Vector2 mouseDelta = newPosition - oldPosition;

            if (mouseDelta.Length >= 50)
            {
                oldPosition = newPosition;
                return;
            }

            currentRotation.Y += -mouseDelta.X * rotationSpeed;
            currentRotation.X += -mouseDelta.Y * rotationSpeed;

            currentRotation.X = MathHelper.Clamp(currentRotation.X, MathHelper.DegreesToRadians(-90.0f), MathHelper.DegreesToRadians(90.0f));
            Quaternion rot = Quaternion.FromAxisAngle(Vector3.UnitY, currentRotation.Y) * Quaternion.FromAxisAngle(Vector3.UnitX, currentRotation.X);
            Quaternion rotation = new Quaternion(rot.X, rot.Y, rot.Z, rot.W);

            transform.rotation = rotation;
            oldPosition = newPosition;
        }
    }
}