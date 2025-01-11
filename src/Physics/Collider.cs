using BulletSharp;
using OpenTK.Mathematics;

namespace Gowtu
{
    public class Collider : Component
    {
        public CollisionShape shape;
        
        public Collider() : base()
        {

        }

        internal override void OnInitializeComponent()
        {
            var rigidBody = gameObject.GetComponent<Rigidbody>();

            if(rigidBody != null)
            {
                rigidBody.OnInitializeComponent();
            }
        }

        public void Dispose()
        {
            shape.Dispose();
        }
    }
}
