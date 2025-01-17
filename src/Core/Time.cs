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
using System.Diagnostics;

namespace Gowtu
{
    public static class Time
    {
        private static Timer timer = new Timer();
        private static float elapsed;

        public static float DeltaTime
        {
            get => timer.GetDeltaTime();
        }

        public static float Elapsed
        {
            get => elapsed;
        }

        public static float FPS
        {
            get => timer.GetFPS();
        }

        internal static void NewFrame()
        {
            timer.Update();
            elapsed += timer.GetDeltaTime();
        }
    }

    public sealed class Timer
    {
        private Stopwatch sw;
        private float deltaTime;
        private float lastFrameTime;
        private float fpsTimer;
        private float averageFPS;
        private int fps;

        public Timer()
        {
            sw = Stopwatch.StartNew();
            deltaTime = 0;
            lastFrameTime = 0;
            fpsTimer = 0;
            averageFPS = 0;
            fps = 0;
        }

        public float GetDeltaTime()
        {
            return deltaTime;
        }

        public float GetFPS()
        {
            return averageFPS;
        }

        public void Update()
        {
            float currentFrameTime = (float)sw.Elapsed.TotalSeconds;
            deltaTime = currentFrameTime - lastFrameTime;
            lastFrameTime = currentFrameTime;

            fpsTimer += deltaTime;

            fps++;

            if (fpsTimer > 0.5f)
            {
                averageFPS = (float)fps / fpsTimer;
                fps = 0;
                fpsTimer = 0.0f;
            }
        }
    }
}