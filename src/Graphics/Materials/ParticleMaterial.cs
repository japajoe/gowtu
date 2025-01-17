using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Gowtu
{
    public sealed class ParticleMaterial : Material
    {
        private int uDiffuseTexture;
        private int uUVOffset;
        private int uUVScale;
        private int uAlphaCutOff;

        private Texture2D diffuseTexture;
        private Vector2 uvScale;
        private Vector2 uvOffset;
        private float alphaCutOff;

        public Texture2D DiffuseTexture
        {
            get
            {
                return diffuseTexture;
            }
            set
            {
                diffuseTexture = value;
            }
        }

        public Vector2 UVScale
        {
            get
            {
                return uvScale;
            }
            set
            {
                uvScale = value;
            }
        }

        public Vector2 UVOffset
        {
            get
            {
                return uvOffset;
            }
            set
            {
                uvOffset = value;
            }
        }

        public float AlphaCutOff
        {
            get
            {
                return alphaCutOff;
            }
            set
            {
                alphaCutOff = value;
            }
        }

        public ParticleMaterial() : base()
        {
            shader = Resources.FindShader(Constants.GetString(ConstantString.ShaderParticle));

            diffuseTexture = Resources.FindTexture<Texture2D>(Constants.GetString(ConstantString.TextureDefault));
            uvScale = new Vector2(1, 1);
            uvOffset = new Vector2(0, 0);
            alphaCutOff = 0.0f;

            if(shader != null)
            {
                uDiffuseTexture = GL.GetUniformLocation(shader.Id, "uDiffuseTexture");
                uUVOffset = GL.GetUniformLocation(shader.Id, "uUVOffset");
                uUVScale = GL.GetUniformLocation(shader.Id, "uUVScale");
                uAlphaCutOff = GL.GetUniformLocation(shader.Id, "uAlphaCutOff");
            }
        }

        public override void Use(Transform transform, Camera camera)
        {
            if(shader == null || camera == null || transform == null)
                return;

            shader.Use();

            int unit = 0;

            if(diffuseTexture != null)
            {
                diffuseTexture.Bind(unit);
                shader.SetInt(uDiffuseTexture, unit);
                unit++;
            }

            shader.SetFloat2(uUVScale, uvScale);
            shader.SetFloat2(uUVOffset, uvOffset);
            shader.SetFloat(uAlphaCutOff, alphaCutOff);
        }
    }
}