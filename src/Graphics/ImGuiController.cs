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
using System.Runtime.CompilerServices;
using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using Vector2 = System.Numerics.Vector2;

namespace Gowtu
{
    internal sealed class ImGuiController : IDisposable
    {
        private bool _frameBegun;
        private int _vertexArray;
        private int _vertexBuffer;
        private int _vertexBufferSize;
        private int _indexBuffer;
        private int _indexBufferSize;
        private int _fontTexture;
        private int _shader;
        private int _shaderFontTextureLocation;
        private int _shaderProjectionMatrixLocation;
        private Vector2 _scaleFactor = Vector2.One;
        private static bool KHRDebugAvailable = false;
        private readonly KeyCode[] keycodes;

        /// <summary>
        /// Constructs a new ImGuiController.
        /// </summary>
        public ImGuiController()
        {
            Input.Keyboard.CharPress += OnPressChar;
            Input.Mouse.Scroll += OnMouseScroll;

            keycodes = Enum.GetValues(typeof(KeyCode)) as KeyCode[];

            int major = GL.GetInteger(GetPName.MajorVersion);
            int minor = GL.GetInteger(GetPName.MinorVersion);

            KHRDebugAvailable = (major == 4 && minor >= 3) || IsExtensionSupported("KHR_debug");

            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);
            var io = ImGui.GetIO();
            io.Fonts.AddFontDefault();

            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

            ImGuiConfigFlags configFlags = ImGuiConfigFlags.DockingEnable;
            io.ConfigFlags |= configFlags; 

            CreateDeviceResources();
            SetKeyMappings();

            SetPerFrameImGuiData(1f / 60f);

            ImGui.NewFrame();
            _frameBegun = true;
        }

        private void DestroyDeviceObjects()
        {
            Dispose();
        }

        private void CreateDeviceResources()
        {
            _vertexBufferSize = 10000;
            _indexBufferSize = 2000;

            int prevVAO = GL.GetInteger(GetPName.VertexArrayBinding);
            int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);

            _vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArray);
            LabelObject(ObjectIdentifier.Buffer, _vertexArray, "ImGui");

            _vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBuffer);
            LabelObject(ObjectIdentifier.Buffer, _vertexBuffer, "VBO: ImGui");
            GL.BufferData(BufferTargetARB.ArrayBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageARB.DynamicDraw);

            _indexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, _indexBuffer);
            LabelObject(ObjectIdentifier.Buffer, _indexBuffer, "EBO: ImGui");
            GL.BufferData(BufferTargetARB.ElementArrayBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageARB.DynamicDraw);

            RecreateFontDeviceTexture();

            string VertexSource = @"#version 330 core

uniform mat4 projection_matrix;

layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_texCoord;
layout(location = 2) in vec4 in_color;

out vec4 color;
out vec2 texCoord;

void main()
{
    gl_Position = projection_matrix * vec4(in_position, 0, 1);
    color = in_color;
    texCoord = in_texCoord;
}";
            string FragmentSource = @"#version 330 core

uniform sampler2D in_fontTexture;

in vec4 color;
in vec2 texCoord;

out vec4 outputColor;

