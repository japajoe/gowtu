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
    public static class WaterShader
    {
        public static readonly string vertexSource = @"#version 330 core
#include <Core>

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aUV;

uniform mat4 uModel;
uniform mat3 uModelInverted;
uniform mat4 uMVP;
uniform vec2 uDirection = vec2(0.5, 0.3);
uniform float uSpeed = 1.0;
uniform float uSteepness = 0.1;
uniform float uAmplitude = 1.0;
uniform float uWaveLength = 5.0;

out VS_OUT {
    vec3 normal;
    vec2 uv;
    vec3 fragPosition;
    mat4 mvp;
} vs_out;

const vec3 origin = vec3(0, 0, 0);

vec3 gerstner(vec3 vertex, vec2 direction, float time, float speed, float steepness, float amplitude, float wavelength){
	float displaced_x = vertex.x + (steepness/wavelength) * direction.x * cos(wavelength * dot(direction, vertex.xz) + speed * time);
	float displaced_z = vertex.z + (steepness/wavelength) * direction.y * cos(wavelength * dot(direction, vertex.xz) + speed * time);
	float displaced_y = vertex.y + amplitude * sin(wavelength * dot(direction, vertex.xz) + speed * time);
	return vec3(displaced_x, displaced_y, displaced_z);
}

void main() {
    vec3 pos = gerstner(aPosition, normalize(uDirection), uWorld.time, uSpeed, uSteepness, uAmplitude, uWaveLength);
    
    vs_out.normal = aNormal;
    vs_out.uv = aUV;
    vs_out.fragPosition = vec3(uModel * vec4(pos, 1.0));
    vs_out.mvp = uMVP;

    gl_Position = vec4(pos, 1.0);
}";

        public static readonly string geometrySource = @"#version 330 core
#include <Core>

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

in VS_OUT {
    vec3 normal;
    vec2 uv;
    vec3 fragPosition;
    mat4 mvp;
} gs_in[];

out GS_OUT {
    vec3 normal;
    vec2 uv;
    vec3 fragPosition;
} gs_out;

void main() {
    vec3 pos0 = gl_in[0].gl_Position.xyz;
    vec3 pos1 = gl_in[1].gl_Position.xyz;
    vec3 pos2 = gl_in[2].gl_Position.xyz;

    vec3 edge1 = pos1 - pos0;
    vec3 edge2 = pos2 - pos0;
    vec3 normal = normalize(cross(edge1, edge2));

    for(int i = 0; i < 3; i++) {
        gs_out.normal = normal;
        gs_out.uv           = gs_in[i].uv;
        gs_out.fragPosition = gs_in[i].fragPosition;
        gl_Position = gs_in[i].mvp * vec4(gl_in[i].gl_Position.xyz, 1.0);
        EmitVertex();
    }

    EndPrimitive();
}";

        public static readonly string fragmentSource = @"#version 330 core
#include <Core>

uniform sampler2D uDiffuseTexture;
uniform vec4 uDiffuseColor;
uniform float uAmbientStrength;
uniform float uShininess;
uniform vec2 uUVScale;
uniform vec2 uUVOffset;

in GS_OUT {
    vec3 normal;
    vec2 uv;
    vec3 fragPosition;
} fs_in;

out vec4 FragColor;

void main() {
    vec4 texColor = texture(uDiffuseTexture, (fs_in.uv + uUVOffset) * uUVScale);
    vec3 normal = normalize(fs_in.normal);
    vec3 lighting = calculate_lighting(fs_in.fragPosition, uCamera.position.xyz, normal, texColor.rgb, uDiffuseColor.rgb, uAmbientStrength, uShininess);

    if(uWorld.fogEnabled > 0) {
        float visibility = calculate_fog(uWorld.fogDensity, uWorld.fogGradient, uCamera.position.xyz, fs_in.fragPosition);
        lighting = mix(uWorld.fogColor.rgb, lighting, visibility);
    }

    FragColor = gamma_correction(vec4(lighting, texColor.a * uDiffuseColor.a));
}";

        internal static Shader Create()
        {
            return new Shader(vertexSource, fragmentSource, geometrySource);
        }
    }
}