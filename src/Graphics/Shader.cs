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
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Gowtu
{
    public sealed class Shader
    {
        private int id;

        public int Id
        {
            get
            {
                return id;
            }
        }

        public Shader(string vertexSource, string fragmentSource)
        {
            int vertShader = Compile(vertexSource, ShaderType.VertexShader);
            int fragShader = Compile(fragmentSource, ShaderType.FragmentShader);

            if(vertShader > 0 && fragShader > 0)
            {
                id = GL.CreateProgram();
                
                GL.AttachShader(id, vertShader);
                GL.AttachShader(id, fragShader);
                GL.LinkProgram(id);

                if(GL.GetProgrami(id, ProgramPropertyARB.LinkStatus) == 0)
                {
                    GL.GetProgramInfoLog(id, out string info);
                    Console.WriteLine(info);
                }

                GL.DeleteShader(vertShader);
                GL.DeleteShader(fragShader);
            }
        }

        public Shader(string vertexSource, string fragmentSource, string geometrySource)
        {
            int vertShader = Compile(vertexSource, ShaderType.VertexShader);
            int fragShader = Compile(fragmentSource, ShaderType.FragmentShader);
            int geometryShader = Compile(geometrySource, ShaderType.GeometryShader);

            if(vertShader > 0 && fragShader > 0 && geometryShader > 0)
            {
                id = GL.CreateProgram();
                
                GL.AttachShader(id, vertShader);
                GL.AttachShader(id, fragShader);
                GL.AttachShader(id, geometryShader);
                GL.LinkProgram(id);

                if(GL.GetProgrami(id, ProgramPropertyARB.LinkStatus) == 0)
                {
                    GL.GetProgramInfoLog(id, out string info);
                    Console.WriteLine(info);
                }

                GL.DeleteShader(vertShader);
                GL.DeleteShader(fragShader);
                GL.DeleteShader(geometryShader);
            }
        }

        public void Use()
        {
            GL.UseProgram(id);
        }

        public void Delete()
        {
            if(id > 0)
            {
                GL.DeleteProgram(id);
                id = 0;
            }
        }

        public void SetFloat(string name, float value)
        {
            SetFloat(GL.GetUniformLocation(id, name), value);
        }

        public void SetFloat2(string name, Vector2 value)
        {
            SetFloat2(GL.GetUniformLocation(id, name), value.X, value.Y);
        }

        public void SetFloat2(string name, float x, float y)
        {
            SetFloat2(GL.GetUniformLocation(id, name), x, y);
        }

        public void SetFloat3(string name, Vector3 value)
        {
            SetFloat3(GL.GetUniformLocation(id, name), value);
        }

        public void SetFloat3(string name, Color value)
        {
            SetFloat3(GL.GetUniformLocation(id, name), value);
        }

        public void SetFloat3(string name, float x, float y, float z)
        {
            SetFloat3(GL.GetUniformLocation(id, name), x, y, z);
        }

        public void SetFloat4(string name, float x, float y, float z, float w)
        {
            SetFloat4(GL.GetUniformLocation(id, name), x, y, z, w);
        }

        public void SetFloat4(string name, Vector4 value)
        {
            SetFloat4(GL.GetUniformLocation(id, name), value);
        }

        public void SetFloat4(string name, Color value)
        {
            SetFloat4(GL.GetUniformLocation(id, name), value.r, value.g, value.b, value.a);
        }

        public void SetInt(string name, int value)
        {
            SetInt(GL.GetUniformLocation(id, name), value);
        }

        public void SetMat4(string name, Matrix4 value, bool transpose = false)
        {
            SetMat4(GL.GetUniformLocation(id, name), value, transpose);
        }

        public void SetMat3(string name, Matrix3 value, bool transpose = false)
        {
            SetMat3(GL.GetUniformLocation(id, name), value, transpose);
        }

        public void SetBool(string name, bool value)
        {
            int val = value == false ? 0 : 1;
            SetInt(GL.GetUniformLocation(id, name), val);
        }

        public void SetFloat(int location, float value)
        {
            GL.Uniform1f(location, value);
        }

        public void SetFloat2(int location, Vector2 value)
        {
            GL.Uniform2f(location, value.X, value.Y);
        }

        public void SetFloat2(int location, float x, float y)
        {
            GL.Uniform2f(location, x, y);
        }

        public void SetFloat3(int location, Vector3 value)
        {
            GL.Uniform3f(location, value.X, value.Y, value.Z);
        }

        public void SetFloat3(int location, Color value)
        {
            GL.Uniform3f(location, value.r, value.g, value.b);
        }

        public void SetFloat3(int location, float x, float y, float z)
        {
            GL.Uniform3f(location, x, y, z);
        }

        public void SetFloat4(int location, Vector4 value)
        {
            GL.Uniform4f(location, value.X, value.Y, value.Z, value.W);
        }

        public void SetFloat4(int location, Color value)
        {
            SetFloat4(location, value.r, value.g, value.b, value.a);
        }

        public void SetFloat4(int location, float x, float y, float z, float w)
        {
            GL.Uniform4f(location, x, y, z, w);
        }

        public void SetInt(int location, int value)
        {
            GL.Uniform1i(location, value);
        }

        public void SetMat4(int location, Matrix4 value, bool transpose = false)
        {
            GL.UniformMatrix4f(location, 1, transpose, value);
        }

        public void SetMat3(int location, Matrix3 value, bool transpose = false)
        {
            GL.UniformMatrix3f(location, 1, transpose, value);
        }

        public void SetBool(int location, bool value)
        {
            int val = value == false ? 0 : 1;
            GL.Uniform1i(location, val);
        }

        private static int Compile(string source, ShaderType type)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            
            if(GL.GetShaderi(shader, ShaderParameterName.CompileStatus) == 0)
            {
                GL.GetShaderInfoLog(shader, out string info);
                Console.WriteLine(type.ToString() + " " + info);
                return 0;
            }
            return shader;
        }
    }
}