void main()
{
    outputColor = color * texture(in_fontTexture, texCoord);
}";

            _shader = CreateProgram("ImGui", VertexSource, FragmentSource);
            _shaderProjectionMatrixLocation = GL.GetUniformLocation(_shader, "projection_matrix");
            _shaderFontTextureLocation = GL.GetUniformLocation(_shader, "in_fontTexture");

            int stride = Unsafe.SizeOf<ImDrawVert>();
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            GL.BindVertexArray(prevVAO);
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, prevArrayBuffer);

            CheckGLError("End of ImGui setup");
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        private void RecreateFontDeviceTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

            int mips = (int)Math.Floor(Math.Log(Math.Max(width, height), 2));

            int prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
            GL.ActiveTexture(TextureUnit.Texture0);
            int prevTexture2D = GL.GetInteger(GetPName.TextureBinding2d);

            _fontTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2d, _fontTexture);
            GL.TexStorage2D(TextureTarget.Texture2d, mips, SizedInternalFormat.Rgba8, width, height);
            LabelObject(ObjectIdentifier.Texture, _fontTexture, "ImGui Text Atlas");

            GL.TexSubImage2D(TextureTarget.Texture2d, 0, 0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

            GL.GenerateMipmap(TextureTarget.Texture2d);

            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMaxLevel, mips - 1);

            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            // Restore state
            GL.BindTexture(TextureTarget.Texture2d, prevTexture2D);
            GL.ActiveTexture((TextureUnit)prevActiveTexture);

            io.Fonts.SetTexID((IntPtr)_fontTexture);

            io.Fonts.ClearTexData();
        }

        /// <summary>
        /// Renders the ImGui draw list data.
        /// </summary>
        public void EndFrame()
        {
            //if (_frameBegun)
            {
                _frameBegun = false;
                ImGui.Render();
                RenderImDrawData(ImGui.GetDrawData());

                // var io = ImGui.GetIO();

                // if(io.ConfigFlags.HasFlag(ImGuiConfigFlags.ViewportsEnable))
                // {   
                //     ImGui.UpdatePlatformWindows();
                //     ImGui.RenderPlatformWindowsDefault();
                //     var window = Gravy.SDLApplication.GetWindow().Handle;
                //     var context = Gravy.SDLApplication.GetWindow().GLContext;
                //     SDL2.SDL.SDL_GL_MakeCurrent(window, context);
                // }
            }
        }

        /// <summary>
        /// Updates ImGui input and IO configuration state.
        /// </summary>
        public void NewFrame()
        {
            float deltaSeconds = Time.DeltaTime;
            
            // if (_frameBegun)
            // {
            //     ImGui.Render();
            // }

            SetPerFrameImGuiData(deltaSeconds);
            UpdateImGuiInput();

            _frameBegun = true;
            ImGui.NewFrame();
        }

        /// <summary>
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            var viewport = Graphics.GetViewport();
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new Vector2(
                viewport.width / _scaleFactor.X,
                viewport.height / _scaleFactor.Y);
            io.DisplayFramebufferScale = _scaleFactor;
            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
        }

        readonly List<char> PressedChars = new List<char>();

        private void UpdateImGuiInput()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            Keyboard keyboard = Input.Keyboard;
            Mouse mouse = Input.Mouse;

            io.MouseDown[0] = mouse.GetState(ButtonCode.Left);
            io.MouseDown[1] = mouse.GetState(ButtonCode.Right);
            io.MouseDown[2] = mouse.GetState(ButtonCode.Middle);

            var mousePosition = mouse.GetPosition();

            io.MousePos = new Vector2(mousePosition.X, mousePosition.Y);
            
            foreach(KeyCode key in keycodes)
            {
                if (key == KeyCode.Unknown)
                {
                    continue;
                }
                io.KeysDown[(int)key] = keyboard.GetState(key);
            }

            foreach (var c in PressedChars)
            {
                io.AddInputCharacter(c);
            }
            
            PressedChars.Clear();

            io.KeyCtrl = keyboard.GetState(KeyCode.LeftControl) || keyboard.GetState(KeyCode.RightControl);
            io.KeyAlt = keyboard.GetState(KeyCode.LeftAlt) || keyboard.GetState(KeyCode.RightAlt);
            io.KeyShift = keyboard.GetState(KeyCode.LeftShift) || keyboard.GetState(KeyCode.RightShift);
            io.KeySuper = keyboard.GetState(KeyCode.LeftSuper) || keyboard.GetState(KeyCode.RightSuper);
        }

        internal void OnPressChar(uint codepoint)
        {
            PressedChars.Add((char)codepoint);
        }

        internal void OnMouseScroll(float offsetX, float offsetY)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            
            io.MouseWheel = offsetY;
            io.MouseWheelH = offsetX;
        }

        private void SetKeyMappings()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.KeyMap[(int)ImGuiKey.Tab] = (int)KeyCode.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)KeyCode.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)KeyCode.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)KeyCode.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)KeyCode.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)KeyCode.PageUp;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)KeyCode.PageDown;
            io.KeyMap[(int)ImGuiKey.Home] = (int)KeyCode.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)KeyCode.End;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)KeyCode.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)KeyCode.Backspace;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)KeyCode.Enter;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)KeyCode.Escape;
            io.KeyMap[(int)ImGuiKey.A] = (int)KeyCode.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)KeyCode.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)KeyCode.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)KeyCode.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)KeyCode.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)KeyCode.Z;
        }

        private void RenderImDrawData(ImDrawDataPtr draw_data)
        {
            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            // Get intial state.
            int prevVAO = GL.GetInteger(GetPName.VertexArrayBinding);
            int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);
            int prevProgram = GL.GetInteger(GetPName.CurrentProgram);
            bool prevBlendEnabled = GL.GetBoolean(GetPName.Blend);
            bool prevScissorTestEnabled = GL.GetBoolean(GetPName.ScissorTest);
            int prevBlendEquationRgb = GL.GetInteger(GetPName.BlendEquationRgb);
            int prevBlendEquationAlpha = GL.GetInteger(GetPName.BlendEquationAlpha);
            int prevBlendFuncSrcRgb = GL.GetInteger(GetPName.BlendSrcRgb);
            int prevBlendFuncSrcAlpha = GL.GetInteger(GetPName.BlendSrcAlpha);
            int prevBlendFuncDstRgb = GL.GetInteger(GetPName.BlendDstRgb);
            int prevBlendFuncDstAlpha = GL.GetInteger(GetPName.BlendDstAlpha);
            bool prevCullFaceEnabled = GL.GetBoolean(GetPName.CullFace);
            bool prevDepthTestEnabled = GL.GetBoolean(GetPName.DepthTest);
            int prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
            GL.ActiveTexture(TextureUnit.Texture0);
            int prevTexture2D = GL.GetInteger(GetPName.TextureBinding2d);
            Span<int> prevScissorBox = stackalloc int[4];
            GL.GetInteger(GetPName.ScissorBox, prevScissorBox);

            // Bind the element buffer (thru the VAO) so that we can resize it.
            GL.BindVertexArray(_vertexArray);
            // Bind the vertex buffer so that we can resize it.
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBuffer);
            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];

                int vertexSize = cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
                if (vertexSize > _vertexBufferSize)
                {
                    int newSize = (int)Math.Max(_vertexBufferSize * 1.5f, vertexSize);
                    
                    GL.BufferData(BufferTargetARB.ArrayBuffer, newSize, IntPtr.Zero, BufferUsageARB.DynamicDraw);
                    _vertexBufferSize = newSize;

                    //Debug.Log($"Resized dear imgui vertex buffer to new size {_vertexBufferSize}");
                }

                int indexSize = cmd_list.IdxBuffer.Size * sizeof(ushort);
                if (indexSize > _indexBufferSize)
                {
                    int newSize = (int)Math.Max(_indexBufferSize * 1.5f, indexSize);
                    GL.BufferData(BufferTargetARB.ElementArrayBuffer, newSize, IntPtr.Zero, BufferUsageARB.DynamicDraw);
                    _indexBufferSize = newSize;

                    //Debug.Log($"Resized dear imgui index buffer to new size {_indexBufferSize}");
                }
            }

            // Setup orthographic projection matrix into our constant buffer
            ImGuiIOPtr io = ImGui.GetIO();
            Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(
                0.0f,
                io.DisplaySize.X,
                io.DisplaySize.Y,
                0.0f,
                -1.0f,
                1.0f);

            GL.UseProgram(_shader);
            GL.UniformMatrix4f(_shaderProjectionMatrixLocation, 1, false, mvp);
            GL.Uniform1i(_shaderFontTextureLocation, 0);
            CheckGLError("Projection");

            GL.BindVertexArray(_vertexArray);
            CheckGLError("VAO");

            draw_data.ScaleClipRects(io.DisplayFramebufferScale);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);
            GL.BlendEquation(BlendEquationModeEXT.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            // Render command lists
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];

                GL.BufferSubData(BufferTargetARB.ArrayBuffer, IntPtr.Zero, cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmd_list.VtxBuffer.Data);
                CheckGLError($"Data Vert {n}");

                GL.BufferSubData(BufferTargetARB.ElementArrayBuffer, IntPtr.Zero, cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data);
                CheckGLError($"Data Idx {n}");

                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        int windowHeight = (int)Graphics.GetViewport().height;

                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2d, (int)pcmd.TextureId);
                        CheckGLError("Texture");

                        // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                        var clip = pcmd.ClipRect;
                        GL.Scissor((int)clip.X, windowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
                        CheckGLError("Scissor");

                        if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                        {
                            GL.DrawElementsBaseVertex(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(pcmd.IdxOffset * sizeof(ushort)), unchecked((int)pcmd.VtxOffset));
                        }
                        else
                        {
                            GL.DrawElements(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (int)pcmd.IdxOffset * sizeof(ushort));
                        }
                        CheckGLError("Draw");
                    }
                }
            }

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.ScissorTest);

            // Reset state
            GL.BindTexture(TextureTarget.Texture2d, prevTexture2D);
            GL.ActiveTexture((TextureUnit)prevActiveTexture);
            GL.UseProgram(prevProgram);
            GL.BindVertexArray(prevVAO);
            GL.Scissor(prevScissorBox[0], prevScissorBox[1], prevScissorBox[2], prevScissorBox[3]);
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, prevArrayBuffer);
            GL.BlendEquationSeparate((BlendEquationModeEXT)prevBlendEquationRgb, (BlendEquationModeEXT)prevBlendEquationAlpha);
            GL.BlendFuncSeparate(
                (BlendingFactor)prevBlendFuncSrcRgb,
                (BlendingFactor)prevBlendFuncDstRgb,
                (BlendingFactor)prevBlendFuncSrcAlpha,
                (BlendingFactor)prevBlendFuncDstAlpha);
            if (prevBlendEnabled) GL.Enable(EnableCap.Blend); else GL.Disable(EnableCap.Blend);
            if (prevDepthTestEnabled) GL.Enable(EnableCap.DepthTest); else GL.Disable(EnableCap.DepthTest);
            if (prevCullFaceEnabled) GL.Enable(EnableCap.CullFace); else GL.Disable(EnableCap.CullFace);
            if (prevScissorTestEnabled) GL.Enable(EnableCap.ScissorTest); else GL.Disable(EnableCap.ScissorTest);
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            GL.DeleteVertexArray(_vertexArray);
            GL.DeleteBuffer(_vertexBuffer);
            GL.DeleteBuffer(_indexBuffer);

            GL.DeleteTexture(_fontTexture);
            GL.DeleteProgram(_shader);
        }

        public static void LabelObject(ObjectIdentifier objLabelIdent, int glObject, string name)
        {
            if (KHRDebugAvailable)
                GL.ObjectLabel(objLabelIdent, (uint)glObject, name.Length, name);
        }

        private bool IsExtensionSupported(string name)
        {
            int n = GL.GetInteger(GetPName.NumExtensions);
            for (uint i = 0; i < n; i++)
            {
                string extension = GL.GetStringi(StringName.Extensions, i);
                if (extension == name) 
                    return true;
            }

            return false;
        }

        private int CreateProgram(string name, string vertexSource, string fragmentSoruce)
        {
            int program = GL.CreateProgram();
            LabelObject(ObjectIdentifier.Program, program, $"Program: {name}");

            int vertex = CompileShader(name, ShaderType.VertexShader, vertexSource);
            int fragment = CompileShader(name, ShaderType.FragmentShader, fragmentSoruce);

            GL.AttachShader(program, vertex);
            GL.AttachShader(program, fragment);

            GL.LinkProgram(program);
            int success = GL.GetProgrami(program, ProgramPropertyARB.LinkStatus);
            if (success == 0)
            {
                GL.GetProgramInfoLog(program, out string info);
                System.Diagnostics.Debug.WriteLine($"GL.LinkProgram had info log [{name}]:\n{info}");
            }

            GL.DetachShader(program, vertex);
            GL.DetachShader(program, fragment);

            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);

            return program;
        }

        private int CompileShader(string name, ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            LabelObject(ObjectIdentifier.Shader, shader, $"Shader: {name}");

            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            int success = GL.GetShaderi(shader, ShaderParameterName.CompileStatus);
            if (success == 0)
            {
                GL.GetShaderInfoLog(shader, out string info);
                System.Diagnostics.Debug.WriteLine($"GL.CompileShader for shader '{name}' [{type}] had info log:\n{info}");
            }

            return shader;
        }

        private void CheckGLError(string title)
        {
            ErrorCode error;
            int i = 1;
            while ((error = GL.GetError()) != ErrorCode.NoError)
            {
                System.Diagnostics.Debug.Print($"{title} ({i++}): {error}");
            }
        }

        internal void InvalidateDeviceObjects()
        {
            RecreateFontDeviceTexture();
        }
    }
}

