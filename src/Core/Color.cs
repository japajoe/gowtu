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

using System.Runtime.InteropServices;

namespace Gowtu
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Color
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public Color(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public Color(int r, int g, int b, int a)
        {
            this.r = Normalize(r);
            this.g = Normalize(g);
            this.b = Normalize(b);
            this.a = Normalize(a);
        }

        private float Normalize(int value)
        {
            if(value <= 0)
                return 0;
            return (float)value / 255.0f;
        }

        public static Color LightGray
        {
            get => new Color(200, 200, 200, 255);
        }

        public static Color Gray
        {
            get => new Color(130, 130, 130, 255);
        }

        public static Color DarkGray
        {
            get => new Color(80, 80, 80, 255);
        }

        public static Color Yellow
        {
            get => new Color(253, 249, 0, 255);
        }

        public static Color Gold
        {
            get => new Color(255, 203, 0, 255);
        }

        public static Color Orange
        {
            get => new Color(255, 161, 0, 255);
        }

        public static Color Pink
        {
            get => new Color(255, 109, 194, 255);
        }

        public static Color Red
        {
            get => new Color(255, 0, 0, 255);
        }

        public static Color Maroon
        {
            get => new Color(190, 33, 55, 255);
        }

        public static Color Green
        {
            get => new Color(0, 255, 0, 255);
        }

        public static Color Lime
        {
            get => new Color(0, 158, 47, 255);
        }

        public static Color LightGreen
        {
            get => new Color(13, 224, 77, 255);
        }

        public static Color DarkGreen
        {
            get => new Color(0, 117, 44, 255);
        }

        public static Color SkyBlue
        {
            get => new Color(102, 191, 255, 255);
        }

        public static Color Blue
        {
            get => new Color(0, 0, 255, 255);
        }

        public static Color DarkBlue
        {
            get => new Color(0, 82, 172, 255);
        }

        public static Color Purple
        {
            get => new Color(200, 122, 255, 255);
        }

        public static Color Violet
        {
            get => new Color(135, 60, 190, 255);
        }

        public static Color DarkPurple
        {
            get => new Color(112, 31, 126, 255);
        }

        public static Color Beige
        {
            get => new Color(211, 176, 131, 255);
        }

        public static Color Brown
        {
            get => new Color(127, 106, 79, 255);
        }

        public static Color DarkBrown
        {
            get => new Color(76, 63, 47, 255);
        }

        public static Color White
        {
            get => new Color(255, 255, 255, 255);
        }

        public static Color Black
        {
            get => new Color(0, 0, 0, 255);
        }

        public static Color Blank
        {
            get => new Color(0, 0, 0, 0);
        }

        public static Color Magenta
        {
            get => new Color(255, 0, 255, 255);
        }
    }
}