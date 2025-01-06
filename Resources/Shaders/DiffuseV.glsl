#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aUV;

uniform mat4 uModel;
uniform mat3 uModelInverted;
uniform mat4 uMVP;

out vec3 oNormal;
out vec3 oFragPosition;
out vec2 oUV;

void main() {
    gl_Position = uMVP * vec4(aPosition, 1.0);
    oNormal = normalize(uModelInverted * aNormal);
    oFragPosition = vec3(uModel * vec4(aPosition, 1.0));
    oUV = aUV;
}