// Haven't figured out yet how to properly set up newer versions of ImGui.Net
// using System;
// using System.Collections.Generic;
// using System.Runtime.CompilerServices;
// using GLFWNet;
// using ImGuiNET;
// using OpenTK.Graphics.OpenGL;
// using OpenTK.Mathematics;

// using Vector2 = System.Numerics.Vector2;

// namespace Gowtu
// {
//     internal sealed class ImGuiController : IDisposable
//     {
//         private bool _frameBegun;
//         private int _vertexArray;
//         private int _vertexBuffer;
//         private int _vertexBufferSize;
//         private int _indexBuffer;
//         private int _indexBufferSize;
//         private int _fontTexture;
//         private int _shader;
//         private int _shaderFontTextureLocation;
//         private int _shaderProjectionMatrixLocation;
//         private Vector2 _scaleFactor = Vector2.One;
//         private static bool KHRDebugAvailable = false;
//         private readonly KeyCode[] keycodes;

//         /// <summary>
//         /// Constructs a new ImGuiController.
//         /// </summary>
//         public ImGuiController()
//         {
//             Input.Keyboard.CharPress += OnPressChar;
//             Input.Mouse.Scroll += OnMouseScroll;

//             keycodes = Enum.GetValues(typeof(KeyCode)) as KeyCode[];

