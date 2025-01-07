// MIT License

// Copyright (c) 2025 W.M.R Jap-A-Joe

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MiniAudioEx;
using MiniAudioEx.Core;

namespace Gowtu
{
    /// <summary>
    /// This class is used to play audio.
    /// </summary>
    public sealed class AudioSource : Component
    {
        /// <summary>
        /// Callback handler for when an AudioClip is loaded using the 'Play(AudioClip clip)' method.
        /// </summary>
        public event AudioLoadEvent Load;
        /// <summary>
        /// Callback handler for when the playback has finished. This does not trigger if the AudioSource is set to Loop.
        /// </summary>
        public event AudioEndEvent End;
        /// <summary>
        /// Callback handler for implementing custom effects.
        /// </summary>
        public event AudioProcessEvent Process;
        /// <summary>
        /// Callback handler for generating procedural audio when using the 'Play()' method.
        /// </summary>
        public event AudioReadEvent Read;

        private IntPtr handle;
        private ma_sound_load_proc loadCallback;
        private ma_sound_end_proc endCallback;
        private ma_sound_process_proc processCallback;
        private ma_waveform_proc waveformCallback;
        private ConcurrentQueue<int> endEventQueue;
        private ConcurrentList<IAudioEffect> effects;
        private ConcurrentList<IAudioGenerator> generators;

        /// <summary>
        /// Gets a handle to the native ma_audio_source instance.
        /// </summary>
        /// <value></value>
        internal IntPtr Handle
        {
            get
            {
                return handle;
            }
        }

        /// <summary>
        /// Gets or sets the current position of the playback cursor in PCM samples.
        /// </summary>
        /// <value></value>
        public UInt64 Cursor
        {
            get
            {
                return Library.ma_ex_audio_source_get_pcm_position(handle);
            }
            set
            {
                Library.ma_ex_audio_source_set_pcm_position(handle, value);
            }
        }

        /// <summary>
        /// Gets the length of the playing audio clip in PCM samples.
        /// </summary>
        /// <value></value>
        public UInt64 Length
        {
            get
            {
                return Library.ma_ex_audio_source_get_pcm_length(handle);
            }
        }

        /// <summary>
        /// Gets or sets the volume of the sound.
        /// </summary>
        /// <value></value>
        public float Volume
        {
            get
            {
                return Library.ma_ex_audio_source_get_volume(handle);
            }
            set
            {
                Library.ma_ex_audio_source_set_volume(handle, value);
            }
        }

        /// <summary>
        /// Gets or sets the pitch of the sound.
        /// </summary>
        /// <value></value>
        public float Pitch
        {
            get
            {
                return Library.ma_ex_audio_source_get_pitch(handle);
            }
            set
            {
                Library.ma_ex_audio_source_set_pitch(handle, value);
            }
        }

        /// <summary>
        /// Gets or sets whether the audio should loop. If true, then the 'End' event will not be called. If the Play() overload is used, this property has no effect because the audio will loop regardless.
        /// </summary>
        /// <value></value>
        public bool Loop
        {
            get
            {
                return Library.ma_ex_audio_source_get_loop(handle) > 0;
            }
            set
            {
                Library.ma_ex_audio_source_set_loop(handle, value ? (uint)1 : 0);
            }
        }

        /// <summary>
        /// Gets or sets whether this source has spatial audio enabled or not.
        /// </summary>
        /// <value></value>
        public bool Spatial
        {
            get
            {
                return Library.ma_ex_audio_source_get_spatialization(handle) > 0;
            }
            set
            {
                Library.ma_ex_audio_source_set_spatialization(handle, value ? (uint)1 : 0);
            }
        }

        /// <summary>
        /// Gets or sets the intensity or strength of the simulated Doppler effect applied to the audio if Spatial is set to true.
        /// </summary>
        /// <value></value>
        public float DopplerFactor
        {
            get
            {
                return Library.ma_ex_audio_source_get_doppler_factor(handle);
            }
            set
            {
                Library.ma_ex_audio_source_set_doppler_factor(handle, value);
            }
        }

        /// <summary>
        /// Gets or sets the distance from the audio source at which the volume of the audio starts to attenuate. Sounds closer to or within this minimum distance are heard at full volume without any attenuation applied.
        /// </summary>
        /// <value></value>
        public float MinDistance
        {
            get
            {
                return Library.ma_ex_audio_source_get_min_distance(handle);
            }
            set
            {
                Library.ma_ex_audio_source_set_min_distance(handle, value);
            }
        }

