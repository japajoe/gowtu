#version 330 core

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
}