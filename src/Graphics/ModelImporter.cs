using Assimp;
using OpenTK.Mathematics;
using System;
using System.IO;

namespace Gowtu
{
    public enum ModelImporterFlags : uint
    {
        None = 0,
        CalculateTangentSpace = 1,
        JoinIdenticalVertices = 2,
        MakeLeftHanded = 4,
        Triangulate = 8,
        RemoveComponent = 16,
        GenerateNormals = 32,
        GenerateSmoothNormals = 64,
        SplitLargeMeshes = 128,
        PreTransformVertices = 256,
        LimitBoneWeights = 512,
        ValidateDataStructure = 1024,
        ImproveCacheLocality = 2048,
        RemoveRedundantMaterials = 4096,
        FixInFacingNormals = 8192,
        SortByPrimitiveType = 32768,
        FindDegenerates = 65536,
        FindInvalidData = 131072,
        GenerateUVCoords = 262144,
        TransformUVCoords = 524288,
        FindInstances = 1048576,
        OptimizeMeshes = 2097152,
        OptimizeGraph = 4194304,
        FlipUVs = 8388608,
        FlipWindingOrder = 16777216,
        SplitByBoneCount = 33554432,
        Debone = 67108864,
        GlobalScale = 134217728,
        EmbedTextures = 268435456,
        ForceGenerateNormals = 536870912,
        DropNormals = 1073741824,
        GenerateBoundingBoxes = 2147483648
    }

    public static class ModelImporter
    {
        public static GameObject LoadFromFile(string filepath, ModelImporterFlags flags = ModelImporterFlags.None, Vector3 scale = default(Vector3))
        {
            if(!File.Exists(filepath))
            {
                Debug.Log("Error loading model: file does not exist: " + filepath);
                return null;
            }

            if(scale == Vector3.Zero)
                scale = Vector3.One;

            AssimpContext importer = new AssimpContext();

            PostProcessSteps postProcessFlags = (PostProcessSteps)flags;
            var scene = importer.ImportFile(filepath, postProcessFlags);

            if(scene == null)
            {
                Debug.Log("Error loading model");
                return null;
            }

            if(scene.SceneFlags.HasFlag(SceneFlags.Incomplete))
            {
                Debug.Log("Error loading model: model is incomplete");
                return null;
            }

            GameObject model = new GameObject();

            ProcessNode(model, scene.RootNode, scene, scale);

            return model;
        }

        public static unsafe GameObject LoadFromMemory(ReadOnlySpan<byte> data, ModelImporterFlags flags = ModelImporterFlags.None, Vector3 scale = default(Vector3))
        {
            if(data == null)
            {
                Debug.Log("Error loading model: data is null");
                return null;
            }

            if(scale == Vector3.Zero)
                scale = Vector3.One;

            fixed(byte* pData = &data[0])
            {
                using(UnmanagedMemoryStream stream = new UnmanagedMemoryStream(pData, data.Length))
                {
                    AssimpContext importer = new AssimpContext();

                    PostProcessSteps postProcessFlags = (PostProcessSteps)flags;
                    var scene = importer.ImportFileFromStream(stream, postProcessFlags);

                    if(scene == null)
                    {
                        Debug.Log("Error loading model");
                        return null;
                    }

                    if(scene.SceneFlags.HasFlag(SceneFlags.Incomplete))
                    {
                        Debug.Log("Error loading model: model is incomplete");
                        return null;
                    }

                    GameObject model = new GameObject();

                    ProcessNode(model, scene.RootNode, scene, scale);

                    return model;
                }
            }
        }

