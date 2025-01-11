using BulletSharp;

namespace Gowtu
{
    public sealed class CapsuleCollider : Collider
    {
        private float m_radius = 0;
        private float m_height = 0;

        public float radius
        {
            get 
            { 
                return m_radius; 
            }
            set 
            { 
                m_radius = value;
                OnInitializeComponent();
            }
        }

        public float height
        {
            get 
            { 
                return m_height; 
            }
            set 
            { 
                m_height = value; 
                OnInitializeComponent();
            }
        }

        public CapsuleCollider() : base()
        {
            
        }

        internal override void OnInitializeComponent()
        {
            if(m_height <= 0 && m_radius <= 0)
                return;

            var rigidbody = gameObject.GetComponent<Rigidbody>();

            if(rigidbody != null)
            {
                shape = new CapsuleShape(radius, height);
                rigidbody.OnInitializeComponent();                
            }
        }
    }
}
