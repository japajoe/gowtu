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
using System.Runtime.InteropServices;
using GLFWNet;
using OpenTK;
using OpenTK.Graphics;

namespace Gowtu
{
    [Flags]
    public enum WindowFlags
    {
        None = 1 << 0,
        VSync = 1 << 1,
        FullScreen = 1 << 2,
        Maximize = 1 << 3
    }

    public struct Configuration
    {
        public string title;
        public int width;
        public int height;
        public WindowFlags flags;
        public byte[] iconData;
    }

    public delegate void LoadEvent();

    public sealed class Application
    {
        public event LoadEvent Load;

        private Configuration config;
        private IntPtr window;
        private static IntPtr nativeWindow;

        public static IntPtr NativeWindow
        {
            get
            {
                return nativeWindow;
            }
        }

        public Application(int width, int height, string title, WindowFlags flags = WindowFlags.VSync)
        {
            config.width = width;
            config.height = height;
            config.title = title;
            config.flags = flags;
            config.iconData = null;
            window = IntPtr.Zero;
            nativeWindow = IntPtr.Zero;
        }

        public Application(Configuration config)
        {
            this.config = config;
            window = IntPtr.Zero;
            nativeWindow = IntPtr.Zero;
        }

        public void Run()
        {
            if(window != IntPtr.Zero)
            {
                Console.WriteLine("Window is already initialized");
                return;
            }

            if(GLFW.Init() == 0)
            {
                Console.WriteLine("Failed to initialize GLFW");
                return;
            }

            GLFW.WindowHint(GLFW.CONTEXT_VERSION_MAJOR, 3);
            GLFW.WindowHint(GLFW.CONTEXT_VERSION_MINOR, 3);
            GLFW.WindowHint(GLFW.OPENGL_PROFILE, GLFW.OPENGL_CORE_PROFILE);
            GLFW.WindowHint(GLFW.VISIBLE, GLFW.FALSE);
            GLFW.WindowHint(GLFW.SAMPLES, 4);

            if(config.flags.HasFlag(WindowFlags.Maximize))
                GLFW.WindowHint(GLFW.MAXIMIZED, GLFW.TRUE);

            if(config.flags.HasFlag(WindowFlags.FullScreen))
            {
                IntPtr monitor =  GLFW.GetPrimaryMonitor();

                if(GLFW.GetVideoMode(monitor, out GLFWvidmode mode))
                    window = GLFW.CreateWindow(mode.width, mode.height, config.title, monitor, IntPtr.Zero);
                else
                    window = GLFW.CreateWindow(config.width, config.height, config.title, IntPtr.Zero, IntPtr.Zero);
            }
            else
            {
                window = GLFW.CreateWindow(config.width, config.height, config.title, IntPtr.Zero, IntPtr.Zero);
            }

            if(window == IntPtr.Zero)
            {
                GLFW.Terminate();
                Console.WriteLine("Failed to create window");
                return;
            }

            if(config.iconData != null)
            {
                Image image = new Image(config.iconData);

                if(image.IsLoaded)
                {
                    GLFWimage windowIcon = new GLFWimage();
                    windowIcon.width = (int)image.Width;
                    windowIcon.height = (int)image.Height;
                    
                    windowIcon.pixels = Marshal.AllocHGlobal(image.Data.Length);
                    
                    if(windowIcon.pixels != IntPtr.Zero)
                    {
                        Marshal.Copy(image.Data, 0, windowIcon.pixels, image.Data.Length);
                        
                        GLFWimage[] images = new GLFWimage[1]
                        {
                            windowIcon
                        };
                        
                        GLFW.SetWindowIcon(window, images);

                        Marshal.FreeHGlobal(windowIcon.pixels);
                    }
                }
            }

            nativeWindow = window;

            GLFW.SwapInterval(config.flags.HasFlag(WindowFlags.VSync) ? 1 : 0);

            GLFW.MakeContextCurrent(window);

            GLLoader.LoadBindings(new GLFWBindingsContext());

            string version = OpenTK.Graphics.OpenGL.GL.GetString(OpenTK.Graphics.OpenGL.StringName.Version);

            if(!string.IsNullOrEmpty(version))
            {
                Console.WriteLine("OpenGL Version: " + version);
            }

            GLFW.SetFramebufferSizeCallback(window, OnWindowResize);
            GLFW.SetWindowPosCallback(window, OnWindowMove);
            GLFW.SetKeyCallback(window, OnKeyPress);
            GLFW.SetCharCallback(window, OnCharPress);
            GLFW.SetMouseButtonCallback(window, OnMouseButtonPress);
            GLFW.SetScrollCallback(window, OnMouseScroll);

            if(AudioSettings.Load("audiosettings.dat"))
            {
                var deviceInfo = new MiniAudioEx.DeviceInfo(IntPtr.Zero, AudioSettings.DeviceId);
                Audio.Initialize(AudioSettings.OutputSampleRate, 2, deviceInfo);
            }
            else
            {
                Audio.Initialize(44100, 2);
            }

            Graphics.Initialize();

            //Loads default shaders/textures/meshes
            Resources.LoadDefault();

            Load?.Invoke();

            GLFW.ShowWindow(window);

            while(GLFW.WindowShouldClose(window) == 0)
            {
                NewFrame();
                EndFrame();
                GLFW.PollEvents();
                GLFW.SwapBuffers(window);
            }

            GameBehaviour.OnApplicationClosing();

            Audio.Deinitialize();
            Graphics.Deinitialize();

            GLFW.DestroyWindow(window);

            window = IntPtr.Zero;
            nativeWindow = IntPtr.Zero;

            GLFW.Terminate();
        }

        private void NewFrame()
        {
            Time.NewFrame();
            Input.NewFrame();
            GameBehaviour.NewFrame();
            Resources.NewFrame();
            Audio.NewFrame();
            Graphics.NewFrame();
        }

        private void EndFrame()
        {
            Input.EndFrame();
            GameObject.EndFrame();
        }

        public static void Quit()
        {
            GLFW.SetWindowShouldClose(NativeWindow, GLFW.TRUE);
        }

        private void OnWindowResize(IntPtr window, int width, int height)
        {
            Graphics.SetViewport(0, 0, width, height);
        }

        private void OnWindowMove(IntPtr window, int x, int y)
        {
            Input.SetWindowPosition(x, y);
        }

        private void OnKeyPress(IntPtr window, int key, int scancode, int action, int mods)
        {
            Input.SetKeyState((KeyCode)key, action > 0 ? 1 : 0);
        }

        private void OnCharPress(IntPtr window, uint codepoint)
        {
            Input.AddInputCharacter(codepoint);
        }

        private void OnMouseButtonPress(IntPtr window, int button, int action, int mods)
        {
            Input.SetButtonState((ButtonCode)button, action > 0 ? 1 : 0);
        }

        private void OnMouseScroll(IntPtr window, double xoffset, double yoffset)
        {
            Input.SetScrollDirection(xoffset, yoffset);
        }
    }

    internal sealed class GLFWBindingsContext : IBindingsContext
    {
        public IntPtr GetProcAddress(string procName)
        {
            return Marshal.GetFunctionPointerForDelegate(GLFW.GetProcAddress(procName));
        }
    }
}