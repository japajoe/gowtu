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
    public static class ParticleShader
    {
        public static readonly string vertexSource = @"#version 330 core
#include <Core>

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aUV;
layout (location = 3) in mat4 aInstanceModel;
layout (location = 7) in vec4 aInstanceColor;

out vec3 oFragPosition;
out vec3 oNormal;
out vec2 oUV;
out vec4 oColor;

void main() {
    oFragPosition = vec3(aInstanceModel * vec4(aPosition, 1.0)); // Vertex in world space
    oNormal = inverse(transpose(mat3(aInstanceModel))) * aNormal;
    oUV = aUV;
    oColor = aInstanceColor;
    gl_Position = uCamera.viewProjection * aInstanceModel * vec4(aPosition, 1.0);
}";

        public static readonly string fragmentSource = @"#version 330 core
#include <Core>

uniform sampler2D uDiffuseTexture;
uniform vec2 uUVOffset;
uniform vec2 uUVScale;
uniform float uAlphaCutOff;

in vec3 oFragPosition;
in vec3 oNormal;
in vec2 oUV;
in vec4 oColor;

out vec4 FragColor;

void main() {
    vec4 texColor = oColor * texture2D(uDiffuseTexture, (oUV + uUVOffset) * uUVScale);

    if(texColor.a < uAlphaCutOff)
        discard;

    if(uWorld.fogEnabled > 0) {
        float visibility = calculate_fog(uWorld.fogDensity, uWorld.fogGradient, uCamera.position.xyz, oFragPosition);
        texColor.rgb = mix(uWorld.fogColor.rgb, texColor.rgb, visibility);
    }

    FragColor = gamma_correction(texColor);
}";

        internal static Shader Create()
        {
            return new Shader(vertexSource, fragmentSource);
        }
    }
}