//             int major = GL.GetInteger(GetPName.MajorVersion);
//             int minor = GL.GetInteger(GetPName.MinorVersion);

//             KHRDebugAvailable = (major == 4 && minor >= 3) || IsExtensionSupported("KHR_debug");

//             IntPtr context = ImGui.CreateContext();
//             ImGui.SetCurrentContext(context);
//             var io = ImGui.GetIO();
//             io.Fonts.AddFontDefault();

//             io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

//             ImGuiConfigFlags configFlags = ImGuiConfigFlags.DockingEnable;
//             io.ConfigFlags |= configFlags; 

//             CreateDeviceResources();

//             SetPerFrameImGuiData(1f / 60f);

//             ImGui.NewFrame();
//             _frameBegun = true;
//         }

//         private void DestroyDeviceObjects()
//         {
//             Dispose();
//         }

//         private void CreateDeviceResources()
//         {
//             _vertexBufferSize = 10000;
//             _indexBufferSize = 2000;

//             int prevVAO = GL.GetInteger(GetPName.VertexArrayBinding);
//             int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);

//             _vertexArray = GL.GenVertexArray();
//             GL.BindVertexArray(_vertexArray);
//             LabelObject(ObjectIdentifier.Buffer, _vertexArray, "ImGui");

//             _vertexBuffer = GL.GenBuffer();
//             GL.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBuffer);
//             LabelObject(ObjectIdentifier.Buffer, _vertexBuffer, "VBO: ImGui");
//             GL.BufferData(BufferTargetARB.ArrayBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageARB.DynamicDraw);

//             _indexBuffer = GL.GenBuffer();
//             GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, _indexBuffer);
//             LabelObject(ObjectIdentifier.Buffer, _indexBuffer, "EBO: ImGui");
//             GL.BufferData(BufferTargetARB.ElementArrayBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageARB.DynamicDraw);

//             RecreateFontDeviceTexture();

//             string VertexSource = @"#version 330 core

// uniform mat4 projection_matrix;

// layout(location = 0) in vec2 in_position;
// layout(location = 1) in vec2 in_texCoord;
// layout(location = 2) in vec4 in_color;

// out vec4 color;
// out vec2 texCoord;

// void main()
// {
//     gl_Position = projection_matrix * vec4(in_position, 0, 1);
//     color = in_color;
//     texCoord = in_texCoord;
// }";
//             string FragmentSource = @"#version 330 core

// uniform sampler2D in_fontTexture;

// in vec4 color;
// in vec2 texCoord;

// out vec4 outputColor;

// void main()
// {
//     outputColor = color * texture(in_fontTexture, texCoord);
// }";

//             _shader = CreateProgram("ImGui", VertexSource, FragmentSource);
//             _shaderProjectionMatrixLocation = GL.GetUniformLocation(_shader, "projection_matrix");
//             _shaderFontTextureLocation = GL.GetUniformLocation(_shader, "in_fontTexture");

//             int stride = Unsafe.SizeOf<ImDrawVert>();
//             GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
//             GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
//             GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

//             GL.EnableVertexAttribArray(0);
//             GL.EnableVertexAttribArray(1);
//             GL.EnableVertexAttribArray(2);

//             GL.BindVertexArray(prevVAO);
//             GL.BindBuffer(BufferTargetARB.ArrayBuffer, prevArrayBuffer);

//             CheckGLError("End of ImGui setup");
//         }

//         /// <summary>
//         /// Recreates the device texture used to render text.
//         /// </summary>
//         private void RecreateFontDeviceTexture()
//         {
//             ImGuiIOPtr io = ImGui.GetIO();
//             io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

//             int mips = (int)Math.Floor(Math.Log(Math.Max(width, height), 2));

//             int prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
//             GL.ActiveTexture(TextureUnit.Texture0);
//             int prevTexture2D = GL.GetInteger(GetPName.TextureBinding2d);

//             _fontTexture = GL.GenTexture();
//             GL.BindTexture(TextureTarget.Texture2d, _fontTexture);
//             GL.TexStorage2D(TextureTarget.Texture2d, mips, SizedInternalFormat.Rgba8, width, height);
//             LabelObject(ObjectIdentifier.Texture, _fontTexture, "ImGui Text Atlas");

//             GL.TexSubImage2D(TextureTarget.Texture2d, 0, 0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

//             GL.GenerateMipmap(TextureTarget.Texture2d);

//             GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
//             GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

//             GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMaxLevel, mips - 1);

//             GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
//             GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

//             // Restore state
//             GL.BindTexture(TextureTarget.Texture2d, prevTexture2D);
//             GL.ActiveTexture((TextureUnit)prevActiveTexture);

//             io.Fonts.SetTexID((IntPtr)_fontTexture);

//             io.Fonts.ClearTexData();
//         }

//         /// <summary>
//         /// Renders the ImGui draw list data.
//         /// </summary>
//         public void EndFrame()
//         {
//             if (_frameBegun)
//             {
//                 _frameBegun = false;
//                 ImGui.Render();
//                 RenderImDrawData(ImGui.GetDrawData());

//                 // var io = ImGui.GetIO();

