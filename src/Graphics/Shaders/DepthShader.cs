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

        public static readonly string geometrySource = @"#version 330 core
//layout(triangles, invocations = 5) in;
layout(triangles) in;
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

void main() {          
	for(int id = 0; id < INVOCATIONS; id++) {
		for (int i = 0; i < 3; i++) {
			gl_Position = uShadow.lightSpaceMatrices[id] * gl_in[i].gl_Position;
			gl_Layer = id;
			EmitVertex();
		}
		EndPrimitive();
	}
}

//void main() {          
//	for (int i = 0; i < 3; ++i) {
//		gl_Position = uShadow.lightSpaceMatrices[gl_InvocationID] * gl_in[i].gl_Position;
//		gl_Layer = gl_InvocationID;
//		EmitVertex();
//	}
//	EndPrimitive();
//}";

        public static readonly string fragmentSource = @"#version 330 core
void main() {

}";
    }
}