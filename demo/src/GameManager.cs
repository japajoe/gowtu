using System;
using System.Collections.Generic;
using Gowtu;
using MiniAudioEx.DSP;
using OpenTK.Mathematics;

namespace GowtuApp
{
    public class GameManager : GameBehaviour
    {
        private int currentResourceCount = 0;
        private int totalResourceCount = 0;
        private GameObject[] spheres;
        private GameObject cube;
        private Terrain terrain;
        private SkyboxMaterial skyboxMaterial;
        private Font defaultFont;
        private bool lightEnabled = true;
        private Color fogColorDay = new Color(251, 241, 206, 255);
        private Color fogColorNight = new Color(0.001f, 0.001f, 0.001f, 1.0f);

        public bool Loaded
        {
            get
            {
                return currentResourceCount >= totalResourceCount;
            }
        }

        private void Start()
        {
            World.FogDensity *= 3;
            World.FogColor = fogColorDay;

            defaultFont = Resources.FindFont(Constants.GetString(ConstantString.FontDefault));
            GUIStyle.SetStyle1();            
            SetupCamera();
            SetupLight();
            LoadResources();
        }

        private void LoadResources()
        {
            var textureBatch = new List<string>()
            {
                "Resources/Textures/Box.jpg",
                "Resources/Textures/Grass.jpg",
                "Resources/Textures/coast_sand_rocks_02_diff_1k.jpg",
                "Resources/Textures/forrest_ground_01_diff_1k.jpg",
                "Resources/Textures/Mud.png",
                "Resources/Textures/Splatmap.jpg",
                "Resources/Textures/billboardgrass0002.png",
                "Resources/Textures/RedFlower.png",
                "Resources/Textures/YellowFlower.png",
                "Resources/Textures/smoke_04.png"
            };

            var fontBatch = new List<string>()
            {
                "Resources/Fonts/SF Sports Night.ttf"
            };

            var audioBatch = new List<string>()
            {
                "Resources/Audio/unhappy-drone-67284.wav",
                "Resources/Audio/click1.mp3",
                "Resources/Audio/click2.mp3",
                "Resources/Audio/Fire.wav"
            };

            var skyboxBatch = new List<string>()
            {
                "Resources/Textures/Skyboxes/Sahara/right.png",
                "Resources/Textures/Skyboxes/Sahara/left.png",
                "Resources/Textures/Skyboxes/Sahara/bottom.png",
                "Resources/Textures/Skyboxes/Sahara/top.png",
                "Resources/Textures/Skyboxes/Sahara/front.png",
                "Resources/Textures/Skyboxes/Sahara/back.png"
            };

            var shaderBatch = new List<string>()
            {
                "Resources/Shaders/BasicV.glsl",
                "Resources/Shaders/FireF.glsl"
            };

            totalResourceCount = 0;
            totalResourceCount += skyboxBatch.Count;
            totalResourceCount += textureBatch.Count;
            totalResourceCount += fontBatch.Count;
            totalResourceCount += audioBatch.Count;
            totalResourceCount += shaderBatch.Count;

            Resources.LoadAsyncBatchFromAssetPack("assets.dat", "assets.dat", ResourceType.TextureCubeMap, skyboxBatch);
            Resources.LoadAsyncBatchFromAssetPack("assets.dat", "assets.dat", ResourceType.Texture2D, textureBatch);
            Resources.LoadAsyncBatchFromAssetPack("assets.dat", "assets.dat", ResourceType.Font, fontBatch);
            Resources.LoadAsyncBatchFromAssetPack("assets.dat", "assets.dat", ResourceType.AudioClip, audioBatch);
            Resources.LoadAsyncBatchFromAssetPack("assets.dat", "assets.dat", ResourceType.Shader, shaderBatch);
        }

        private void SetupCamera()
        {
            var camera = Camera.mainCamera;
            camera.transform.position = new Vector3(0, 1, 4);
            camera.farClippingPlane = 10000.0f;

            camera.gameObject.AddComponent<AudioListener>();

            var fps = Camera.mainCamera.gameObject.AddComponent<FirstPersonCamera>();
            fps.speed *= 0.01f;
        }

        private void SetupLight()
        {
            var directionalLight = Light.mainLight;
            directionalLight.Type = LightType.Directional;
            directionalLight.Strength = 0.1f;
            directionalLight.transform.position = new Vector3(0, 1000, 1000);
            directionalLight.transform.LookAt(new Vector3(1000, 0, 0), new Vector3(0, 1, 0));
            directionalLight.gameObject.isActive = lightEnabled;

            float x = MathHelper.DegreesToRadians(45.0f);
            float z = MathHelper.DegreesToRadians(15.0f);
            directionalLight.transform.rotation = Quaternion.FromEulerAngles(x, 0, z);
        }

