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
    internal static class ProceduralSkyboxShader
    {
        public static readonly string vertexSource = @"#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aUV;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;
uniform float uRayleighCoefficient = 0.0025;
uniform float uMieCoefficient = 0.0003;
uniform float uScatteringDirection = 0.9800;

out vec2 oUV;
out vec3 oFragPosition;
out float oBr;
out float oBm;
out float og;
out vec3 onitrogen;
out vec3 oKr;
out vec3 oKm;

void main() {
    oUV = aUV;
	oBr = uRayleighCoefficient;
	oBm = uMieCoefficient;
	og =  uScatteringDirection;
	onitrogen = vec3(0.650, 0.570, 0.475);
	oKr = oBr / pow(onitrogen, vec3(4.0));
	oKm = oBm / pow(onitrogen, vec3(0.84));

    oFragPosition = vec3(uModel * vec4(aPosition, 1.0));
	gl_Position = uProjection * mat4(mat3(uView)) * vec4(aPosition, 1.0);
}";

        public static readonly string fragmentSource = @"#version 330 core

uniform float uCloudSpeed = 1.0;
uniform float uCirrus = 0.4;
uniform float uCumulus = 0.8;
uniform vec3 uSunPosition;

layout(std140) uniform World {
    vec4 fogColor;      //don't use vec3 because the alignment causes issues
    float fogDensity;
    float fogGradient;
    int fogEnabled;
    float time;
    float padding1;
    float padding2;
    float padding3;
    float padding4;
} uWorld;

in vec2 oUV;
in vec3 oFragPosition;

in float oBr;
in float oBm;
in float og;
in vec3 onitrogen;
in vec3 oKr;
in vec3 oKm;

out vec4 FragColor;

float hash(float n) {
	return fract(sin(n) * 43758.5453123);
}

float noise(vec3 x) {
	vec3 f = fract(x);
	float n = dot(floor(x), vec3(1.0, 157.0, 113.0));
	return mix(mix(mix(hash(n +   0.0), hash(n +   1.0), f.x),
					mix(hash(n + 157.0), hash(n + 158.0), f.x), f.y),
					mix(mix(hash(n + 113.0), hash(n + 114.0), f.x),
					mix(hash(n + 270.0), hash(n + 271.0), f.x), f.y), f.z);
}

const mat3 m = mat3(0.0, 1.60,  1.20, -1.6, 0.72, -0.96, -1.2, -0.96, 1.28);

float fbm(vec3 p) {
	float f = 0.0;
	f += noise(p) / 2; p = m * p * 1.1;
	f += noise(p) / 4; p = m * p * 1.2;
	f += noise(p) / 6; p = m * p * 1.3;
	f += noise(p) / 12; p = m * p * 1.4;
	f += noise(p) / 24;
	return f;
}

vec4 gamma_correction(vec4 color) {
    return vec4(pow(vec3(color.xyz), vec3(1.0/2.2)), color.a);
}

vec4 calculate_sky_color() {
	vec3 pos = oFragPosition;

	float Br = oBr;
	float Bm = oBm;
	float g = og;
	vec3 nitrogen = onitrogen;
	vec3 Kr = oKr;
	vec3 Km = oKm;

	vec4 color = vec4(0.0, 0.0, 0.0, 1.0);

	// Atmosphere Scattering
	float mu = dot(normalize(pos), normalize(uSunPosition));
	float rayleigh = 3.0 / (8.0 * 3.14) * (1.0 + mu * mu);
	vec3 mie = (Kr + Km * (1.0 - g * g) / (2.0 + g * g) / pow(1.0 + g * g - 2.0 * g * mu, 1.5)) / (Br + Bm);

	vec3 day_extinction = exp(-exp(-((pos.y + uSunPosition.y * 4.0) * (exp(-pos.y * 16.0) + 0.1) / 80.0) / Br) * (exp(-pos.y * 16.0) + 0.1) * Kr / Br) * exp(-pos.y * exp(-pos.y * 8.0 ) * 4.0) * exp(-pos.y * 2.0) * 4.0;
	vec3 night_extinction = vec3(1.0 - exp(uSunPosition.y)) * 0.2;
	vec3 extinction = mix(day_extinction, night_extinction, -uSunPosition.y * 0.2 + 0.5);	
	color.rgb = rayleigh * mie * extinction;

	float time = uWorld.time * uCloudSpeed;

	// Cirrus Clouds
	float density = smoothstep(1.0 - uCirrus, 1.0, fbm(pos.xyz / pos.y * 2.0 + time * 0.05)) * 0.3;
	color.rgb = mix(color.rgb, extinction * 4.0, density * max(pos.y, 0.0));

	// Cumulus Clouds
	for (int i = 0; i < 3; i++) {
		float density = smoothstep(1.0 - uCumulus, 1.0, fbm((0.7 + float(i) * 0.01) * pos.xyz / pos.y + time * 0.3));
		color.rgb = mix(color.rgb, extinction * density * 5.0, min(density, 1.0) * max(pos.y, 0.0));
	}

	// Dithering Noise
	color.rgb += noise(pos * 1000) * 0.01;

	color = gamma_correction(color);
	return color;
}

void main() {
	FragColor = calculate_sky_color();
}";

        internal static Shader Create()
        {
            return new Shader(vertexSource, fragmentSource);
        }
    }
}