//                 // if(io.ConfigFlags.HasFlag(ImGuiConfigFlags.ViewportsEnable))
//                 // {   
//                 //     ImGui.UpdatePlatformWindows();
//                 //     ImGui.RenderPlatformWindowsDefault();
//                 //     var window = Gravy.SDLApplication.GetWindow().Handle;
//                 //     var context = Gravy.SDLApplication.GetWindow().GLContext;
//                 //     SDL2.SDL.SDL_GL_MakeCurrent(window, context);
//                 // }
//             }
//         }

//         /// <summary>
//         /// Updates ImGui input and IO configuration state.
//         /// </summary>
//         public void NewFrame()
//         {
//             float deltaSeconds = Time.DeltaTime;
            
//             if (_frameBegun)
//             {
//                 ImGui.Render();
//             }

//             SetPerFrameImGuiData(deltaSeconds);
//             UpdateImGuiInput();

//             _frameBegun = true;
//             ImGui.NewFrame();
//         }

//         /// <summary>
//         /// Sets per-frame data based on the associated window.
//         /// This is called by Update(float).
//         /// </summary>
//         private void SetPerFrameImGuiData(float deltaSeconds)
//         {
//             var viewport = Graphics.GetViewport();
//             ImGuiIOPtr io = ImGui.GetIO();
//             io.DisplaySize = new Vector2(
//                 viewport.width / _scaleFactor.X,
//                 viewport.height / _scaleFactor.Y);
//             io.DisplayFramebufferScale = _scaleFactor;
//             io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
//         }

//         readonly List<char> PressedChars = new List<char>();

//         private void UpdateImGuiInput()
//         {
//             ImGuiIOPtr io = ImGui.GetIO();

//             Keyboard keyboard = Input.Keyboard;
//             Mouse mouse = Input.Mouse;

//             io.MouseDown[0] = mouse.GetState(ButtonCode.Left);
//             io.MouseDown[1] = mouse.GetState(ButtonCode.Right);
//             io.MouseDown[2] = mouse.GetState(ButtonCode.Middle);
//             io.MouseDown[3] = mouse.GetState(ButtonCode.Button4);
//             io.MouseDown[4] = mouse.GetState(ButtonCode.Button5);

//             var mousePosition = mouse.GetPosition();

//             io.MousePos = new Vector2(mousePosition.X, mousePosition.Y);
            
//             foreach(KeyCode key in keycodes)
//             {
//                 if (key == KeyCode.Unknown)
//                 {
//                     continue;
//                 }
                
//                 io.AddKeyEvent(TranslateGLFWKey((int)key), keyboard.GetState(key));
//             }

//             foreach (var c in PressedChars)
//             {
//                 io.AddInputCharacter(c);
//             }
            
//             PressedChars.Clear();

//             io.KeyCtrl = keyboard.GetState(KeyCode.LeftControl) || keyboard.GetState(KeyCode.RightControl);
//             io.KeyAlt = keyboard.GetState(KeyCode.LeftAlt) || keyboard.GetState(KeyCode.RightAlt);
//             io.KeyShift = keyboard.GetState(KeyCode.LeftShift) || keyboard.GetState(KeyCode.RightShift);
//             io.KeySuper = keyboard.GetState(KeyCode.LeftSuper) || keyboard.GetState(KeyCode.RightSuper);
//         }

//         internal void OnPressChar(uint codepoint)
//         {
//             PressedChars.Add((char)codepoint);
//         }

//         internal void OnMouseScroll(float offsetX, float offsetY)
//         {
//             ImGuiIOPtr io = ImGui.GetIO();
            
//             io.MouseWheel = offsetY;
//             io.MouseWheelH = offsetX;
//         }

