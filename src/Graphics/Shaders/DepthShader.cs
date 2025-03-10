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

namespace Gowtu
{
    public static class DepthShader
    {
        public static readonly string vertexSource = @"#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 3) in mat4 aInstanceModel;

uniform mat4 uModel;
uniform int uHasInstanceData;

void main() {
    if(uHasInstanceData > 0)
        gl_Position = aInstanceModel * vec4(aPosition, 1.0);
    else
        gl_Position = uModel * vec4(aPosition, 1.0);
}";

        public static readonly string geometrySource = @"#version 420 core
layout(triangles, invocations = 5) in;
layout(triangle_strip, max_vertices = 3) out;

#include <Core>

void main() {          
    gl_Position = uShadow.lightSpaceMatrices[gl_InvocationID] * gl_in[0].gl_Position;
    gl_Layer = gl_InvocationID;
    EmitVertex();

    gl_Position = uShadow.lightSpaceMatrices[gl_InvocationID] * gl_in[1].gl_Position;
    gl_Layer = gl_InvocationID;
    EmitVertex();

    gl_Position = uShadow.lightSpaceMatrices[gl_InvocationID] * gl_in[2].gl_Position;
    gl_Layer = gl_InvocationID;
    EmitVertex();

	EndPrimitive();
}";

        public static readonly string fragmentSource = @"#version 330 core
void main() {
}";

        internal static Shader Create()
        {
            return new Shader(vertexSource, fragmentSource, geometrySource);
        }
    }
}