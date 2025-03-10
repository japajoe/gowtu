# Gowtu
This is a C# framework for making simple games and other graphical applications. There is quite some functionality to quickly get something going.

# Features
- Component based design, very similar to Unity.
- Create primitive objects easily (including procedural skybox and terrain).
- Directional and point lights.
- Cascaded shadow mapping.
- Frustum culling.
- Immediate mode custom 2D shape and text rendering with your own shaders.
- UI rendering with Dear ImGui.
- Rigidbody physics.
- Mouse picking.
- Advanced audio system with full support for spatialization.
- Asynchronous asset loading from disk or asset packs.

# Dependencies
- [GLFWNet](https://www.nuget.org/packages/JAJ.Packages.GLFWNet)
- [OpenTK5](https://www.nuget.org/packages/JAJ.Packages.OpenTK5)
- [MiniAudioEx](https://www.nuget.org/packages/JAJ.Packages.MiniAudioEx)
- [BulletSharp](https://www.nuget.org/packages/JAJ.Packages.BulletSharp)
- [FreeTypeSharp](https://www.nuget.org/packages/JAJ.Packages.FreeTypeSharp/)
- [ImGui.NET](https://www.nuget.org/packages/ImGui.NET)
- [StbImageSharp](https://www.nuget.org/packages/StbImageSharp)
- [AssimpNet](https://www.nuget.org/packages/AssimpNet)

# Examples
![Example 1](images/Example1.png)
![Example 2](images/Example2.png)
![Example 3](images/Example3.png)

# Hello world
```csharp
using Gowtu;
using OpenTK.Mathematics;

namespace GowtuApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            Application application = new Application(512, 512, "GowtuApp");
            application.Load += OnLoaded;
            application.Run();
        }
        
        private static void OnLoaded()
        {
            GameObject g = new GameObject();
            g.AddComponent<Game>();
        }
    }

    public class Game : GameBehaviour
    {
        private GameObject camera;
        private GameObject light;
        private GameObject skybox;
        private GameObject plane;
        private GameObject cube;

        private void Start()
        {
            camera = Camera.mainCamera.gameObject;
            camera.transform.position = new Vector3(0, 1, 4);
            
            //To control camera use WASD (R to go up, F to go down)
            //Hold right mouse button to rotate
            var firstPersonCamera = camera.AddComponent<FirstPersonCamera>();
            firstPersonCamera.speed *= 0.05f;

            //This will light things up :)
            light = Light.mainLight.gameObject;
            light.transform.position = new Vector3(100, 200, 100);

            //A procedural skybox
            skybox = GameObject.CreatePrimitive(PrimitiveType.Skybox);

            //The ground
            plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.scale = new Vector3(1000, 1, 1000);
            var material = plane.GetComponent<MeshRenderer>().GetMaterial<DiffuseMaterial>(0);
            material.DiffuseColor = new Color(55, 166, 7, 255);

            //A cube which we will rotate
            cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = new Vector3(0, 2, 0);
            material = cube.GetComponent<MeshRenderer>().GetMaterial<DiffuseMaterial>(0);
            material.DiffuseColor = new Color(52, 164, 235, 255);
        }

        private void Update()
        {
            cube.transform.rotation = Quaternion.FromAxisAngle(Vector3.UnitY, Time.Elapsed);
        }
    }
}
```