        private void SetupTerrain()
        {
            var plane = GameObject.CreatePrimitive(PrimitiveType.Terrain);
            terrain = plane.GetComponent<Terrain>();
            var terrainMaterial = terrain.GetMaterial();

            var perlin = new Perlin();
            float frequency = 0.1f;
            float amplitude = 10.0f;

            for(int y = 0; y < terrain.Resolution.Y + 1; y++)
            {
                for(int x = 0; x < terrain.Resolution.X + 1; x++)
                {
                    float sample = (float)perlin.Noise(x * frequency, y * frequency) * amplitude;

                    float baseHeight = 1.0f;
                    terrain.SetHeight(x, y, sample + baseHeight);
                }
            }

            terrain.Update();

            Vector2 terrainSize = new Vector2(terrain.Resolution.X * terrain.Scale.X, terrain.Resolution.X * terrain.Scale.Z);

            plane.transform.position -= new Vector3(terrainSize.X / 2, 0, -1.0f * (terrainSize.Y / 2));

            terrainMaterial.SplatMap = Resources.FindTexture<Texture2D>("Resources/Textures/Splatmap.jpg");
            terrainMaterial.Texture1 = Resources.FindTexture<Texture2D>("Resources/Textures/Mud.png");
            terrainMaterial.Texture2 = Resources.FindTexture<Texture2D>("Resources/Textures/coast_sand_rocks_02_diff_1k.jpg");
            terrainMaterial.Texture3 = Resources.FindTexture<Texture2D>("Resources/Textures/forrest_ground_01_diff_1k.jpg");
            terrainMaterial.Texture4 = Resources.FindTexture<Texture2D>("Resources/Textures/Grass.jpg");

            float uvScaleX = (terrain.Resolution.X * terrain.Scale.X * 0.5f * 1.0f);
            float uvScaleY = (terrain.Resolution.Y * terrain.Scale.Z * 0.5f * 1.0f);

            terrainMaterial.UvScale1 = new Vector2(uvScaleX, uvScaleY);
            terrainMaterial.UvScale2 = new Vector2(uvScaleX / 5.0f, uvScaleY / 5.0f);
            terrainMaterial.UvScale3 = new Vector2(uvScaleX, uvScaleY);
            terrainMaterial.UvScale4 = new Vector2(uvScaleX / 5.0f, uvScaleY / 5.0f);

            var rb1 = plane.AddComponent<Rigidbody>();
            rb1.mass = 0;
            var collider = plane.AddComponent<MeshCollider>();
            collider.mesh = terrain.GetMesh(0);

            var testCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testCube.GetComponent<MeshRenderer>().GetMaterial<DiffuseMaterial>(0).DiffuseTexture = Resources.FindTexture<Texture2D>("Resources/Textures/Box.jpg");
            testCube.AddComponent<CollisionTester>();
            var rb2 = testCube.AddComponent<Rigidbody>();
            rb2.mass = 10;
            var boxCollider = testCube.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(1, 1, 1);
            rb2.MovePosition(new Vector3(0, 100, 0));

        }

        private void SetupObjects()
        {
            spheres = new GameObject[3];

            Color[] lightColors = new Color[3]
            {
                Color.Red,
                Color.Green,
                Color.Blue,
            };

            for(int i = 0; i < 3; i++)
            {
                spheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                spheres[i].GetComponent<MeshRenderer>().GetMaterial<DiffuseMaterial>(0).DiffuseColor = lightColors[i];

                var light = spheres[i].AddComponent<Light>();
                light.Type = LightType.Point;
                light.Strength = 1.5f;
                light.Color = lightColors[i];
                
                double t = (2 * Math.PI) / 3.0f;
                float x = (float)Math.Cos(t * i) * 50.0f;
                float z = (float)Math.Sin(t * i) * 50.0f;
                
                spheres[i].transform.position = new Vector3(x, 5, z);

                var audioSource = spheres[i].AddComponent<AudioSource>();
                audioSource.Spatial = true;
                audioSource.DopplerFactor = 0.1f;
                audioSource.MinDistance = 5.0f;
                audioSource.MaxDistance = 1000.0f;
                audioSource.AttenuationModel = MiniAudioEx.AttenuationModel.Exponential;

                var noiseGenerator = new NoiseGenerator(NoiseType.Brown);
                var lfoEffect = new LFOEffect(WaveType.Sine);
                var filterEffect = new FilterEffect(FilterType.Lowpass, 220.0f, 0.5f, 1.0f);

                audioSource.AddGenerator(noiseGenerator);
                audioSource.AddEffect(lfoEffect);
                audioSource.AddEffect(filterEffect);

                audioSource.Play();
            }

            cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Cube";
            cube.transform.position = new Vector3(0, 2, 0);
            cube.transform.scale = new Vector3(1, 1, 1);
            cube.GetComponent<MeshRenderer>().GetMaterial<DiffuseMaterial>(0).DiffuseTexture = Resources.FindTexture<Texture2D>("Resources/Textures/Box.jpg");

            var particles = GameObject.CreatePrimitive(PrimitiveType.ParticleSystem);
            particles.transform.SetParent(spheres[0].transform);
            particles.transform.position = spheres[0].transform.position;
            particleSystem = particles.GetComponent<ParticleSystem>();
            particleSystem.renderQueue = 1005;
            particleSystem.Space = ParticleSpace.World;
            var material = particleSystem.GetMaterial();
            material.DiffuseTexture = Resources.FindTexture<Texture2D>("Resources/Textures/smoke_04.png");
            material.AlphaCutOff = 0.05f;
        }