        /// <summary>
        /// Gets or sets the distance from the audio source beyond which the audio is no longer audible or significantly attenuated. Sounds beyond this maximum distance are either inaudible or heard at greatly reduced volume.
        /// </summary>
        /// <value></value>
        public float MaxDistance
        {
            get
            {
                return Library.ma_ex_audio_source_get_max_distance(handle);
            }
            set
            {
                Library.ma_ex_audio_source_set_max_distance(handle, value);
            }
        }

        /// <summary>
        /// Gets or sets the mathematical model used to simulate the attenuation of sound over distance.
        /// </summary>
        /// <value></value>
        public AttenuationModel AttenuationModel
        {
            get
            {
                return (AttenuationModel)Library.ma_ex_audio_source_get_attenuation_model(handle);
            }
            set
            {
                Library.ma_ex_audio_source_set_attenuation_model(handle, (ma_attenuation_model)value);
            }
        }

        /// <summary>
        /// Indicates whether the source is playing audio or not.
        /// </summary>
        /// <value></value>
        public bool IsPlaying
        {
            get
            {
                return Library.ma_ex_audio_source_get_is_playing(handle) > 0;
            }
        }

        public AudioSource() : base()
        {

        }

        internal override void OnInitializeComponent()
        {
            handle = Library.ma_ex_audio_source_init(Audio.NativeContext);

            if(handle != IntPtr.Zero)
            {
                endEventQueue = new ConcurrentQueue<int>();
                effects = new ConcurrentList<IAudioEffect>();
                generators = new ConcurrentList<IAudioGenerator>();

                loadCallback = OnLoad;
                endCallback = OnEnd;
                processCallback = OnProcess;
                waveformCallback = OnWaveform;

                ma_ex_audio_source_callbacks callbacks = new ma_ex_audio_source_callbacks();
                callbacks.pUserData = handle;
                callbacks.processCallback = processCallback;
                callbacks.endCallback = endCallback;
                callbacks.loadCallback = loadCallback;
                callbacks.waveformCallback = waveformCallback;

                Library.ma_ex_audio_source_set_callbacks(handle, callbacks);
                
                Audio.Add(this);
            }
        }

        internal override void OnDestroyComponent()
        {
            Audio.Remove(this);
        }

        internal void Destroy()
        {
            if(handle != IntPtr.Zero)
            {
                Library.ma_ex_audio_source_stop(handle);
                Library.ma_ex_audio_source_uninit(handle);
                handle = IntPtr.Zero;

                //Clear the queues (netstandard2.0 does not have a Clear method for ConcurrentQueue)
                while(endEventQueue.Count > 0)
                    endEventQueue.TryDequeue(out _);

                for(int i = 0; i < effects.Count; i++)
                    effects[i].OnDestroy();

                for(int i = 0; i < generators.Count; i++)
                    generators[i].OnDestroy();

                effects.Clear();
                generators.Clear();
            }
        }

        /// <summary>
        /// Use this method if you registered a method to the Read callback to generate audio.
        /// </summary>
        public void Play()
        {
            Library.ma_ex_audio_source_play(handle);
        }

        /// <summary>
        /// Plays an AudioClip by given filepath or encoded data buffer (data must be either WAV/MP3/FLAC).
        /// </summary>
        /// <param name="clip">The AudioClip to play.</param>
        public void Play(AudioClip clip)
        {
            if(clip.Handle != IntPtr.Zero)
                Library.ma_ex_audio_source_play_from_memory(handle, clip.Handle, clip.DataSize);
            else
                Library.ma_ex_audio_source_play_from_file(handle, clip.FilePath, clip.StreamFromDisk ? (uint)1 : 0);
        }

        /// <summary>
        /// Stop the AudioSource playback, note that this method does not set the cursor back to 0 so it could be used as a pause method.
        /// </summary>
        public void Stop()
        {
            Library.ma_ex_audio_source_stop(handle);
        }

        internal void Update()
        {
            if(endEventQueue.Count > 0)
            {
                while(endEventQueue.TryDequeue(out _))
                {
                    End?.Invoke();
                }
            }
        }

        /// <summary>
        /// A thread safe method to add an IAudioEffect.
        /// </summary>
        /// <param name="effect"></param>
        public void AddEffect(IAudioEffect effect)
        {
            effects.Add(effect);
        }

        /// <summary>
        /// A thread safe method to remove an IAudioEffect.
        /// </summary>
        /// <param name="effect"></param>
        public void RemoveEffect(IAudioEffect effect)
        {
            effects.Remove(effect);
        }

        /// <summary>
        /// A thread safe method to remove an IAudioEffect by its index.
        /// </summary>
        /// <param name="effect"></param>
        public void RemoveEffect(int index)
        {
            if(index >= 0 && index < effects.Count)
            {
                var target = effects[index];
                effects.Remove(target);
            }
        }

