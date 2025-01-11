using BulletSharp;
using OpenTK.Mathematics;

namespace Gowtu
{
    public sealed class BoxCollider : Collider
    {
        private Vector3 m_size;

        public Vector3 size
        {
            get
            {
                return m_size;
            }
            set
            {
                m_size = value;
                OnInitializeComponent();
            }
        }

        public BoxCollider() : base()
        {

        }
        
        internal override void OnInitializeComponent()
        {
            if(m_size == Vector3.Zero)
                return;

            var rigidbody = gameObject.GetComponent<Rigidbody>();

            if(rigidbody != null)
            {
                shape = new BoxShape(m_size.X / 2, m_size.Y / 2, m_size.Z / 2);
                rigidbody.OnInitializeComponent();                
            }
        }
    }
}