        private void SetupVegetation(string texturePath, int instanceCount)
        {
            GameObject grass = new GameObject();
            var batchRenderer = grass.AddComponent<BatchRenderer>();
            batchRenderer.SetMesh(MeshGenerator.CreateQuad(Vector3.One));
            var material = new DiffuseInstancedMaterial();
            material.DiffuseTexture = Resources.FindTexture<Texture2D>(texturePath);
            batchRenderer.SetMaterial(material);
            batchRenderer.InstanceCount = (uint)instanceCount;
            var settings = batchRenderer.GetSettings();
            settings.cullFace = false;
            settings.alphaBlend = true;
            
            batchRenderer.castShadows = false;
            material.ReceiveShadows = true;
            batchRenderer.renderQueue = 1001;
            material.UVScale = new Vector2(1, -1);

            int seed = texturePath.GetHashCode();
            var rand = new System.Random(seed);

            var getRandomFloat = () => {
                float x = (float)rand.NextDouble();
                float y = rand.NextDouble() > 0.5 ? 1.0f : -1.0f;
                return x * y;
            };

            var getRandomInt = (int min, int max) => {
                double x = rand.NextDouble() * (max - min);
                return (int)(min + x);
            };

            for(int i = 0; i < batchRenderer.InstanceCount; i++)
            {
                int x = getRandomInt(0, (int)terrain.Resolution.X + 1);
                int z = getRandomInt(0, (int)terrain.Resolution.Y + 1);

                if(terrain.GetVertexAtPoint(x, z, out Vector3 position))
                {
                    position += terrain.transform.position;
                    position.X += getRandomFloat() * 5.0f;
                    position.Y += 0.5f;
                    position.Z += getRandomFloat() * 5.0f;

                    //terrain.GetHeightAtPosition(position, out position.Y);

                    var rotation = Quaternion.FromEulerAngles(0, MathHelper.DegreesToRadians(getRandomFloat() * 360.0f), 0);
                    var scale = new Vector3(1, 1, 1);
                    batchRenderer.SetInstanceData(i, position, rotation, scale, Color.White);
                }
            }
        }

        private void OnResourceBatchLoaded(ResourceBatch batch)
        {
            bool error = false;

            for(int i = 0; i < batch.resources.Count; i++)
            {
                if(batch.resources[i].result != ResourceLoadResult.Ok)
                {
                    Console.WriteLine("Failed to load resource " + batch.resources[i].filePath);
                    error = true;
                }
            }

            if(error)
                return;

            switch(batch.type)
            {
                case ResourceType.AudioClip:
                {
                    for(int i = 0; i < batch.resources.Count; i++)
                    {
                        var audioclip = new AudioClip(batch.resources[i].data);
                        Resources.AddAudioClip(batch.resources[i].filePath, audioclip);
                    }
                    break;
                }
                case ResourceType.Texture2D:
                {
                    for(int i = 0; i < batch.resources.Count; i++)
                    {
                        var texture = new Texture2D(new Image(batch.resources[i].data));
                        Resources.AddTexture(batch.resources[i].filePath, texture);
                    }
                    break;
                }
                case ResourceType.TextureCubeMap:
                {
                    Image[] images = new Image[6];

                    for(int i = 0; i < batch.resources.Count; i++)
                    {
                        images[i] = new Image(batch.resources[i].data);
                    }
                    
                    World.FogColor = lightEnabled ? Color.White : new Color(0.001f, 0.001f, 0.001f, 1.0f);

                    TextureCubeMap cubemap = new TextureCubeMap(images);
                    Resources.AddTexture("SaharaSkybox", cubemap);
                    GameObject skybox = GameObject.CreatePrimitive(PrimitiveType.Skybox);
                    skyboxMaterial = skybox.GetComponent<MeshRenderer>().GetMaterial<SkyboxMaterial>(0);
                    skyboxMaterial.Texture = cubemap;
                    skyboxMaterial.DiffuseColor = lightEnabled ? Color.White : new Color(0.001f, 0.001f, 0.001f, 1.0f);

                    break;
                }
                case ResourceType.Shader:
                {
                    string vertex = BinaryConverter.ToString(batch.resources[0].data, 0, batch.resources[0].data.Length, TextEncoding.UTF8);
                    string fragment = BinaryConverter.ToString(batch.resources[1].data, 0, batch.resources[1].data.Length, TextEncoding.UTF8);
                    
                    var shader = new Shader(vertex, fragment);
                    Resources.AddShader("Fire", shader);

                    break;
                }
                case ResourceType.Font:
                {
                    for(int i = 0; i < batch.resources.Count; i++)
                    {
                        var font = new Font();
                        if(font.LoadFromMemory(batch.resources[i].data, batch.resources[i].data.Length, 64, FontRenderMethod.SDF))
                        {
                            font.GenerateTexture();
                            Resources.AddFont(batch.resources[i].filePath, font);
                        }
                    }
                    break;
                }
                default:
                    break;
            }

            currentResourceCount += batch.resources.Count;

            if(currentResourceCount == totalResourceCount)
            {
                OnLoadingComplete();
            }

            batch.resources.Clear();
        }

