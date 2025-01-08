namespace Gowtu
{
    public static class DiffuseShader
    {
        public static readonly string vertexSource = @"#version 330 core

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
}";

        public static readonly string fragmentSource = @"#version 330 core

uniform sampler2D uDiffuseTexture;
uniform vec4 uDiffuseColor;
uniform float uAmbientStrength;
uniform float uShininess;
uniform vec2 uUVScale;
uniform vec2 uUVOffset;

#define MAX_NUM_LIGHTS 32

struct LightInfo {
    int isActive;       //4
    int type;           //4
    float constant;     //4
    float linear;       //4
    float quadratic;    //4
    float strength;     //4
    float padding1;     //4
    float padding2;     //4
    vec4 position;      //16
    vec4 direction;     //16
    vec4 color;
    vec4 ambient;
    vec4 diffuse;
    vec4 specular;
};

layout(std140) uniform Lights {
    LightInfo lights[MAX_NUM_LIGHTS];
} uLights;

layout(std140) uniform Camera {
    mat4 view;
    mat4 projection;
    mat4 viewProjection;
    vec4 position;
} uCamera;

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

layout (std140) uniform Shadow
{
    int cascadeCount;
    float shadowBias;
    float farPlane;
    int enabled;
    mat4 lightSpaceMatrices[16];
    float cascadePlaneDistances[16];
} uShadow;

in vec3 oNormal;
in vec3 oFragPosition;
in vec2 oUV;

out vec4 FragColor;

vec4 gamma_correction(vec4 color) {
    return vec4(pow(vec3(color.xyz), vec3(1.0/2.2)), color.a);
}

vec3 calculate_lighting(vec3 fragPosition, vec3 cameraPosition, vec3 normal, vec3 texColor, vec3 diffuseColor, float ambientStrength, float shininess) {
    vec3 ambient = vec3(0.0);
    vec3 diffuse = vec3(0.0);
    vec3 specular = vec3(0.0);

    for(int i = 0; i < MAX_NUM_LIGHTS; i++) {
        if(uLights.lights[i].isActive > 0) {
            float attenuation = 1.0;
            
            vec3 lightDir = vec3(0.0);

            if(uLights.lights[i].type == 0)  { //Directional
                lightDir = normalize(uLights.lights[i].direction.xyz);
            } else { //Point
                lightDir = normalize(uLights.lights[i].position.xyz - fragPosition);
                float lightLinear = uLights.lights[i].linear;
                float lightConstant = uLights.lights[i].constant;
                float lightQuadratic = uLights.lights[i].quadratic;
                float distance  = length(uLights.lights[i].position.xyz - fragPosition);
                attenuation = 1.0 / (lightConstant + lightLinear * distance + lightQuadratic * (distance * distance));
            }

            // ambient
            ambient += uLights.lights[i].ambient.rgb * ambientStrength * texColor.rgb * attenuation;

            // diffuse
            float diff = max(dot(lightDir, normal), 1.0);
            //float diff = 1.0;
            diffuse += uLights.lights[i].diffuse.rgb * diff * uLights.lights[i].color.rgb * uLights.lights[i].strength * attenuation;

            // specular
            vec3 viewDir = normalize(cameraPosition - fragPosition);
            vec3 halfwayDir = normalize(lightDir + viewDir);  
            float spec = pow(max(dot(normal, halfwayDir), 0.0), shininess);
            specular += uLights.lights[i].specular.rgb * spec * uLights.lights[i].color.rgb * uLights.lights[i].strength * attenuation;
        }
    }

    vec3 lighting = (ambient * (diffuse + specular)) * texColor.rgb * diffuseColor.rgb;
    return lighting;
}

float calculate_fog(float density, float gradient, vec3 camPosition, vec3 fragPosition) {
    float fogDistance = length(camPosition - fragPosition);
    float d = (fogDistance * density) * (fogDistance * density);
    float fogVisibility = pow(2, -d);
    //float fogVisibility = exp(-pow(fogDistance * density, gradient));
    fogVisibility = clamp(fogVisibility, 0.0f, 1.0f);
    return fogVisibility;
}

void main() {
    vec4 texColor = texture(uDiffuseTexture, (oUV + uUVOffset) * uUVScale);
    vec3 normal = normalize(oNormal);
    vec3 lighting = calculate_lighting(oFragPosition, uCamera.position.xyz, normal, texColor.rgb, uDiffuseColor.rgb, uAmbientStrength, uShininess);

    if(uWorld.fogEnabled > 0) {
        float visibility = calculate_fog(uWorld.fogDensity, uWorld.fogGradient, uCamera.position.xyz, oFragPosition);
        lighting = mix(uWorld.fogColor.rgb, lighting, visibility);
    }

    FragColor = gamma_correction(vec4(lighting, texColor.a));
}";
    }
}