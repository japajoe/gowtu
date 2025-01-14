namespace Gowtu
{
    public sealed class ParticleSystem : Renderer
    {
        private Mesh m_mesh;
        private VertexBufferObject m_instanceVBO;
        private Material m_material;
        private RenderSettings m_settings;
        private uint m_instanceCount;
        private uint m_maxInstances;

        public ParticleSystem() : base()
        {
            m_settings = new RenderSettings();
            m_instanceCount = 0;
            m_maxInstances = 100000; // 2 ^ 20
            m_instanceVBO = new VertexBufferObject();
        }

        internal override void OnInitializeComponent()
        {
            
        }

        internal override void OnDestroyComponent()
        {

        }

        internal override void OnRender()
        {
            
        }

        public override Mesh GetMesh(int index)
        {
            //Just to keep the API behaving like expected
            if (index != 0)
                return null;

            return m_mesh;
        }
    }
}