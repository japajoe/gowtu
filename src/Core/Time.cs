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

        internal static void NewFrame()
        {
            timer.Update();
            elapsed += timer.GetDeltaTime();
        }
    }

    public sealed class Timer
    {
        private DateTime tp1;
        private DateTime tp2;
        private float deltaTime;

        public Timer()
        {
            tp1 = DateTime.Now;
            tp2 = DateTime.Now;
            deltaTime = 0;
        }

        public float GetDeltaTime()
        {
            return deltaTime;
        }

        public void Update()
        {
            tp2 = DateTime.Now;
            TimeSpan timeSpan = tp2 - tp1;
            deltaTime = (float)timeSpan.TotalSeconds;
            tp1 = tp2;
        }
    }
}