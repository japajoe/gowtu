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
    public static class TerrainShader
    {
        public static readonly string fragmentSource = @"#version 330 core
#include <Core>

uniform vec4 uDiffuseColor;
uniform float uAmbientStrength;
uniform float uShininess;
uniform sampler2D uSplatMap;
uniform sampler2D uTexture1;
uniform sampler2D uTexture2;
uniform sampler2D uTexture3;
uniform sampler2D uTexture4;
uniform vec2 uUVScale1;
uniform vec2 uUVScale2;
uniform vec2 uUVScale3;
uniform vec2 uUVScale4;

in vec3 oNormal;
in vec3 oFragPosition;
in vec2 oUV;

out vec4 FragColor;

vec4 calculate_texture_color() {
    vec4 blendMapColor = texture(uSplatMap, oUV);
    float backgroundTextureAmount = 1.0 - (blendMapColor.r + blendMapColor.g + blendMapColor.b);

    vec4 backgroundTextureColor = texture(uTexture1, oUV * uUVScale1) * backgroundTextureAmount;
    vec4 rTextureColor = texture(uTexture2, oUV * uUVScale2) * blendMapColor.r;
    vec4 gTextureColor = texture(uTexture3, oUV * uUVScale3) * blendMapColor.g;
    vec4 bTextureColor = texture(uTexture4, oUV * uUVScale4) * blendMapColor.b;
    vec4 result = backgroundTextureColor + rTextureColor + gTextureColor + bTextureColor;
    return result;
}

void main() {
    vec4 texColor = calculate_texture_color();
    vec3 normal = normalize(oNormal);
    vec3 lighting = calculate_lighting(oFragPosition, uCamera.position.xyz, normal, texColor.rgb, uDiffuseColor.rgb, uAmbientStrength, uShininess);

    if(uWorld.fogEnabled > 0) {
        float visibility = calculate_fog(uWorld.fogDensity, uWorld.fogGradient, uCamera.position.xyz, oFragPosition);
        lighting = mix(uWorld.fogColor.rgb, lighting, visibility);
    }

    FragColor = gamma_correction(vec4(lighting, texColor.a));
}";

        internal static Shader Create()
        {
            return new Shader(DiffuseShader.vertexSource, fragmentSource);
        }
    }
}