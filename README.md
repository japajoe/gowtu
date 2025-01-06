# Gowtu
A simple library for game development. I initially made it to showcase the capabilities of [my audio library](https://github.com/japajoe/MiniAudioExNET), although the demo is not ready yet.

# Features
- Component based design, very similar to Unity.
- Create primitive objects easily (including procedural skybox and terrain).
- UI rendering with Dear ImGui.
- Directional and point lights.
- Immediate mode custom 2D shape rendering with your own shaders.
- Frustum culling.
- Mouse picking.
- Advanced audio system with full support for spatialization.
- Asset loading from asset packs.
- Cross platform.

# Dependencies
- [GLFWNet](https://www.nuget.org/packages/JAJ.Packages.GLFWNet)
- [OpenTK5](https://www.nuget.org/packages/JAJ.Packages.OpenTK5)
- [MiniAudioEx](https://www.nuget.org/packages/JAJ.Packages.MiniAudioEx)
- [ImGui.NET](https://www.nuget.org/packages/ImGui.NET)
- [StbImageSharp](https://www.nuget.org/packages/StbImageSharp)

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
        private GameObject light;
        private GameObject skybox;
        private GameObject plane;
        private GameObject cube;

        private void Start()
        {
            var camera = new GameObject();
            camera.transform.position = new Vector3(0, 1, 4);
            
            //Main camera must be assigned in order to render objects
            Camera.mainCamera = camera.AddComponent<Camera>();
            
            //To control camera use WASD (R to go up, F to go down)
            //Hold right mouse button to rotate
            var firstPersonCamera = camera.AddComponent<FirstPersonCamera>();
            firstPersonCamera.speed *= 0.05f;

            //This will light things up :)
            light = new GameObject();
            light.transform.position = new Vector3(100, 200, 100);
            light.transform.LookAt(transform);

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