// MIT License

// Copyright (c) 2019 Silvio Henrique Ferreira (https://github.com/shff/opengl_sky)

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
    internal static class SkyboxShader
    {
        public static readonly string vertexSource = @"#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aUV;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out vec3 oUV;
out vec3 oFragPosition;
out float oBr;

void main() {
    //oUV = aUV;
	oUV = aPosition;
    oFragPosition = vec3(uModel * vec4(aPosition, 1.0));
	gl_Position = uProjection * mat4(mat3(uView)) * vec4(aPosition, 1.0);
}";

        public static readonly string fragmentSource = @"#version 330 core
#include <Core>

uniform samplerCube uTexture;
uniform vec4 uDiffuseColor;

in vec3 oUV;
in vec3 oFragPosition;
out vec4 FragColor;

void main() {
    vec3 uv = oUV;
    uv.y *= -1.0;
    vec4 result = texture(uTexture, uv) * uDiffuseColor;
	result = gamma_correction(result);

	FragColor = result;
}";

        internal static Shader Create()
        {
            return new Shader(vertexSource, fragmentSource);
        }
    }
}