using Gowtu;

namespace GowtuApp
{
    public class AudioManager : GameBehaviour
    {
        private AudioSource[] audioSources;
        private AudioClip clipDrone;
        private AudioClip clipClick1;
        private AudioClip clipClick2;
        private AudioClip clipFire;
        private FadeInEffect fadeInEffect;
        
        private void Start()
        {
            audioSources = new AudioSource[4];

            for(int i = 0; i < audioSources.Length; i++)
                audioSources[i] = gameObject.AddComponent<AudioSource>();

            clipDrone = Resources.FindAudioClip("Resources/Audio/unhappy-drone-67284.wav");
            clipClick1 = Resources.FindAudioClip("Resources/Audio/click1.mp3");
            clipClick2 = Resources.FindAudioClip("Resources/Audio/click2.mp3");
            clipFire = Resources.FindAudioClip("Resources/Audio/Fire.wav");

            if(clipDrone != null)
            {
                audioSources[0].Volume = 0.25f;
                audioSources[0].Loop = true;
                audioSources[0].Play(clipDrone);
            }
            
            fadeInEffect = new FadeInEffect(4.0f);

            audioSources[3].Loop = true;
            audioSources[3].AddEffect(fadeInEffect);

            ImGuiEx.MouseClick += OnMouseClick;
            ImGuiEx.MouseEnter += OnMouseEnter;
        }

        private void OnMouseClick(long id)
        {
            if(clipClick1 != null)
            {
                audioSources[1].Stop();
                audioSources[1].Cursor = 0;
                audioSources[1].Play(clipClick1);
            }
        }

        private void OnMouseEnter(long id)
        {
            if(clipClick2 != null)
            {
                audioSources[2].Stop();
                audioSources[2].Cursor = 0;
                audioSources[2].Play(clipClick2);
            }
        }

        public float GetMusicVolume()
        {
            return audioSources[0].Volume;
        }

        public void SetMusicVolume(float volume)
        {
            audioSources[0].Volume = volume;
        }

        public float GetFXVolume()
        {
            return audioSources[1].Volume;
        }

        public void SetFXVolume(float volume)
        {
            audioSources[1].Volume = volume;
            audioSources[2].Volume = volume;
        }

        public void PlayFireSound()
        {            
            audioSources[3].Play(clipFire);
        }

        public void StopFireSound()
        {
            audioSources[3].Stop();
            fadeInEffect.Reset();
        }
    }
}