        private static void ProcessNode(GameObject parent, Node node, Scene scene, Vector3 scale)
        {
            var transformation = ToMatrix4(node.Transform);
            parent.transform.position = transformation.ExtractTranslation() * scale;
            parent.transform.rotation = transformation.ExtractRotation();
            parent.transform.scale = transformation.ExtractScale();

            MeshRenderer renderer = null;
            
            if(node.MeshCount > 0)
            {
                parent.name = node.Name;
                renderer = parent.AddComponent<MeshRenderer>();
            }

            // Process all meshes in the current node
            for (int i = 0; i < node.MeshCount; i++) 
            {
                var aMesh = scene.Meshes[node.MeshIndices[i]];

                Vertex[] vertices = new Vertex[aMesh.VertexCount];
                
                if(aMesh.HasTextureCoords(0))
                {
                    for(int j = 0; j < aMesh.VertexCount; j++)
                    {
                        var pos = aMesh.Vertices[j];
                        var nrm = aMesh.Normals[j];
                        var uv = aMesh.TextureCoordinateChannels[0][j];

                        vertices[j].position = new Vector3(pos.X, pos.Y, pos.Z) * scale;
                        vertices[j].normal = new Vector3(nrm.X, nrm.Y, nrm.Z);
                        vertices[j].uv = new Vector2(uv.X, uv.Y);
                    }
                }
                else
                {
                    for(int j = 0; j < aMesh.VertexCount; j++)
                    {
                        var pos = aMesh.Vertices[j];
                        var nrm = aMesh.Normals[j];

                        vertices[j].position = new Vector3(pos.X, pos.Y, pos.Z) * scale;
                        vertices[j].normal = new Vector3(nrm.X, nrm.Y, nrm.Z);
                        vertices[j].uv = new Vector2(0, 0);
                    }
                }

                uint[] indices = new uint[aMesh.FaceCount * 3];
                int vIndex = 0;

                for(int j = 0; j < aMesh.FaceCount; j++)
                {
                    indices[vIndex+0] = (uint)aMesh.Faces[j].Indices[0];
                    indices[vIndex+1] = (uint)aMesh.Faces[j].Indices[1];
                    indices[vIndex+2] = (uint)aMesh.Faces[j].Indices[2];
                    vIndex += 3;
                }

                // int materialIndex = aMesh.MaterialIndex;
                // var aMaterial = scene.Materials[materialIndex];

                var mesh = new Mesh(vertices, indices, false);
                mesh.Generate();

                var material = new DiffuseMaterial();
                
                renderer.Add(mesh, material);
            }

            // // Process all child nodes
            for (int i = 0; i < node.ChildCount; i++) 
            {
                var child = new GameObject(node.Children[i].Name);
                child.transform.SetParent(parent.transform);
                ProcessNode(child, node.Children[i], scene, scale);
            }
        }

        private static OpenTK.Mathematics.Matrix4 ToMatrix4(Matrix4x4 m)
        {
            m.Transpose();
            var mat = new OpenTK.Mathematics.Matrix4();

            mat.M11 = m.A1;
            mat.M12 = m.A2;
            mat.M13 = m.A3;
            mat.M14 = m.A4;

            mat.M21 = m.B1;
            mat.M22 = m.B2;
            mat.M23 = m.B3;
            mat.M24 = m.B4;

            mat.M31 = m.C1;
            mat.M32 = m.C2;
            mat.M33 = m.C3;
            mat.M34 = m.C4;

            mat.M41 = m.D1;
            mat.M42 = m.D2;
            mat.M43 = m.D3;
            mat.M44 = m.D4;            

            return mat;
        }

        // private static OpenTK.Mathematics.Matrix4 ToMatrix4(System.Numerics.Matrix4x4 m)
        // {
        //     var mat = new OpenTK.Mathematics.Matrix4();
        //     m = System.Numerics.Matrix4x4.Transpose(m);

        //     mat.M11 = m.M11;
        //     mat.M12 = m.M12;
        //     mat.M13 = m.M13;
        //     mat.M14 = m.M14;

        //     mat.M21 = m.M21;
        //     mat.M22 = m.M22;
        //     mat.M23 = m.M23;
        //     mat.M24 = m.M24;

        //     mat.M31 = m.M31;
        //     mat.M32 = m.M32;
        //     mat.M33 = m.M33;
        //     mat.M34 = m.M34;

        //     mat.M41 = m.M41;
        //     mat.M42 = m.M42;
        //     mat.M43 = m.M43;
        //     mat.M44 = m.M44;            

        //     return mat;
        // }
    }
}