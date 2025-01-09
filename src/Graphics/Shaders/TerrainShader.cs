namespace Gowtu
{
    public static class TerrainShader
    {
        public static readonly string fragmentSource = @"#version 330 core

uniform vec4 uDiffuseColor;
uniform float uAmbientStrength;
uniform float uShininess;
uniform sampler2D uSplatMap;
uniform sampler2D uTexture1;
uniform sampler2D uTexture2;
uniform sampler2D uTexture3;
uniform sampler2D uTexture4;
uniform sampler2DArray uDepthMap;
uniform vec2 uUVScale1;
uniform vec2 uUVScale2;
uniform vec2 uUVScale3;
uniform vec2 uUVScale4;
uniform int uReceiveShadows;

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

float calculate_shadow(vec3 fragPosWorldSpace, mat4 view, vec3 normal, vec3 lightDirection) {
    if(uReceiveShadows < 1)
        return 0.0;

    if(uShadow.enabled < 1)
        return 0.0;

    // select cascade layer
    vec4 fragPosViewSpace = view * vec4(fragPosWorldSpace, 1.0);
    float depthValue = abs(fragPosViewSpace.z);

    int layer = -1;
    
    for (int i = 0; i < uShadow.cascadeCount; ++i) {
        if (depthValue < uShadow.cascadePlaneDistances[i]) {
            layer = i;
            break;
        }
    }
    
    if (layer == -1) {
        layer = uShadow.cascadeCount;
    }

    vec4 fragPosLightSpace = uShadow.lightSpaceMatrices[layer] * vec4(fragPosWorldSpace, 1.0);
    // perform perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    // transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;

    // get depth of current fragment from light's perspective
    float currentDepth = projCoords.z;

    // keep the shadow at 0.0 when outside the far_plane region of the light's frustum.
    if (currentDepth > 1.0) {
        return 0.0;
    }
    // calculate bias (based on depth map resolution and slope)
    normal = normalize(normal);
    float maxBias = uShadow.shadowBias; //default 0.005f;
    float bias = max(0.05 * (1.0 - dot(normal, lightDirection)), maxBias);
    const float biasModifier = 0.5f;

    if (layer == uShadow.cascadeCount) {
        bias *= 1 / (uShadow.farPlane * biasModifier);
    } else {
        bias *= 1 / (uShadow.cascadePlaneDistances[layer] * biasModifier);
    }

    // PCF
    float shadow = 0.0;
    vec2 texelSize = 1.0 / vec2(textureSize(uDepthMap, 0));

    for(int x = -1; x <= 1; ++x) {
        for(int y = -1; y <= 1; ++y) {
            float pcfDepth = texture(uDepthMap, vec3(projCoords.xy + vec2(x, y) * texelSize, layer)).r;
            shadow += (currentDepth - bias) > pcfDepth ? 1.0 : 0.0;        
        }    
    }
    
    shadow /= 9.0;
        
    return shadow;
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

    vec3 lightDirection = normalize(uLights.lights[0].direction.xyz);
    float shadow = calculate_shadow(oFragPosition, uCamera.view, normal, lightDirection);
    vec3 lighting = (ambient + (1.0 - shadow) * (diffuse + specular)) * texColor.rgb * diffuseColor.rgb;
    
    //vec3 lighting = (ambient * (diffuse + specular)) * texColor.rgb * diffuseColor.rgb;
    
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
    }
}