namespace Gowtu
{
    public static class Constants
    {
        public static uint GetBindingIndex(ConstantBindingIndex b)
        {
            switch(b)
            {
                case ConstantBindingIndex.UniformBufferCamera:
                    return Camera.UBO_BINDING_INDEX;
                case ConstantBindingIndex.UniformBufferLights:
                    return Light.UBO_BINDING_INDEX;
                case ConstantBindingIndex.UniformBufferShadow:
                    return Shadow.UBO_BINDING_INDEX;
                case ConstantBindingIndex.UniformBufferWorld:
                    return World.UBO_BINDING_INDEX;
                default:
                    throw new System.ArgumentException("Unknown ConstantBindingIndex");
            }
        }

        public static string GetString(ConstantString n)
        {
            switch(n)
            {
                case ConstantString.FontDefault:
                    return "Default";
                case ConstantString.UniformBufferCamera:
                    return Camera.UBO_NAME;
                case ConstantString.UniformBufferLights:
                    return Light.UBO_NAME;
                case ConstantString.UniformBufferShadow:
                    return Shadow.UBO_NAME;
                case ConstantString.UniformBufferWorld:
                    return World.UBO_NAME;
                case ConstantString.MeshCapsule:
                    return "Capsule";
                case ConstantString.MeshCube:
                    return "Cube";
                case ConstantString.MeshPlane:
                    return "Plane";
                case ConstantString.MeshQuad:
                    return "Quad";
                case ConstantString.MeshSphere:
                    return "Sphere";
                case ConstantString.MeshSkybox:
                    return "Skybox";
                case ConstantString.ShaderDepth:
                    return "Depth";
                case ConstantString.ShaderDiffuse:
                    return "Diffuse";
                case ConstantString.ShaderDiffuseInstanced:
                    return "DiffuseInstanced";
                case ConstantString.ShaderParticle:
                    return "Particle";
                case ConstantString.ShaderProceduralSkybox:
                    return "ProceduralSkybox";
                case ConstantString.ShaderSkybox:
                    return "Skybox";
                case ConstantString.ShaderWater:
                    return "Water";
                case ConstantString.ShaderTerrain:
                    return "Terrain";
                case ConstantString.TextureDefault:
                    return "Default";
                case ConstantString.TextureDefaultCubeMap:
                    return "DefaultCubeMap";
                case ConstantString.TextureDepth:
                    return "Depth";
                default:
                    return "Unknown";
            }
        }
    }

    public enum ConstantBindingIndex
    {
        UniformBufferCamera,
        UniformBufferLights,
        UniformBufferShadow,
        UniformBufferWorld
    }

    public enum ConstantString
    {
        FontDefault,
        MeshCapsule,
        MeshCube,
        MeshPlane,
        MeshQuad,
        MeshSphere,
        MeshSkybox,
        ShaderDepth,
        ShaderDiffuse,
        ShaderDiffuseInstanced,
        ShaderParticle,
        ShaderProceduralSkybox,
        ShaderSkybox,
        ShaderTerrain,
        ShaderWater,
        TextureDefault,
        TextureDefaultCubeMap,
        TextureDepth,
        UniformBufferCamera,
        UniformBufferLights,
        UniformBufferShadow,
        UniformBufferWorld
    }
}