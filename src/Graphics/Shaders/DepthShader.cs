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

uniform mat4 uModel;

void main() {
    gl_Position = uModel * vec4(aPosition, 1.0);
}";

        public static readonly string geometrySource = @"#version 420 core
layout(triangles, invocations = 5) in;
//layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

//Test to see if this works on 330 core
#define INVOCATIONS 5

layout (std140) uniform Shadow
{
    int cascadeCount;
    float shadowBias;
    float farPlane;
    int enabled;
    mat4 lightSpaceMatrices[16];
    float cascadePlaneDistances[16];
} uShadow;

// void main() {          
// 	for(int id = 0; id < INVOCATIONS; id++) {
// 		for (int i = 0; i < 3; i++) {
// 			gl_Position = uShadow.lightSpaceMatrices[id] * gl_in[i].gl_Position;
// 			gl_Layer = id;
// 			EmitVertex();
// 		}
// 		EndPrimitive();
// 	}
// }

void main() {          
	for (int i = 0; i < 3; ++i) {
		gl_Position = uShadow.lightSpaceMatrices[gl_InvocationID] * gl_in[i].gl_Position;
		gl_Layer = gl_InvocationID;
		EmitVertex();
	}
	EndPrimitive();
}";

        public static readonly string fragmentSource = @"#version 330 core
void main() {

}";
    }
}