        ParticleSystem particleSystem = null;

        private void OnLoadingComplete()
        {
            SetupTerrain();
            SetupVegetation("Resources/Textures/billboardgrass0002.png", 50000);
            SetupVegetation("Resources/Textures/RedFlower.png", 1000);
            SetupVegetation("Resources/Textures/YellowFlower.png", 1000);
            SetupObjects();
            gameObject.AddComponent<AudioManager>();
            gameObject.AddComponent<MenuManager>();
        }

        private float angle = 0;

        private void Update()
        {
            if(!Loaded)
            {
                const float fontSize = 32;
                string text = "Loading asset " + (currentResourceCount+1) + "/" + totalResourceCount;
                defaultFont.CalculateBounds(text, text.Length, fontSize, out float w, out float h);
                
                var viewport = Graphics.GetViewport();
                Vector2 position = new Vector2((viewport.width - w) * 0.5f, (viewport.height - h) * 0.5f);

                Graphics2D.AddText(position, defaultFont, text, fontSize, Color.Black, false);
            }

            if(cube == null || spheres == null)
                return;

            cube.transform.rotation = Quaternion.FromAxisAngle(Vector3.UnitY, Time.Elapsed);

            if(Input.GetKeyDown(KeyCode.C))
            {
                Input.SetMouseCursor(!Input.IsCursorVisible());
            }

            angle += 1.0f * Time.DeltaTime * 0.5f;

            for(int i = 0; i < spheres.Length; i++)
            {
                float radius = 50.0f;
                float sphereAngle = angle + (i * (float)Math.PI * 2 / spheres.Length);
                float x = (float)Math.Cos(sphereAngle) * radius;
                float z = (float)Math.Sin(sphereAngle) * radius;
                
                spheres[i].transform.position = new Vector3(x, 5, z);
            }

            if(particleSystem != null)
            {
                var properties = particleSystem.Properties;
                properties.colorBegin = new Color(0.1f, 0.1f, 0.1f, 0.5f);
                properties.lifeTime = 2.0f;
                particleSystem.Emit(8, properties);
            }
        }

        private void OnGUI()
        {
            if(!Loaded)
                return;

            ImGuiNET.ImGui.SetNextWindowPos(new System.Numerics.Vector2(5, 5));
            if(ImGuiEx.BeginWindow("Light", new System.Numerics.Vector2(128, 64)))
            {
                if(ImGuiNET.ImGui.Checkbox("Toggle Light", ref lightEnabled))
                {
                    ToggleLight(lightEnabled);
                }
                
                ImGuiNET.ImGui.Text("FPS " + Time.FPS);

                ImGuiEx.EndWindow();
            }
        }

        private void ToggleLight(bool enabled)
        {
            lightEnabled = enabled;

            Light.mainLight.gameObject.isActive = lightEnabled;

            World.FogColor = lightEnabled ? fogColorDay : fogColorNight;
            skyboxMaterial.DiffuseColor = lightEnabled ? Color.White : fogColorNight;
        }
    }

    public class CollisionTester : GameBehaviour
    {
        private void OnCollision(Rigidbody rb1, Rigidbody rb2)
        {
            // rb1.MovePosition(new Vector3(0, 5, -5));
            // rb1.MoveRotation(Quaternion.Identity);
            // rb1.velocity = Vector3.Zero;
            // rb1.angularVelocity = Vector3.Zero;
        }
    }
}