        /// <summary>
        /// A thread safe method to remove all IAudioEffect instances.
        /// </summary>
        public void RemoveEffects()
        {
            List<IAudioEffect> targets = new List<IAudioEffect>();
            for(int i = 0; i < effects.Count; i++)
            {
                targets.Add(effects[i]);
            }
            if(targets.Count > 0)
            {
                effects.Remove(targets);
            }
        }

        /// <summary>
        /// A thread safe method to add an IAudioGenerator.
        /// </summary>
        /// <param name="effect"></param>
        public void AddGenerator(IAudioGenerator generator)
        {
            generators.Add(generator);
        }

        /// <summary>
        /// A thread safe method to remove an IAudioGenerator.
        /// </summary>
        /// <param name="effect"></param>
        public void RemoveGenerator(IAudioGenerator generator)
        {
            generators.Remove(generator);
        }

        /// <summary>
        /// A thread safe method to remove an IAudioGenerator by its index.
        /// </summary>
        /// <param name="effect"></param>
        public void RemoveGenerator(int index)
        {
            if(index >= 0 && index < generators.Count)
            {
                var target = generators[index];
                generators.Remove(target);
            }
        }

        /// <summary>
        /// A thread safe method to remove all IAudioGenerator instances.
        /// </summary>
        public void RemoveGenerators()
        {
            List<IAudioGenerator> targets = new List<IAudioGenerator>();
            for(int i = 0; i < generators.Count; i++)
            {
                targets.Add(generators[i]);
            }
            if(targets.Count > 0)
            {
                generators.Remove(targets);
            }
        }

        /// <summary>
        /// Called whenever audio is loaded using the 'Play' method.
        /// </summary>
        /// <param name="pUserData"></param>
        /// <param name="pSound"></param>
        private void OnLoad(IntPtr pUserData, IntPtr pSound)
        {
            Load?.Invoke();
        }

        /// <summary>
        /// Called whenever audio has finished playing. This does not trigger when 'Loop' is true.
        /// </summary>
        /// <param name="pUserData"></param>
        /// <param name="pSound"></param>
        private void OnEnd(IntPtr pUserData, IntPtr pSound)
        {
            //This callback is called from another thread so we move the message to a queue that the main thread can safely access
            //If the audio is set to looping, this event is never triggered
            endEventQueue.Enqueue(1);            
        }
        
        /// <summary>
        /// Called whenever the audio buffer of this source is filled with data.
        /// </summary>
        /// <param name="pUserData"></param>
        /// <param name="pSound"></param>
        /// <param name="pFramesOut"></param>
        /// <param name="frameCount"></param>
        /// <param name="channels"></param>
        private void OnProcess(IntPtr pUserData, IntPtr pSound, IntPtr pFramesOut, UInt64 frameCount, UInt32 channels)
        {
            int length = (int)(frameCount * channels);

            AudioBuffer<float> framesOut = new AudioBuffer<float>(pFramesOut, length);

            for(int i = 0; i < effects.Count; i++)
            {
                effects[i].OnProcess(framesOut, frameCount, (int)channels);
            }

            Process?.Invoke(framesOut, frameCount, (int)channels);
        }

        /// <summary>
        /// Called whenever the buffer of this source needs data. This is only called whenever the 'Play' method without parameters is called.
        /// </summary>
        /// <param name="pUserData"></param>
        /// <param name="pFramesOut"></param>
        /// <param name="frameCount"></param>
        /// <param name="channels"></param>
        private void OnWaveform(IntPtr pUserData, IntPtr pFramesOut, UInt64 frameCount, UInt32 channels)
        {
            int length = (int)(frameCount * channels);            

            AudioBuffer<float> framesOut = new AudioBuffer<float>(pFramesOut, length);

            for(int i = 0; i < generators.Count; i++)
            {
                generators[i].OnGenerate(framesOut, frameCount, (int)channels);
            }

            Read?.Invoke(framesOut, frameCount, (int)channels);
        }
    }

    // /// <summary>
    // /// An interface for implementing audio effects. These effects can be added to an AudioSource by using the AddEffect method.
    // /// </summary>
    // public interface IAudioEffect
    // {
    //     void OnProcess(AudioBuffer<float> framesOut, UInt64 frameCount, Int32 channels);
    //     void OnDestroy();
    // }

    // /// <summary>
    // /// An interface for implementing audio generators. These generators can be added to an AudioSource by using the AddGenerator method.
    // /// </summary>
    // public interface IAudioGenerator
    // {
    //     void OnGenerate(AudioBuffer<float> framesOut, UInt64 frameCount, Int32 channels);
    //     void OnDestroy();
    // }
}