using BulletSharp;
using OpenTK.Mathematics;

namespace Gowtu
{
    public sealed class CompoundCollider : Collider
    {
        public CompoundCollider() : base()
        {

        }

        public void AddShape(CollisionShape collisionShape, Vector3 position, Quaternion rotation)
        {
            if(shape == null)
                shape = new CompoundShape();

            CompoundShape compound = (CompoundShape)shape;

            var pos = new System.Numerics.Vector3(position.X, position.Y, position.Z);
            var rot = new System.Numerics.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
            var scale = new System.Numerics.Vector3(1, 1, 1);
                        
            var translationMatrix = System.Numerics.Matrix4x4.CreateTranslation(pos);
            var rotationMatrix = System.Numerics.Matrix4x4.CreateFromQuaternion(rot);
            var scaleMatrix = System.Numerics.Matrix4x4.CreateScale(scale);

            var m = scaleMatrix * rotationMatrix * translationMatrix;

            compound.AddChildShape(m, collisionShape);
        }

        public void Initialize()
        {
            OnInitializeComponent();
        }
        
        internal override void OnInitializeComponent()
        {
            if(shape == null)
                return;
            
            var rigidbody = gameObject.GetComponent<Rigidbody>();

            if(rigidbody != null)
            {
                rigidbody.OnInitializeComponent();                
            }
        }
    }
}