//         private ImGuiKey TranslateGLFWKey(int key)
//         {
//             switch (key)
//             {
//                 case GLFW.KEY_TAB: return ImGuiKey.Tab;
//                 case GLFW.KEY_LEFT: return ImGuiKey.LeftArrow;
//                 case GLFW.KEY_RIGHT: return ImGuiKey.RightArrow;
//                 case GLFW.KEY_UP: return ImGuiKey.UpArrow;
//                 case GLFW.KEY_DOWN: return ImGuiKey.DownArrow;
//                 case GLFW.KEY_PAGE_UP: return ImGuiKey.PageUp;
//                 case GLFW.KEY_PAGE_DOWN: return ImGuiKey.PageDown;
//                 case GLFW.KEY_HOME: return ImGuiKey.Home;
//                 case GLFW.KEY_END: return ImGuiKey.End;
//                 case GLFW.KEY_INSERT: return ImGuiKey.Insert;
//                 case GLFW.KEY_DELETE: return ImGuiKey.Delete;
//                 case GLFW.KEY_BACKSPACE: return ImGuiKey.Backspace;
//                 case GLFW.KEY_SPACE: return ImGuiKey.Space;
//                 case GLFW.KEY_ENTER: return ImGuiKey.Enter;
//                 case GLFW.KEY_ESCAPE: return ImGuiKey.Escape;
//                 case GLFW.KEY_APOSTROPHE: return ImGuiKey.Apostrophe;
//                 case GLFW.KEY_COMMA: return ImGuiKey.Comma;
//                 case GLFW.KEY_MINUS: return ImGuiKey.Minus;
//                 case GLFW.KEY_PERIOD: return ImGuiKey.Period;
//                 case GLFW.KEY_SLASH: return ImGuiKey.Slash;
//                 case GLFW.KEY_SEMICOLON: return ImGuiKey.Semicolon;
//                 case GLFW.KEY_EQUAL: return ImGuiKey.Equal;
//                 case GLFW.KEY_LEFT_BRACKET: return ImGuiKey.LeftBracket;
//                 case GLFW.KEY_BACKSLASH: return ImGuiKey.Backslash;
//                 case GLFW.KEY_RIGHT_BRACKET: return ImGuiKey.RightBracket;
//                 case GLFW.KEY_GRAVE_ACCENT: return ImGuiKey.GraveAccent;
//                 case GLFW.KEY_CAPS_LOCK: return ImGuiKey.CapsLock;
//                 case GLFW.KEY_SCROLL_LOCK: return ImGuiKey.ScrollLock;
//                 case GLFW.KEY_NUM_LOCK: return ImGuiKey.NumLock;
//                 case GLFW.KEY_PRINT_SCREEN: return ImGuiKey.PrintScreen;
//                 case GLFW.KEY_PAUSE: return ImGuiKey.Pause;
//                 case GLFW.KEY_KP_0: return ImGuiKey.Keypad0;
//                 case GLFW.KEY_KP_1: return ImGuiKey.Keypad1;
//                 case GLFW.KEY_KP_2: return ImGuiKey.Keypad2;
//                 case GLFW.KEY_KP_3: return ImGuiKey.Keypad3;
//                 case GLFW.KEY_KP_4: return ImGuiKey.Keypad4;
//                 case GLFW.KEY_KP_5: return ImGuiKey.Keypad5;
//                 case GLFW.KEY_KP_6: return ImGuiKey.Keypad6;
//                 case GLFW.KEY_KP_7: return ImGuiKey.Keypad7;
//                 case GLFW.KEY_KP_8: return ImGuiKey.Keypad8;
//                 case GLFW.KEY_KP_9: return ImGuiKey.Keypad9;
//                 case GLFW.KEY_KP_DECIMAL: return ImGuiKey.KeypadDecimal;
//                 case GLFW.KEY_KP_DIVIDE: return ImGuiKey.KeypadDivide;
//                 case GLFW.KEY_KP_MULTIPLY: return ImGuiKey.KeypadMultiply;
//                 case GLFW.KEY_KP_SUBTRACT: return ImGuiKey.KeypadSubtract;
//                 case GLFW.KEY_KP_ADD: return ImGuiKey.KeypadAdd;
//                 case GLFW.KEY_KP_ENTER: return ImGuiKey.KeypadEnter;
//                 case GLFW.KEY_KP_EQUAL: return ImGuiKey.KeypadEqual;
//                 case GLFW.KEY_LEFT_SHIFT: return ImGuiKey.LeftShift;
//                 case GLFW.KEY_LEFT_CONTROL: return ImGuiKey.LeftCtrl;
//                 case GLFW.KEY_LEFT_ALT: return ImGuiKey.LeftAlt;
//                 case GLFW.KEY_LEFT_SUPER: return ImGuiKey.LeftSuper;
//                 case GLFW.KEY_RIGHT_SHIFT: return ImGuiKey.RightShift;
//                 case GLFW.KEY_RIGHT_CONTROL: return ImGuiKey.RightCtrl;
//                 case GLFW.KEY_RIGHT_ALT: return ImGuiKey.RightAlt;
//                 case GLFW.KEY_RIGHT_SUPER: return ImGuiKey.RightSuper;
//                 case GLFW.KEY_MENU: return ImGuiKey.Menu;
//                 case GLFW.KEY_0: return ImGuiKey._0;
//                 case GLFW.KEY_1: return ImGuiKey._1;
//                 case GLFW.KEY_2: return ImGuiKey._2;
//                 case GLFW.KEY_3: return ImGuiKey._3;
//                 case GLFW.KEY_4: return ImGuiKey._4;
//                 case GLFW.KEY_5: return ImGuiKey._5;
//                 case GLFW.KEY_6: return ImGuiKey._6;
//                 case GLFW.KEY_7: return ImGuiKey._7;
//                 case GLFW.KEY_8: return ImGuiKey._8;
//                 case GLFW.KEY_9: return ImGuiKey._9;
//                 case GLFW.KEY_A: return ImGuiKey.A;
//                 case GLFW.KEY_B: return ImGuiKey.B;
//                 case GLFW.KEY_C: return ImGuiKey.C;
//                 case GLFW.KEY_D: return ImGuiKey.D;
//                 case GLFW.KEY_E: return ImGuiKey.E;
//                 case GLFW.KEY_F: return ImGuiKey.F;
//                 case GLFW.KEY_G: return ImGuiKey.G;
//                 case GLFW.KEY_H: return ImGuiKey.H;
//                 case GLFW.KEY_I: return ImGuiKey.I;
//                 case GLFW.KEY_J: return ImGuiKey.J;
//                 case GLFW.KEY_K: return ImGuiKey.K;
//                 case GLFW.KEY_L: return ImGuiKey.L;
//                 case GLFW.KEY_M: return ImGuiKey.M;
//                 case GLFW.KEY_N: return ImGuiKey.N;
//                 case GLFW.KEY_O: return ImGuiKey.O;
//                 case GLFW.KEY_P: return ImGuiKey.P;
//                 case GLFW.KEY_Q: return ImGuiKey.Q;
//                 case GLFW.KEY_R: return ImGuiKey.R;
//                 case GLFW.KEY_S: return ImGuiKey.S;
//                 case GLFW.KEY_T: return ImGuiKey.T;
//                 case GLFW.KEY_U: return ImGuiKey.U;
//                 case GLFW.KEY_V: return ImGuiKey.V;
//                 case GLFW.KEY_W: return ImGuiKey.W;
//                 case GLFW.KEY_X: return ImGuiKey.X;
//                 case GLFW.KEY_Y: return ImGuiKey.Y;
//                 case GLFW.KEY_Z: return ImGuiKey.Z;
//                 case GLFW.KEY_F1: return ImGuiKey.F1;
//                 case GLFW.KEY_F2: return ImGuiKey.F2;
//                 case GLFW.KEY_F3: return ImGuiKey.F3;
//                 case GLFW.KEY_F4: return ImGuiKey.F4;
//                 case GLFW.KEY_F5: return ImGuiKey.F5;
//                 case GLFW.KEY_F6: return ImGuiKey.F6;
//                 case GLFW.KEY_F7: return ImGuiKey.F7;
//                 case GLFW.KEY_F8: return ImGuiKey.F8;
//                 case GLFW.KEY_F9: return ImGuiKey.F9;
//                 case GLFW.KEY_F10: return ImGuiKey.F10;
//                 case GLFW.KEY_F11: return ImGuiKey.F11;
//                 case GLFW.KEY_F12: return ImGuiKey.F12;
//                 case GLFW.KEY_F13: return ImGuiKey.F13;
//                 case GLFW.KEY_F14: return ImGuiKey.F14;
//                 case GLFW.KEY_F15: return ImGuiKey.F15;
//                 case GLFW.KEY_F16: return ImGuiKey.F16;
//                 case GLFW.KEY_F17: return ImGuiKey.F17;
//                 case GLFW.KEY_F18: return ImGuiKey.F18;
//                 case GLFW.KEY_F19: return ImGuiKey.F19;
//                 case GLFW.KEY_F20: return ImGuiKey.F20;
//                 case GLFW.KEY_F21: return ImGuiKey.F21;
//                 case GLFW.KEY_F22: return ImGuiKey.F22;
//                 case GLFW.KEY_F23: return ImGuiKey.F23;
//                 case GLFW.KEY_F24: return ImGuiKey.F24;
//                 default: return ImGuiKey.None;
//             }
//         }

