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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Gowtu
{
    public sealed class ParticleSystem : Renderer
    {
        private ParticleSpace space;
        private ParticleProperties properties;
        private int numParticles;
        private int activeParticles;
        private int poolIndex;
        private int emitAmount;
        private List<Mesh> meshes;
        private List<Particle> particles;
        private List<ParticleInstanceData> particleData;
        private ParticleMaterial material;
        private VertexArrayObject VAO;
        private VertexBufferObject VBO;
        private VertexBufferObject instanceVBO;
        private ElementBufferObject EBO;
        private Mesh pMesh;
        private ParticleType particleType;

        public int ActiveParticles
        {
            get { return activeParticles; }
        }

        public ParticleSpace Space
        {
            get { return space; }
            set { space = value; }
        }

        public ParticleProperties Properties
        {
            get { return properties; }
            set { properties = value; }
        }

        public ParticleSystem() : base()
        {
            this.castShadows = false;
            this.receiveShadows = false;
            this.particleType = ParticleType.Quad;

            space = ParticleSpace.Local;
            properties = new ParticleProperties();
            
            const int maxParticles = 1000;
            numParticles = maxParticles;
            activeParticles = 0;
            poolIndex = numParticles - 1;

            particleData = new List<ParticleInstanceData>(new ParticleInstanceData[numParticles]);
            particles = new List<Particle>(new Particle[numParticles]);
            meshes = new List<Mesh>();

            for(int i = 0; i < particleData.Count; i++)
            {
                particleData[i] = new ParticleInstanceData();
                particles[i] = new Particle();
            }

            material = new ParticleMaterial();
            emitAmount = 1;

            VAO = new VertexArrayObject();
            VBO = new VertexBufferObject();
            instanceVBO = new VertexBufferObject();
            EBO = new ElementBufferObject();

            meshes.Add(Resources.FindMesh(Constants.GetString(ConstantString.MeshCube)));
            meshes.Add(Resources.FindMesh(Constants.GetString(ConstantString.MeshPlane)));
            meshes.Add(Resources.FindMesh(Constants.GetString(ConstantString.MeshQuad)));
            meshes.Add(Resources.FindMesh(Constants.GetString(ConstantString.MeshSphere)));

            pMesh = meshes[2];
        }

        public ParticleMaterial GetMaterial()
        {
            return material;
        }

        internal override void OnInitializeComponent()
        {
            Initialize();
            base.OnInitializeComponent();
        }

        internal override void OnDestroyComponent()
        {
            base.OnDestroyComponent();

            EBO.Delete();
            VBO.Delete();
            VAO.Delete();
            instanceVBO.Delete();
        }

        private void Initialize()
        {
            var vertices = pMesh.Vertices;
            var indices = pMesh.Indices;

            if(VAO.Id == 0 && VBO.Id == 0 && instanceVBO.Id == 0 && EBO.Id == 0)
            {
                VAO.Generate();
                VBO.Generate();
                instanceVBO.Generate();
                EBO.Generate();
                
                VAO.Bind();

                VBO.Bind();
                VBO.BufferData<Vertex>(vertices, BufferUsageARB.StaticDraw);

                VAO.EnableVertexAttribArray(0);
                VAO.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), Marshal.OffsetOf<Vertex>("position"));
                
                VAO.EnableVertexAttribArray(1);
                VAO.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), Marshal.OffsetOf<Vertex>("normal"));
                
                VAO.EnableVertexAttribArray(2);
                VAO.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), Marshal.OffsetOf<Vertex>("uv"));

                instanceVBO.Bind();
                instanceVBO.BufferData(particleData, BufferUsageARB.DynamicDraw);
                
                VAO.EnableVertexAttribArray(3);
                VAO.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<ParticleInstanceData>(), new IntPtr(0 * Marshal.SizeOf<Vector4>()));
                
                VAO.EnableVertexAttribArray(4);
                VAO.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<ParticleInstanceData>(), new IntPtr(1 * Marshal.SizeOf<Vector4>()));
                
                VAO.EnableVertexAttribArray(5);
                VAO.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<ParticleInstanceData>(), new IntPtr(2 * Marshal.SizeOf<Vector4>()));
                
                VAO.EnableVertexAttribArray(6);
                VAO.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<ParticleInstanceData>(), new IntPtr(3 * Marshal.SizeOf<Vector4>()));
                
                VAO.EnableVertexAttribArray(7);
                VAO.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<ParticleInstanceData>(), new IntPtr(4 * Marshal.SizeOf<Color>()));

                VAO.VertexAttribDivisor(3, 1);
                VAO.VertexAttribDivisor(4, 1);
                VAO.VertexAttribDivisor(5, 1);
                VAO.VertexAttribDivisor(6, 1);
                VAO.VertexAttribDivisor(7, 1);

                EBO.Bind();
                EBO.BufferData<uint>(indices, BufferUsageARB.StaticDraw);

                VAO.Unbind();
                VBO.Unbind();
                instanceVBO.Unbind();
                EBO.Unbind();
            }
            else
            {
                VBO.Bind();
                VBO.BufferData<Vertex>(vertices, BufferUsageARB.StaticDraw);
                EBO.Bind();
                EBO.BufferData<uint>(indices, BufferUsageARB.StaticDraw);
                VBO.Unbind();
            }
        }

        private void Update()
        {
            activeParticles = 0;

            Matrix4 parentMatrix = space == ParticleSpace.Local ? transform.GetModelMatrix() : Matrix4.Identity;
            Camera camera = Camera.mainCamera;
            Matrix4 viewMatrix = camera.GetViewMatrix();

            for(int i = 0; i < particles.Count; i++)
            {
                if (particles[i].lifeRemaining <= 0.0f)
                {
                    particles[i].active = false;                    
                    continue;
                }

                float sizeMin = Math.Min(particles[i].sizeBegin, particles[i].sizeEnd);
                float sizeMax = Math.Max(particles[i].sizeBegin, particles[i].sizeEnd);
                float sizeRange = sizeMax - sizeMin;
                float timeProgress = particles[i].lifeTime - particles[i].lifeRemaining;
                float timePercentage = InverseLerp(0, particles[i].lifeTime, timeProgress);
                
                bool shrinking = particles[i].sizeEnd < particles[i].sizeBegin;
                float size = 0;

                if(shrinking)
                    size = particles[i].sizeBegin - (timePercentage * sizeRange);
                else
                    size = particles[i].sizeBegin + (timePercentage * sizeRange);

                if((particles[i].sizeBegin - particles[i].sizeEnd) == 0)
                    size = particles[i].sizeBegin;

                Color color = Color.Lerp(particles[i].colorBegin, particles[i].colorEnd, timePercentage);

                particles[i].lifeRemaining -= Time.DeltaTime;
                particles[i].position += particles[i].velocity * Time.DeltaTime;

                float rotation = particles[i].rotationSpeed * Time.DeltaTime;
                particles[i].rotation += rotation;
                particles[i].scale = size;
                
                Matrix4 matrix = particles[i].GetMatrix(viewMatrix);/// * parentMatrix;

                var data = particleData[activeParticles];
                data.matrix = matrix;
                data.color = color;
                particleData[activeParticles] = data;
                activeParticles++;
            }

            if(activeParticles > 0)
            {
                instanceVBO.Bind();
                instanceVBO.BufferSubData(particleData, 0);
                instanceVBO.Unbind();
            }
        }

        private void Emit(ParticleProperties particleProps)
        {
            Particle particle = particles[poolIndex];
            particle.active = true;            

            if(space == ParticleSpace.Local)
                particle.position = particleProps.position;
            else
                particle.position = transform.position + particleProps.position;

            particle.position.X += particleProps.positionVariation.X * Gowtu.Random.Range(-1.0f, 1.0f);
            particle.position.Y += particleProps.positionVariation.Y * Gowtu.Random.Range(-1.0f, 1.0f);
            particle.position.Z += particleProps.positionVariation.Z * Gowtu.Random.Range(-1.0f, 1.0f);

            //float randomRotation = static_cast<float>(Random::Range(0.0, 360.0));
            float randomRotation = 0.0f;
            particle.rotation = randomRotation;
            particle.rotationSpeed = particleProps.rotationSpeed;
            
            // Velocity
            float theta = (float)(2 * Math.PI * Gowtu.Random.GetNext());
            float x = (float)Math.Sin(theta);
            float y = (float)Math.Cos(theta);
            float z = (float)(Math.Sin(theta) * Math.Sin(theta));
            Vector3 r = new Vector3(x, y, z);

            particle.velocity = particleProps.velocity;
            particle.velocity.X += particleProps.velocityVariation.X * Gowtu.Random.Range(-1.0f, 1.0f);
            particle.velocity.Y += particleProps.velocityVariation.Y * Gowtu.Random.Range(-1.0f, 1.0f);
            particle.velocity.Z += particleProps.velocityVariation.Z * Gowtu.Random.Range(-1.0f, 1.0f);

            // Color
            particle.colorBegin = particleProps.colorBegin;
            particle.colorEnd = particleProps.colorEnd;

            particle.lifeTime = particleProps.lifeTime;
            particle.lifeRemaining = particleProps.lifeTime;
            particle.sizeBegin = particleProps.sizeBegin + particleProps.sizeVariation * (Gowtu.Random.Range(0.0f, 1.0f) - 0.5f);
            particle.sizeEnd = particleProps.sizeEnd;
            particle.size = particle.sizeBegin;

            poolIndex--;
            if (poolIndex < 0)
                poolIndex = particles.Count - 1;
        }

        public void Emit(uint amount)
        {
            if(amount > numParticles)
                amount = (uint)numParticles;

            for(uint i = 0; i < amount; i++)
                Emit(properties);
        }

        public void Emit(uint amount, ParticleProperties particleProps)
        {
            if(amount > numParticles)
                amount = (uint)numParticles;

            for(uint i = 0; i < amount; i++)
                Emit(particleProps);
        }

        internal override void OnRender()
        {
            if(VAO.Id == 0)
                return;
            
            if (!gameObject.isActive)
                return;

            Camera camera = Camera.mainCamera;

            if (camera == null || transform == null)
                return;

            if (pMesh == null)
                return;

            if (material == null)
                return;

            if (material.Shader == null)
                return;

            Update();

            if(activeParticles == 0)
                return;

            GLState.DepthMask(false);
            GLState.CullFace(false);
            GLState.DepthTest(true);
            GLState.BlendMode(true);

            material.Use(transform, camera);

            VAO.Bind();

            //Console.WriteLine("Particle Count: " + activeParticles);

            if (EBO.Id > 0)
                GL.DrawElementsInstanced(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, pMesh.IndiceCount, DrawElementsType.UnsignedInt, IntPtr.Zero, activeParticles);
            else
                GL.DrawArraysInstanced(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, 0, pMesh.VertexCount, activeParticles);

            VAO.Unbind();

            GLState.DepthMask(true);
            GLState.CullFace(true);
            GLState.DepthTest(true);
            GLState.BlendMode(false);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static float InverseLerp(float start, float end, float value)
        {
            return (value - start) / (end - start);
        }
    }

    // public sealed class ParticleSystem : Renderer
    // {
    //     [StructLayout(LayoutKind.Sequential)]
    //     private struct ParticleInstanceData
    //     {
    //         public Matrix4 matrix;
    //         public Color color;
            
    //         public ParticleInstanceData()
    //         {
    //             matrix = Matrix4.Identity;
    //             color = Color.White;
    //         }
    //     }

    //     private class Particle
    //     {
    //         public Vector3 position;
    //         public float rotation;
    //         public float rotationSpeed;
    //         public float scale;
    //         public Vector3 velocity;
    //         public Color colorBegin;
    //         public Color colorEnd;
    //         public float size;
    //         public float sizeBegin;
    //         public float sizeEnd;
    //         public float lifeTime;
    //         public float lifeRemaining;
    //         public bool active;

    //         public Particle()
    //         {
    //             position = Vector3.Zero;
    //             rotation = 0.0f;
    //             rotationSpeed = 0.0f;
    //             scale = 1.0f;
    //             velocity = Vector3.Zero;
    //             colorBegin = Color.Black;
    //             colorEnd = Color.White;
    //             sizeBegin = 1.0f;
    //             sizeEnd = 0.1f;
    //             size = sizeBegin;
    //             lifeTime = 2.0f;
    //             lifeRemaining = 0;
    //             active = false;
    //         }

    //         public Matrix4 GetMatrix(Matrix4 viewMatrix)
    //         {
    //             Matrix4 translationMatrix = Matrix4.CreateTranslation(position.X, position.Y, position.Z);

    //             Matrix4 rotationMatrix = Matrix4.Identity;

    //             // rotationMatrix[0][0] = viewMatrix[0][0];
    //             // rotationMatrix[0][1] = viewMatrix[1][0];
    //             // rotationMatrix[0][2] = viewMatrix[2][0];
                
    //             // rotationMatrix[1][0] = viewMatrix[0][1];
    //             // rotationMatrix[1][1] = viewMatrix[1][1];
    //             // rotationMatrix[1][2] = viewMatrix[2][1];
                
    //             // rotationMatrix[2][0] = viewMatrix[0][2];
    //             // rotationMatrix[2][1] = viewMatrix[1][2];
    //             // rotationMatrix[2][2] = viewMatrix[2][2];
                
    //             rotationMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation)) * rotationMatrix;
                
    //             Matrix4 scaleMatrix = Matrix4.CreateScale(scale, scale, scale);
    //             return scaleMatrix * rotation * translationMatrix;
    //         }
    //     }

    //     private Mesh m_mesh;
    //     private VertexArrayObject m_vao;
    //     private VertexBufferObject m_vbo;
    //     private ElementBufferObject m_ebo;
    //     private VertexBufferObject m_instanceVBO;
    //     private ParticleMaterial m_material;
    //     private RenderSettings m_settings;
    //     private uint m_particleCount;
    //     private uint m_maxParticles;
    //     private int m_poolIndex;
    //     private List<ParticleInstanceData> m_particleData;
    //     private List<Particle> m_particles;
    //     private ParticleSpace m_space;
    //     private ParticleProperties m_properties;

    //     public uint MaxParticles
    //     {
    //         get
    //         {
    //             return m_maxParticles;
    //         }
    //     }

    //     public ParticleSystem() : base()
    //     {
    //         m_settings = new RenderSettings();
    //         m_particleCount = 0;
    //         m_maxParticles = 10000;
    //         m_poolIndex = (int)m_maxParticles - 1;

    //         m_vao = new VertexArrayObject();
    //         m_vbo = new VertexBufferObject();
    //         m_ebo = new ElementBufferObject();
    //         m_instanceVBO = new VertexBufferObject();
    //         m_particleData = new List<ParticleInstanceData>(new ParticleInstanceData[m_maxParticles]);
    //         m_particles = new List<Particle>(new Particle[m_maxParticles]);
    //         m_space = ParticleSpace.World;

    //         for(int i = 0; i < m_particles.Count; i++)
    //             m_particles[i] = new Particle();

    //         m_properties = new ParticleProperties();
            
    //         m_material = new ParticleMaterial();
    //     }

    //     internal override void OnInitializeComponent()
    //     {
    //         SetMesh(Resources.FindMesh(Constants.GetString(ConstantString.MeshCube)));
    //         base.OnInitializeComponent();
    //     }

    //     internal override void OnDestroyComponent()
    //     {
    //         m_vao.Delete();
    //         m_vbo.Delete();
    //         m_ebo.Delete();
    //         m_instanceVBO.Delete();
    //         base.OnDestroyComponent();
    //     }

    //     public void SetMesh(Mesh mesh)
    //     {
    //         if(mesh == null)
    //             return;

    //         if(mesh.Vertices == null)
    //             return;

    //         m_mesh = mesh;

    //         //We don't want to use the VAO on the mesh itself, rather make a new one
    //         if(m_vao.Id == 0)
    //         {
    //             m_vao.Generate();   
    //             m_vbo.Generate();
    //             m_ebo.Generate();
    //             m_instanceVBO.Generate();

    //             m_vao.Bind();

    //             m_vbo.Bind();
    //             m_vbo.BufferData<Vertex>(mesh.Vertices, BufferUsageARB.StaticDraw);

    //             m_vao.EnableVertexAttribArray(0);
    //             m_vao.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), Marshal.OffsetOf(typeof(Vertex), "position"));

    //             m_vao.EnableVertexAttribArray(1);
    //             m_vao.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), Marshal.OffsetOf(typeof(Vertex), "normal"));

    //             m_vao.EnableVertexAttribArray(2);
    //             m_vao.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), Marshal.OffsetOf(typeof(Vertex), "uv"));

    //             if(mesh.Indices.Length > 0)
    //             {
    //                 m_ebo.Bind();                
    //                 m_ebo.BufferData<uint>(mesh.Indices, BufferUsageARB.StaticDraw);
    //             }

    //             m_instanceVBO.Bind();
    //             m_instanceVBO.BufferData<ParticleInstanceData>(m_particleData, BufferUsageARB.DynamicDraw);

    //             m_vao.EnableVertexAttribArray(3);
    //             m_vao.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<ParticleInstanceData>(), new IntPtr(0 * Marshal.SizeOf<Vector4>()));
                
    //             m_vao.EnableVertexAttribArray(4);
    //             m_vao.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<ParticleInstanceData>(), new IntPtr(1 * Marshal.SizeOf<Vector4>()));
                
    //             m_vao.EnableVertexAttribArray(5);
    //             m_vao.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<ParticleInstanceData>(), new IntPtr(2 * Marshal.SizeOf<Vector4>()));
                
    //             m_vao.EnableVertexAttribArray(6);
    //             m_vao.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<ParticleInstanceData>(), new IntPtr(3 * Marshal.SizeOf<Vector4>()));
                
    //             m_vao.EnableVertexAttribArray(7);
    //             m_vao.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, Marshal.SizeOf<ParticleInstanceData>(), new IntPtr(4 * Marshal.SizeOf<Color>()));

    //             m_vao.VertexAttribDivisor(3, 1);
    //             m_vao.VertexAttribDivisor(4, 1);
    //             m_vao.VertexAttribDivisor(5, 1);
    //             m_vao.VertexAttribDivisor(6, 1);
    //             m_vao.VertexAttribDivisor(7, 1);

    //             m_vao.Unbind();
    //             m_vbo.Unbind();
    //             m_ebo.Unbind();
    //             m_instanceVBO.Unbind();
    //         }
    //         else
    //         {
    //             m_vao.Bind();

    //             m_vbo.Bind();
    //             m_vbo.BufferData<Vertex>(mesh.Vertices, BufferUsageARB.StaticDraw);

    //             if(mesh.Indices.Length > 0)
    //             {
    //                 m_ebo.Bind();                
    //                 m_ebo.BufferData<uint>(mesh.Indices, BufferUsageARB.StaticDraw);
    //             }

    //             m_vao.Unbind();
    //             m_vbo.Unbind();
    //             m_ebo.Unbind();
    //         }
    //     }

    //     public override Mesh GetMesh(int index)
    //     {
    //         //Just to keep the API behaving like expected
    //         if (index != 0)
    //             return null;

    //         return m_mesh;
    //     }

    //     public ParticleMaterial GetMaterial()
    //     {
    //         return m_material;
    //     }

    //     public RenderSettings GetSettings()
    //     {
    //         return m_settings;
    //     }

    //     public void Emit(uint amount)
    //     {
    //         if(amount > m_maxParticles)
    //             amount = m_maxParticles;

    //         for(uint i = 0; i < amount; i++)
    //             Emit(m_properties);
    //     }

    //     public void Emit(ParticleProperties particleProps, uint amount)
    //     {
    //         if(amount > m_maxParticles)
    //             amount = m_maxParticles;

    //         for(uint i = 0; i < amount; i++)
    //             Emit(particleProps);
    //     }

    //     private void Emit(ParticleProperties particleProps)
    //     {
    //         Particle particle = m_particles[m_poolIndex];
    //         particle.active = true;            

    //         if(m_space == ParticleSpace.Local)
    //             particle.position = particleProps.position;
    //         else
    //             particle.position = transform.position + particleProps.position;

    //         particle.position.X += particleProps.positionVariation.X * Gowtu.Random.Range(-1.0f, 1.0f);
    //         particle.position.Y += particleProps.positionVariation.Y * Gowtu.Random.Range(-1.0f, 1.0f);
    //         particle.position.Z += particleProps.positionVariation.Z * Gowtu.Random.Range(-1.0f, 1.0f);

    //         //float randomRotation = static_cast<float>(Random::Range(0.0, 360.0));
    //         float randomRotation = 0.0f;
    //         particle.rotation = randomRotation;
    //         particle.rotationSpeed = particleProps.rotationSpeed;
            
    //         // Velocity
    //         float theta = 2 * (float)Math.PI * Gowtu.Random.GetNext();
    //         float x = (float)Math.Sin(theta);
    //         float y = (float)Math.Cos(theta);
    //         float z = (float)Math.Sin(theta) * (float)Math.Sin(theta);
    //         Vector3 r = new Vector3(x, y, z);

    //         particle.velocity = particleProps.velocity;
    //         particle.velocity.X += particleProps.velocityVariation.X * Gowtu.Random.Range(-1.0f, 1.0f);
    //         particle.velocity.Y += particleProps.velocityVariation.Y * Gowtu.Random.Range(-1.0f, 1.0f);
    //         particle.velocity.Z += particleProps.velocityVariation.Z * Gowtu.Random.Range(-1.0f, 1.0f);

    //         // Color
    //         particle.colorBegin = particleProps.colorBegin;
    //         particle.colorEnd = particleProps.colorEnd;

    //         particle.lifeTime = particleProps.lifeTime;
    //         particle.lifeRemaining = particleProps.lifeTime;
    //         particle.sizeBegin = particleProps.sizeBegin + particleProps.sizeVariation * (Gowtu.Random.Range(0.0f, 1.0f) - 0.5f);
    //         particle.sizeEnd = particleProps.sizeEnd;
    //         particle.size = particle.sizeBegin;

    //         m_poolIndex--;
    //         if (m_poolIndex < 0)
    //             m_poolIndex = m_particles.Count - 1;
    //     }

    //     internal override void OnRender()
    //     {
    //         if(m_vao.Id == 0)
    //             return;
            
    //         if (!gameObject.isActive)
    //             return;

    //         Camera camera = Camera.mainCamera;

    //         if (camera == null || transform == null)
    //             return;

    //         if (m_mesh == null)
    //             return;

    //         if (m_material == null)
    //             return;

    //         if (m_material.Shader == null)
    //             return;

    //         Update();

    //         if(m_particleCount == 0)
    //             return;

    //         GLState.DepthTest(m_settings.depthTest);
    //         GLState.CullFace(m_settings.cullFace);
    //         GLState.BlendMode(m_settings.alphaBlend);
    //         GLState.SetDepthFunc(m_settings.depthFunc);

    //         m_material.Use(transform, camera);

    //         m_vao.Bind();

    //         Console.WriteLine("Particle Count: " + m_particleCount);

    //         if (m_ebo.Id > 0)
    //             GL.DrawElementsInstanced(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, m_mesh.IndiceCount, DrawElementsType.UnsignedInt, IntPtr.Zero, (int)m_particleCount);
    //         else
    //             GL.DrawArraysInstanced(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, 0, m_mesh.VertexCount, (int)m_particleCount);

    //         m_vao.Unbind();
    //     }

    //     private void Update()
    //     {
    //         m_particleCount = 0;

    //         Matrix4 parentMatrix = m_space == ParticleSpace.Local ? transform.GetModelMatrix() : Matrix4.Identity;
    //         Camera camera = Camera.mainCamera;
    //         Matrix4 viewMatrix = camera.GetViewMatrix();

    //         for(int i = 0; i < m_particles.Count; i++)
    //         {
    //             if (m_particles[i].lifeRemaining <= 0.0f)
    //             {
    //                 m_particles[i].active = false;                    
    //                 continue;
    //             }

    //             float sizeMin = Math.Min(m_particles[i].sizeBegin, m_particles[i].sizeEnd);
    //             float sizeMax = Math.Max(m_particles[i].sizeBegin, m_particles[i].sizeEnd);
    //             float sizeRange = sizeMax - sizeMin;
    //             float timeProgress = m_particles[i].lifeTime - m_particles[i].lifeRemaining;
    //             float timePercentage = InverseLerp(0, m_particles[i].lifeTime, timeProgress);
                
    //             bool shrinking = m_particles[i].sizeEnd < m_particles[i].sizeBegin;
    //             float size = 0;

    //             if(shrinking)
    //                 size = m_particles[i].sizeBegin - (timePercentage * sizeRange);
    //             else
    //                 size = m_particles[i].sizeBegin + (timePercentage * sizeRange);

    //             if((m_particles[i].sizeBegin - m_particles[i].sizeEnd) == 0)
    //                 size = m_particles[i].sizeBegin;

    //             Color color = Color.Lerp(m_particles[i].colorBegin, m_particles[i].colorEnd, timePercentage);

    //             m_particles[i].lifeRemaining -= Time.DeltaTime;
    //             m_particles[i].position += m_particles[i].velocity * Time.DeltaTime;

    //             float rotation = m_particles[i].rotationSpeed * Time.DeltaTime;
    //             m_particles[i].rotation += rotation;
    //             m_particles[i].scale = size;
                
    //             Matrix4 matrix = parentMatrix * m_particles[i].GetMatrix(viewMatrix);

    //             var particleData = m_particleData[(int)m_particleCount];
    //             particleData.matrix = Matrix4.Identity;
    //             particleData.color = Color.Red;
    //             m_particleData[(int)m_particleCount] = particleData;
    //             m_particleCount++;
    //         }

    //         if(m_particleCount > 0)
    //         {
    //             var pData = CollectionsMarshal.AsSpan(m_particleData).Slice((int)m_particleCount);
    //             m_instanceVBO.Bind();
    //             m_instanceVBO.BufferSubData<ParticleInstanceData>(pData, 0);
    //             m_instanceVBO.Unbind();
    //         }
    //     }

    //     [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    //     private static float InverseLerp(float start, float end, float value)
    //     {
    //         return (value - start) / (end - start);
    //     }
    // }

    public struct ParticleProperties
    {
        public Vector3 position;
        public Vector3 positionVariation;
        public float rotationSpeed;
        public Vector3 velocity;
        public Vector3 velocityVariation;
        public Color colorBegin;
        public Color colorEnd;
        public float sizeBegin;
        public float sizeEnd;
        public float sizeVariation;
        public float lifeTime;

        public ParticleProperties()
        {
            position = new Vector3(0, 0, 0);
            positionVariation = new Vector3(1, 1, 1);
            rotationSpeed = 0.0f;
            velocity = new Vector3(0, 1, 0);
            velocityVariation = new Vector3(0.1f, 0.1f, 0.1f);
            colorBegin = new Color(0.5f,  0.5f, 0.5f, 0.5f);
            colorEnd = new Color(0.1f, 0.1f, 0.1f, 0.0f);
            sizeBegin = 1.5f;
            sizeEnd = 0.1f;
            sizeVariation = 2.0f;
            lifeTime = 10.0f;
        }
    }

    public enum ParticleSpace
    {
        Local,
        World
    }

    public enum ParticleType
    {
        Capsule,
        Cube,
        Plane,
        Quad,
        Sphere
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ParticleInstanceData
    {
        public Matrix4 matrix;
        public Color color;
        
        public ParticleInstanceData()
        {
            Matrix4 scale = Matrix4.CreateScale(1, 1, 1);
            Matrix4 translation = Matrix4.CreateTranslation(0, 0, 0);
            Matrix4 rotation = Matrix4.CreateFromQuaternion(Quaternion.Identity);
            matrix = scale * rotation * scale;
            color = Color.White;
        }
    }

    public class Particle
    {
        public Vector3 position;
        public float rotation;
        public float rotationSpeed;
        public float scale;
        public Vector3 velocity;
        public Color colorBegin;
        public Color colorEnd;
        public float size;
        public float sizeBegin;
        public float sizeEnd;
        public float lifeTime;
        public float lifeRemaining;
        public bool active;

        public Particle()
        {
            position = Vector3.Zero;
            rotation = 0.0f;
            rotationSpeed = 0.0f;
            scale = 1.0f;
            velocity = Vector3.Zero;
            colorBegin = Color.Black;
            colorEnd = Color.White;
            sizeBegin = 1.0f;
            sizeEnd = 0.1f;
            size = sizeBegin;
            lifeTime = 2.0f;
            lifeRemaining = 0;
            active = false;
        }

        public Matrix4 GetMatrix(Matrix4 viewMatrix)
        {
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position.X, position.Y, position.Z);

            Matrix4 rotationMatrix = Matrix4.Identity;

            //Makes sure the particles are always facing the camera
            rotationMatrix.Row0.Xyz = viewMatrix.Column0.Xyz;
            rotationMatrix.Row1.Xyz = viewMatrix.Column1.Xyz;
            rotationMatrix.Row2.Xyz = viewMatrix.Column2.Xyz;

            rotationMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation)) * rotationMatrix;
            
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale, scale, scale);
            return scaleMatrix * rotationMatrix * translationMatrix;
        }
    }
}