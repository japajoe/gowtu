using System;
using Gowtu;

namespace GowtuApp
{
    public sealed class MenuManager : GameBehaviour
    {
        private struct AudioConfig
        {
            public uint sampleRate;
            public uint deviceId;
            public bool dirty;

            public AudioConfig()
            {
                this.sampleRate = AudioSettings.OutputSampleRate;
                this.deviceId = AudioSettings.DeviceId;
                this.dirty = false;
            }
        }

        private enum MenuState
        {
            None,
            Main,
            Settings,
            Quit
        }

        private MenuState menuState;
        private Shader shader;
        private Font font;
        private AudioManager audioManager;
        private AudioConfig audioConfig;
        private string[] devices;
        private int selectedDevice = -1;

        private void Start()
        {
            audioConfig = new AudioConfig();

            var deviceInfo = Audio.GetDevices();

            if(deviceInfo != null)
            {
                devices = new string[deviceInfo.Length];

                for(int i = 0; i < devices.Length; i++)
                {
                    devices[i] = deviceInfo[i].Name;
                }
            }

            menuState = MenuState.None;

            audioManager = gameObject.GetComponent<AudioManager>();

            shader = Resources.FindShader("Fire");

            audioManager = gameObject.GetComponent<AudioManager>();

            font = Resources.FindFont("Resources/Fonts/SF Sports Night.ttf");
        }

        private void Update()
        {
            if(Input.GetKeyUp(KeyCode.Escape))
            {
                switch(menuState)
                {
                    case MenuState.None:
                        menuState = MenuState.Main;
                        break;
                    case MenuState.Main:
                        menuState = MenuState.None;
                        break;
                    case MenuState.Settings:
                        if(audioConfig.dirty)
                        {
                            audioConfig.dirty = false;
                            selectedDevice = -1;
                        }
                        menuState = MenuState.Main;
                        break;
                    case MenuState.Quit:
                        menuState = MenuState.Main;
                        break;
                }

                SetCursor();
                SetMenuAudio();
            }

            var getColor = (Color color, float t) => 
            {
                color.a -= (float)((Math.Sin(Time.Elapsed) + 1.0f) * 0.5f) * t;
                return color;  
            };

            if(menuState != MenuState.None)
            {
                var position = new OpenTK.Mathematics.Vector2(0, 0);
                var viewport = Graphics.GetViewport();
                var size = new OpenTK.Mathematics.Vector2(viewport.width, viewport.height);
                Graphics2D.AddRectangle(position, size, 0, Color.White, new Rectangle(), shader.Id);
                Color color = getColor(new Color(1.0f, 1.0f, 1.0f, 0.2f), 0.12f);
                RenderText("Gowtu", 16, color, new OpenTK.Mathematics.Vector2(10, 10));
            }
            else
            {
                Color color = new Color(0, 0, 0, 0.5f);
                RenderText("Gowtu", 16, color, new OpenTK.Mathematics.Vector2(10, 10));
            }

        }

        private void OnGUI()
        {
            switch(menuState)
            {
                case MenuState.None:
                    break;
                case MenuState.Main:
                    ShowMainMenu();
                    break;
                case MenuState.Settings:
                    ShowSettingsMenu();
                    break;
                case MenuState.Quit:
                    ShowQuitMenu();
                    break;
            }

            GUI.EndFrame();
        }

        private void ShowMainMenu()
        {
            if(GUI.BeginWindow("Main Menu", new System.Numerics.Vector2(250, 150)))
            {
                GUI.CenterWindow();

                var buttonSize = new System.Numerics.Vector2(100, 30);

                if(GUI.Button("Continue", buttonSize))
                {
                    menuState = MenuState.None;
                    SetCursor();
                    SetMenuAudio();
                }

                if(GUI.Button("Settings", buttonSize))
                    menuState = MenuState.Settings;
                
                if(GUI.Button("Quit", buttonSize))
                    menuState = MenuState.Quit;
                
                GUI.EndWindow();
            }
        }

        private void ShowSettingsMenu()
        {            
            if(GUI.BeginWindow("Settings Menu", new System.Numerics.Vector2(250, 150)))
            {
                GUI.CenterWindow();

                GUI.Text("Select device");

                if(GUI.Combo("##device", ref selectedDevice, devices, devices.Length, new System.Numerics.Vector2(150, 30)))
                {
                    audioConfig.deviceId = (uint)selectedDevice;
                    audioConfig.dirty = true;
                }

                GUI.Text("Music Volume");

                float volume = audioManager.GetMusicVolume();

                if(GUI.SliderFloat("##MusicVolume", new System.Numerics.Vector2(100, 30), ref volume, 0.0f, 1.0f))
                {
                    audioManager.SetMusicVolume(volume);
                }

                if(audioConfig.dirty)
                    GUI.Text("You have unsaved changes");

                var buttonSize = new System.Numerics.Vector2(100, 30);

                if(GUI.Button("Save", buttonSize, new System.Numerics.Vector2(-52, 0), audioConfig.dirty))
                {
                    if(audioConfig.dirty)
                    {
                        audioConfig.dirty = false;
                        AudioSettings.DeviceId = audioConfig.deviceId;
                        AudioSettings.Save("audiosettings.dat");
                        menuState = MenuState.Main;
                    }
                }

                GUI.SameLine();

                if(GUI.Button("Back", buttonSize, new System.Numerics.Vector2(52, 0)))
                {
                    if(audioConfig.dirty)
                    {
                        audioConfig.dirty = false;
                        selectedDevice = -1;
                    }
                    menuState = MenuState.Main;
                }
                
                GUI.EndWindow();
            }
        }

        private void ShowQuitMenu()
        {
            if(GUI.BeginWindow("Quit Menu", new System.Numerics.Vector2(250, 150)))
            {
                GUI.CenterWindow();

                GUI.Text("Are you sure you want to quit?");

                var buttonSize = new System.Numerics.Vector2(100, 30);

                if(GUI.Button("Yes", buttonSize, new System.Numerics.Vector2(-52, 0)))
                    Application.Quit();

                GUI.SameLine();

                if(GUI.Button("No", buttonSize, new System.Numerics.Vector2(52, 0)))
                    menuState = MenuState.Main;

                GUI.EndWindow();
            }
        }

        private void RenderText(string text, float fontSize, Color color, OpenTK.Mathematics.Vector2 position)
        {
            var size = new OpenTK.Mathematics.Vector2();
            font.CalculateBounds(text, text.Length, fontSize, out size.X, out size.Y);
            var viewport = Graphics.GetViewport();
            position.X = viewport.width - size.X - position.X;
            position.Y = viewport.height - size.Y - position.Y;
            Graphics2D.AddText(position, font, text, fontSize, color);
        }

        private void SetCursor()
        {
            if(menuState == MenuState.None)
            {
                Input.SetMouseCursor(false);
            }
            else
            {
                Input.SetMouseCursor(true);
            }
        }

        private void SetMenuAudio()
        {
            if(menuState == MenuState.None)
            {
                audioManager.StopFireSound();
            }
            else
            {
                audioManager.PlayFireSound();
            }
        }
    }
}