//         private void RenderImDrawData(ImDrawDataPtr draw_data)
//         {
//             if (draw_data.CmdListsCount == 0)
//             {
//                 return;
//             }

//             // Get intial state.
//             int prevVAO = GL.GetInteger(GetPName.VertexArrayBinding);
//             int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);
//             int prevProgram = GL.GetInteger(GetPName.CurrentProgram);
//             bool prevBlendEnabled = GL.GetBoolean(GetPName.Blend);
//             bool prevScissorTestEnabled = GL.GetBoolean(GetPName.ScissorTest);
//             int prevBlendEquationRgb = GL.GetInteger(GetPName.BlendEquationRgb);
//             int prevBlendEquationAlpha = GL.GetInteger(GetPName.BlendEquationAlpha);
//             int prevBlendFuncSrcRgb = GL.GetInteger(GetPName.BlendSrcRgb);
//             int prevBlendFuncSrcAlpha = GL.GetInteger(GetPName.BlendSrcAlpha);
//             int prevBlendFuncDstRgb = GL.GetInteger(GetPName.BlendDstRgb);
//             int prevBlendFuncDstAlpha = GL.GetInteger(GetPName.BlendDstAlpha);
//             bool prevCullFaceEnabled = GL.GetBoolean(GetPName.CullFace);
//             bool prevDepthTestEnabled = GL.GetBoolean(GetPName.DepthTest);
//             int prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
//             GL.ActiveTexture(TextureUnit.Texture0);
//             int prevTexture2D = GL.GetInteger(GetPName.TextureBinding2d);
//             Span<int> prevScissorBox = stackalloc int[4];
//             GL.GetInteger(GetPName.ScissorBox, prevScissorBox);

//             // Bind the element buffer (thru the VAO) so that we can resize it.
//             GL.BindVertexArray(_vertexArray);
//             // Bind the vertex buffer so that we can resize it.
//             GL.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBuffer);
//             for (int i = 0; i < draw_data.CmdListsCount; i++)
//             {
//                 //ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];
//                 ImDrawListPtr cmd_list = draw_data.CmdLists[i];

//                 int vertexSize = cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
//                 if (vertexSize > _vertexBufferSize)
//                 {
//                     int newSize = (int)Math.Max(_vertexBufferSize * 1.5f, vertexSize);
                    
//                     GL.BufferData(BufferTargetARB.ArrayBuffer, newSize, IntPtr.Zero, BufferUsageARB.DynamicDraw);
//                     _vertexBufferSize = newSize;

//                     //Debug.Log($"Resized dear imgui vertex buffer to new size {_vertexBufferSize}");
//                 }

//                 int indexSize = cmd_list.IdxBuffer.Size * sizeof(ushort);
//                 if (indexSize > _indexBufferSize)
//                 {
//                     int newSize = (int)Math.Max(_indexBufferSize * 1.5f, indexSize);
//                     GL.BufferData(BufferTargetARB.ElementArrayBuffer, newSize, IntPtr.Zero, BufferUsageARB.DynamicDraw);
//                     _indexBufferSize = newSize;

//                     //Debug.Log($"Resized dear imgui index buffer to new size {_indexBufferSize}");
//                 }
//             }

//             // Setup orthographic projection matrix into our constant buffer
//             ImGuiIOPtr io = ImGui.GetIO();
//             Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(
//                 0.0f,
//                 io.DisplaySize.X,
//                 io.DisplaySize.Y,
//                 0.0f,
//                 -1.0f,
//                 1.0f);

//             GL.UseProgram(_shader);
//             GL.UniformMatrix4f(_shaderProjectionMatrixLocation, 1, false, mvp);
//             GL.Uniform1i(_shaderFontTextureLocation, 0);
//             CheckGLError("Projection");

//             GL.BindVertexArray(_vertexArray);
//             CheckGLError("VAO");

//             draw_data.ScaleClipRects(io.DisplayFramebufferScale);

//             GL.Enable(EnableCap.Blend);
//             GL.Enable(EnableCap.ScissorTest);
//             GL.BlendEquation(BlendEquationModeEXT.FuncAdd);
//             GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
//             GL.Disable(EnableCap.CullFace);
//             GL.Disable(EnableCap.DepthTest);

//             // Render command lists
//             for (int n = 0; n < draw_data.CmdListsCount; n++)
//             {
//                 ImDrawListPtr cmd_list = draw_data.CmdLists[n];

//                 GL.BufferSubData(BufferTargetARB.ArrayBuffer, IntPtr.Zero, cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmd_list.VtxBuffer.Data);
//                 CheckGLError($"Data Vert {n}");

