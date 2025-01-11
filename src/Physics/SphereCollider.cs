using BulletSharp;

namespace Gowtu
{
    public sealed class SphereCollider : Collider
    {
        private float m_radius = 0;

        public float radius
        {
            get 
            { 
                return m_radius; 
            }
            set 
            { 
                m_radius = value; 
                if(m_radius > 0)
                {
                    OnInitializeComponent();
                }
            }
        }

        public SphereCollider() : base()
        {

        }

        internal override void OnInitializeComponent()
        {
            if(m_radius <= 0)
                return;

            var rigidbody = gameObject.GetComponent<Rigidbody>();

            if(rigidbody != null)
            {
                shape = new SphereShape(radius);
                rigidbody.OnInitializeComponent();                
            }
        }
    }
}