//                 GL.BufferSubData(BufferTargetARB.ElementArrayBuffer, IntPtr.Zero, cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data);
//                 CheckGLError($"Data Idx {n}");

//                 for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
//                 {
//                     ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
//                     if (pcmd.UserCallback != IntPtr.Zero)
//                     {
//                         throw new NotImplementedException();
//                     }
//                     else
//                     {
//                         int windowHeight = (int)Graphics.GetViewport().height;

//                         GL.ActiveTexture(TextureUnit.Texture0);
//                         GL.BindTexture(TextureTarget.Texture2d, (int)pcmd.TextureId);
//                         CheckGLError("Texture");

//                         // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
//                         var clip = pcmd.ClipRect;
//                         GL.Scissor((int)clip.X, windowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
//                         CheckGLError("Scissor");

//                         if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
//                         {
//                             GL.DrawElementsBaseVertex(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(pcmd.IdxOffset * sizeof(ushort)), unchecked((int)pcmd.VtxOffset));
//                         }
//                         else
//                         {
//                             GL.DrawElements(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (int)pcmd.IdxOffset * sizeof(ushort));
//                         }
//                         CheckGLError("Draw");
//                     }
//                 }
//             }

//             GL.Disable(EnableCap.Blend);
//             GL.Disable(EnableCap.ScissorTest);

//             // Reset state
//             GL.BindTexture(TextureTarget.Texture2d, prevTexture2D);
//             GL.ActiveTexture((TextureUnit)prevActiveTexture);
//             GL.UseProgram(prevProgram);
//             GL.BindVertexArray(prevVAO);
//             GL.Scissor(prevScissorBox[0], prevScissorBox[1], prevScissorBox[2], prevScissorBox[3]);
//             GL.BindBuffer(BufferTargetARB.ArrayBuffer, prevArrayBuffer);
//             GL.BlendEquationSeparate((BlendEquationModeEXT)prevBlendEquationRgb, (BlendEquationModeEXT)prevBlendEquationAlpha);
//             GL.BlendFuncSeparate(
//                 (BlendingFactor)prevBlendFuncSrcRgb,
//                 (BlendingFactor)prevBlendFuncDstRgb,
//                 (BlendingFactor)prevBlendFuncSrcAlpha,
//                 (BlendingFactor)prevBlendFuncDstAlpha);
//             if (prevBlendEnabled) GL.Enable(EnableCap.Blend); else GL.Disable(EnableCap.Blend);
//             if (prevDepthTestEnabled) GL.Enable(EnableCap.DepthTest); else GL.Disable(EnableCap.DepthTest);
//             if (prevCullFaceEnabled) GL.Enable(EnableCap.CullFace); else GL.Disable(EnableCap.CullFace);
//             if (prevScissorTestEnabled) GL.Enable(EnableCap.ScissorTest); else GL.Disable(EnableCap.ScissorTest);
//         }

//         /// <summary>
//         /// Frees all graphics resources used by the renderer.
//         /// </summary>
//         public void Dispose()
//         {
//             GL.DeleteVertexArray(_vertexArray);
//             GL.DeleteBuffer(_vertexBuffer);
//             GL.DeleteBuffer(_indexBuffer);

//             GL.DeleteTexture(_fontTexture);
//             GL.DeleteProgram(_shader);
//         }

//         public static void LabelObject(ObjectIdentifier objLabelIdent, int glObject, string name)
//         {
//             if (KHRDebugAvailable)
//                 GL.ObjectLabel(objLabelIdent, (uint)glObject, name.Length, name);
//         }

//         private bool IsExtensionSupported(string name)
//         {
//             int n = GL.GetInteger(GetPName.NumExtensions);
//             for (uint i = 0; i < n; i++)
//             {
//                 string extension = GL.GetStringi(StringName.Extensions, i);
//                 if (extension == name) 
//                     return true;
//             }

//             return false;
//         }

//         private int CreateProgram(string name, string vertexSource, string fragmentSoruce)
//         {
//             int program = GL.CreateProgram();
//             LabelObject(ObjectIdentifier.Program, program, $"Program: {name}");

//             int vertex = CompileShader(name, ShaderType.VertexShader, vertexSource);
//             int fragment = CompileShader(name, ShaderType.FragmentShader, fragmentSoruce);

//             GL.AttachShader(program, vertex);
//             GL.AttachShader(program, fragment);

//             GL.LinkProgram(program);
//             int success = GL.GetProgrami(program, ProgramPropertyARB.LinkStatus);
//             if (success == 0)
//             {
//                 GL.GetProgramInfoLog(program, out string info);
//                 System.Diagnostics.Debug.WriteLine($"GL.LinkProgram had info log [{name}]:\n{info}");
//             }

//             GL.DetachShader(program, vertex);
//             GL.DetachShader(program, fragment);

//             GL.DeleteShader(vertex);
//             GL.DeleteShader(fragment);

//             return program;
//         }

//         private int CompileShader(string name, ShaderType type, string source)
//         {
//             int shader = GL.CreateShader(type);
//             LabelObject(ObjectIdentifier.Shader, shader, $"Shader: {name}");

//             GL.ShaderSource(shader, source);
//             GL.CompileShader(shader);

//             int success = GL.GetShaderi(shader, ShaderParameterName.CompileStatus);
//             if (success == 0)
//             {
//                 GL.GetShaderInfoLog(shader, out string info);
//                 System.Diagnostics.Debug.WriteLine($"GL.CompileShader for shader '{name}' [{type}] had info log:\n{info}");
//             }

//             return shader;
//         }

//         private void CheckGLError(string title)
//         {
//             ErrorCode error;
//             int i = 1;
//             while ((error = GL.GetError()) != ErrorCode.NoError)
//             {
//                 System.Diagnostics.Debug.Print($"{title} ({i++}): {error}");
//             }
//         }

//         internal void InvalidateDeviceObjects()
//         {
//             RecreateFontDeviceTexture();
//         }
//     }
// }
